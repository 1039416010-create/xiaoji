"""Reference-aligned procedural WIP. Does not claim manual retopology or final animation."""
import argparse
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True

import bpy
import bmesh
import numpy as np
from mathutils import Matrix, Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
import build_phase11a_benchmark as base
import advance_phase11a_wip_v2 as motion

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'VisualV3'
PALETTES = {
    'flash': ['EDBE48', 'F4E4BD', 'C83235', '3976AD'],
    'chubby': ['E9C783', 'F4E6C6', 'BA4939', '428B7B'],
    'slacker': ['737D8B', 'BCB0A0', 'C3724C', '342E2D'],
}


def linear(hex_color):
    values = [int(hex_color[i:i+2], 16) / 255 for i in (0, 2, 4)]
    return tuple(v / 12.92 if v <= .04045 else ((v + .055) / 1.055) ** 2.4 for v in values) + (1,)


def material(name, color, roughness=.6, texture=False, metallic=0):
    mat = base.material(name, linear(color), metallic, roughness)
    mat['roughness'] = roughness
    mat['metallic'] = metallic
    if texture:
        nodes, links = mat.node_tree.nodes, mat.node_tree.links
        shader = next(n for n in nodes if n.type == 'BSDF_PRINCIPLED')
        tex = nodes.new('ShaderNodeTexNoise')
        tex.inputs['Scale'].default_value = 85
        tex.inputs['Detail'].default_value = 2
        bump = nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .18
        bump.inputs['Distance'].default_value = .012
        links.new(tex.outputs['Fac'], bump.inputs['Height'])
        links.new(bump.outputs['Normal'], shader.inputs['Normal'])
    return mat


def assign(obj, mat, bone, tag):
    obj.data.materials.append(mat)
    obj.vertex_groups.new(name=bone).add(list(range(len(obj.data.vertices))), 1, 'REPLACE')
    obj.vertex_groups.new(name='part_' + tag).add(list(range(len(obj.data.vertices))), 1, 'REPLACE')
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    return obj


def mesh(name, vertices, faces, mat, bone):
    data = bpy.data.meshes.new(name)
    data.from_pydata(vertices, [], faces)
    data.update()
    bm=bmesh.new(); bm.from_mesh(data)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(data); bm.free()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return assign(obj, mat, bone, name)


def sphere(name, pos, scale, mat, bone, segments=28, rings=18):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=pos)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return assign(obj, mat, bone, name)


def feather(name, start, end, width, mat, bone, bend=0):
    # Closed, tapered volume; no transparency cards. Front ridge catches light.
    start, end = Vector(start), Vector(end)
    tangent = (end-start).normalized()
    side = tangent.cross(Vector((0, -1, 0))).normalized()
    if side.length < .01:
        side = Vector((1, 0, 0))
    normal = side.cross(tangent).normalized()
    verts, faces = [], []
    rows, cols = 12, 10
    for row in range(rows+1):
        t = row/rows
        center = start.lerp(end, t) + Vector((0, bend * math.sin(t*math.pi), 0))
        envelope = max(.009, math.sin(math.pi * t) ** .72) * (1-.30*t)
        for col in range(cols):
            angle = col*2*math.pi/cols
            point = center + side*(width*envelope*math.cos(angle)) + normal*(width*.27*envelope*math.sin(angle))
            verts.append(point)
    for row in range(rows):
        for col in range(cols):
            a = row*cols+col
            b = row*cols+(col+1)%cols
            faces.append((a, b, b+cols, a+cols))
    faces += [tuple(reversed(range(cols))), tuple(rows*cols+i for i in range(cols))]
    return mesh(name, verts, faces, mat, bone)


def band(name, center, rx, ry, height, mat, bone):
    verts, faces = [], []
    # A thin closed ribbon rather than a flattened sphere through the face.
    for radius_offset, zoff in ((0,-height/2),(0,height/2),(.015,height/2),(.015,-height/2)):
        for i in range(64):
            angle = i*2*math.pi/64
            verts.append((center[0]+(rx+radius_offset)*math.cos(angle), center[1]+(ry+radius_offset)*math.sin(angle), center[2]+zoff))
    for r in range(4):
        for i in range(64):
            faces.append((r*64+i,r*64+(i+1)%64,((r+1)%4)*64+(i+1)%64,((r+1)%4)*64+i))
    return mesh(name, verts, faces, mat, bone)


def continuous_body(profile, mats):
    bx, by, bz = profile['body']
    hz = profile['head_z']
    hx, hy, head_height = profile['head']
    bottom = profile['body_z']-bz
    # Cross-section loft: continuous torso, neck and head, with joint rings.
    stations = [
        (bottom,.02,.02,0), (bottom+.12,bx*.57,by*.58,0),
        (profile['body_z']-.25,bx*.96,by*.97,0),
        (profile['body_z'],bx,by,0),
        (profile['body_z']+.32,bx*.82,by*.81,0),
        (hz-.39,hx*.67,hy*.67,-.01),
        (hz-.24,hx*.90,hy*.89,-.03),
        (hz,hx,hy,-.03), (hz+.25,hx*.88,hy*.88,-.01),
        (hz+head_height*.91,hx*.4,hy*.4,0), (hz+head_height,.015,.015,0),
    ]
    stations.sort()
    # Catmull-Rom interpolation smooths the designed profile without subdividing
    # disconnected primitives into a fake high-poly surface.
    samples=[]
    for i in range(len(stations)-1):
        p0=np.array(stations[max(0,i-1)])
        p1=np.array(stations[i]); p2=np.array(stations[i+1]); p3=np.array(stations[min(len(stations)-1,i+2)])
        for j in range(4):
            t=j/4
            samples.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t))
    samples.append(np.array(stations[-1]))
    verts, faces = [], []
    cols=64
    for z,rx,ry,cy in samples:
        for i in range(cols):
            angle=i*2*math.pi/cols
            verts.append((max(.008,rx)*math.cos(angle),cy+max(.008,ry)*math.sin(angle),z))
    for row in range(len(samples)-1):
        for i in range(cols):
            faces.append((row*cols+i,row*cols+(i+1)%cols,(row+1)*cols+(i+1)%cols,(row+1)*cols+i))
    faces.extend([tuple(reversed(range(cols))),tuple((len(samples)-1)*cols+i for i in range(cols))])
    torso_mat=mats['body'].copy(); torso_mat.name='TorsoGradient'
    obj=mesh('TorsoNeckHead',verts,faces,torso_mat,'Body_01')
    tint=obj.data.color_attributes.new(name='Tint',type='FLOAT_COLOR',domain='POINT')
    c0=np.array(mats['body'].diffuse_color); c1=np.array(mats['cream'].diffuse_color)
    for v in obj.data.vertices:
        # Smooth feather-color boundary across the chest; avoids polygon steps.
        x,y,z=v.co
        side=max(0,min(1,(.66-abs(x)/bx)/.15))
        front=max(0,min(1,(-y-.03)/.10))
        top=max(0,min(1,(hz-.20-z)/.15))
        blend=side*front*top
        tint.data[v.index].color=tuple(c0*(1-blend)+c1*blend)
    color_node=torso_mat.node_tree.nodes.new('ShaderNodeVertexColor'); color_node.layer_name='Tint'
    shader=next(n for n in torso_mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    torso_mat.node_tree.links.new(color_node.outputs['Color'],shader.inputs['Base Color'])
    anchors=[(profile['body_z']-.3,'Body_01'),(profile['body_z']+.28,'Body_02'),(hz-.36,'Neck_01'),(hz-.21,'Neck_02'),(hz-.10,'Head')]
    anchors.sort()
    for group in list(obj.vertex_groups):
        if not group.name.startswith('part_'):
            obj.vertex_groups.remove(group)
    groups={n:obj.vertex_groups.new(name=n) for _,n in anchors}
    for v in obj.data.vertices:
        z=v.co.z
        if z<=anchors[0][0]:
            groups[anchors[0][1]].add([v.index],1,'REPLACE')
        elif z>=anchors[-1][0]:
            groups[anchors[-1][1]].add([v.index],1,'REPLACE')
        else:
            for (a,an),(b,bn) in zip(anchors,anchors[1:]):
                if a<=z<=b:
                    t=(z-a)/(b-a)
                    groups[an].add([v.index],1-t,'REPLACE'); groups[bn].add([v.index],t,'REPLACE')
                    break
    return obj


def lid(side, center, mat, angle):
    # Spherical eyelid covering the upper eye. Shape keys move only this shell.
    verts, faces=[],[]
    for row in range(9):
        theta=.012+(angle-.012)*row/8
        for col in range(32):
            phi=col*2*math.pi/32
            verts.append((center[0]+.181*math.sin(theta)*math.cos(phi), center[1]+.190*math.sin(theta)*math.sin(phi), center[2]+.216*math.cos(theta)))
    for row in range(8):
        for col in range(32):
            a=row*32+col; b=row*32+(col+1)%32
            faces.append((a,b,b+32,a+32))
    return mesh('Lid_'+side,verts,faces,mat,'Head')


def join(parts,name,rig):
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts: part.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    obj=bpy.context.object; obj.name=name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    obj.parent=rig
    mod=obj.modifiers.new('Skin','ARMATURE'); mod.object=rig
    return obj


def ids(obj,tag):
    group=obj.vertex_groups.get('part_'+tag)
    return [] if group is None else [v.index for v in obj.data.vertices if any(g.group==group.index and g.weight>.5 for g in v.groups)]


def expression_shapes(body, eye_centers, lid_angle):
    basis=body.shape_key_add(name='Basis',from_mix=False)
    def shape(name):
        k=body.shape_key_add(name=name,from_mix=False)
        for i,v in enumerate(basis.data): k.data[i].co=v.co
        k.value=0
        return k
    for side in ('L','R'):
        key=shape('Blink_'+side)
        center=eye_centers[side]
        for j,index in enumerate(ids(body,'Lid_'+side)):
            row,col=divmod(j,32); theta=.012+(math.pi-.018)*row/8; phi=col*2*math.pi/32
            key.data[index].co=(center[0]+.181*math.sin(theta)*math.cos(phi),center[1]+.190*math.sin(theta)*math.sin(phi),center[2]+.216*math.cos(theta))
    for name in ('Smile','Shock','Determined','Sad','Dizzy','Cheek_Puff','Beak_Corner_Up'):
        key=shape(name)
        for side in ('L','R'):
            sign=-1 if side=='L' else 1
            if name in ('Smile','Beak_Corner_Up','Shock') and side=='L':
                for i in ids(body,'BeakLower'):
                    key.data[i].co.z += -.10 if name=='Shock' else .028*min(1,abs(basis.data[i].co.x)/.18)
            if name in ('Sad','Determined'):
                for i in ids(body,'Brow_'+side):
                    key.data[i].co.z += (.16 if name=='Sad' else -.13)*sign*(basis.data[i].co.x-eye_centers[side][0])
            if name=='Dizzy':
                for tag in ('Iris_','Pupil_','Glint_'):
                    for i in ids(body,tag+side): key.data[i].co.x-=sign*.045
            if name=='Cheek_Puff':
                for i in ids(body,'Cheek_'+side):
                    key.data[i].co.x+=sign*.028; key.data[i].co.y-=.024
    return body


def create_character(cid):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.world=bpy.data.worlds.new('World')
    bpy.context.preferences.filepaths.save_version=0
    bpy.context.preferences.filepaths.file_preview_type='NONE'
    p=base.PROFILES[cid].copy(); palette=PALETTES[cid]
    rig=base.make_armature(p,'CH_'+cid)
    for bone in rig.data.bones: bone.use_deform=True
    mats={
        'body':material('Feathers',palette[0],texture=True),
        'cream':material('Chest',palette[1],texture=True),
        'accent':material('Cloth',palette[2],texture=True),
        'detail':material('Detail',palette[3],texture=True),
        'beak':material('Beak','EBA13C',.4),
        'white':material('EyeWhite','FFF5DF',.28),
        'iris':material('Iris','975129',.23),
        'black':material('Pupil','17151B',.17),
        'red':material('Comb','BA4939',.62),
        'brow':material('Brow','664529',.66),
        'pink':material('Cheek','EDB382',.7),
        'glass':material('GoggleGlass','26333E',.13,.0,.22),
        'rim':material('GoggleRim','8E7D62',.35,False,.6),
    }
    parts=[continuous_body(p,mats)]; accessories=[]
    bx,by,bz=p['body']; hx,hy,hz=p['head']; z=p['head_z']; body_z=p['body_z']
    parts += [sphere('BeakUpper',(0,-hy-.09,z-.02),(.205,.245,.105),mats['beak'],'Beak_Upper'),sphere('BeakLower',(0,-hy-.09,z-.135),(.16,.17,.062),mats['beak'],'Beak_Lower')]
    eyes={}; lid_angle=1.48 if cid=='slacker' else .82
    for side,sign in (('L',-1),('R',1)):
        ex=sign*hx*.47; ey=-hy+.012; ez=z+.135
        eyes[side]=(ex,ey,ez)
        parts.append(sphere('Eye_'+side,(ex,ey,ez),(.172,.127,.203),mats['white'],'Eye_'+side))
        parts.append(sphere('Iris_'+side,(ex,ey-.124,ez-.006),(.105,.028,.128),mats['iris'],'Eye_'+side))
        parts.append(sphere('Pupil_'+side,(ex,ey-.147,ez-.006),(.064,.021,.092),mats['black'],'Eye_'+side))
        parts.append(sphere('Glint_'+side,(ex-.021,ey-.169,ez+.04),(.022,.008,.027),mats['white'],'Eye_'+side,16,10))
        parts.append(lid(side,eyes[side],mats['body'] if cid!='slacker' else mats['cream'],lid_angle))
        parts.append(feather('Brow_'+side,(ex-sign*.14,ey-.055,ez+.21),(ex+sign*.13,ey-.055,ez+.25),.052,mats['brow'],'Head'))
        parts.append(sphere('Cheek_'+side,(sign*hx*.7,-hy*.7,z-.12),(.16,.08,.105),mats['cream'] if cid!='chubby' else mats['pink'],'Head',24,14))
        # Wing root and primary feathers form an overlapping fan.
        parts.append(sphere('WingRoot_'+side,(sign*bx*.84,.02,body_z+.13),(.22,by*.6,bz*.42),mats['body'],'Wing_'+side+'_Shoulder'))
        for j in range(6):
            parts.append(feather('Wing_'+side+str(j),(sign*(bx*.81+j*.025),-.13+j*.065,body_z+.22),(sign*(bx+.19+j*.033),-.17+j*.10,body_z-.44-j*.055),.115,mats['cream'] if j>3 else mats['body'],'Wing_'+side+'_Elbow',.03))
        # Three front toes and rear toe, with nail tips; tags keep editing easy.
        lx=sign*p['leg_x']; top=p['leg_top']
        base.add_cylinder('Shin_'+side,(lx,0,top),(lx,-.02,.12),.055,mats['beak'],'Leg_'+side+'_Knee',parts)
        parts.append(sphere('Ankle_'+side,(lx,-.015,.115),(.075,.09,.08),mats['beak'],'Leg_'+side+'_Ankle',20,12))
        for j in range(3):
            end=(lx+(j-1)*.125,-.27+abs(j-1)*.04,.055)
            bone='Leg_'+side+('_Toe_A' if j==1 else '_Toe_B')
            base.add_cylinder('Toe_'+side+str(j),(lx,-.02,.09),end,.034,mats['beak'],bone,parts)
            parts.append(feather('Claw_'+side+str(j),end,(end[0],end[1]-.07,.035),.027,mats['cream'],bone))
        # Small contour feathers at cheek and shoulder, not a carpet of cards.
        for j in range(3):
            parts.append(feather('FaceTuft_'+side+str(j),(sign*hx*.79,.01+j*.05,z-.08),(sign*(hx+.10+j*.025),.07+j*.08,z-.30-j*.06),.095,mats['body'],'Head'))
        if cid=='flash':
            vertices=[(sign*(bx*.94+.10),-.27,body_z+.29),(sign*(bx*.94+.035),-.28,body_z+.01),(sign*(bx*.94+.13),-.28,body_z+.075),(sign*(bx*.94+.08),-.28,body_z-.23),(sign*(bx*.94+.26),-.27,body_z+.15),(sign*(bx*.94+.16),-.27,body_z+.10)]
            parts.append(mesh('Lightning_'+side,vertices,[(0,1,2,3,4,5)],mats['detail'],'Wing_'+side+'_Shoulder'))
    for j in range(7):
        offset=(j-3)*.09
        length=.38 if cid=='chubby' else .65
        parts.append(feather('Tail'+str(j),(offset*.4,by*.7,body_z+.14),(offset,by+length,body_z+.62-abs(offset)*.7),.12,mats['cream'] if j%3==0 else mats['body'],('TailFan_L','TailFan_C','TailFan_R')[min(2,j//3)],-.06))
    for j in range(5 if cid!='slacker' else 8):
        if cid=='chubby':
            parts.append(sphere('Comb'+str(j),(0,-.07+j*.095,z+hz-.04+math.sin((j+1)*math.pi/6)*.17),(.085,.105,.22-j*.02),mats['red'],'Comb_01',24,16))
        else:
            start=((j%3-1)*.055,.0,z+hz*.80)
            end=((j%3-1)*.17,.12+j*.065,z+hz+.20+(j%3)*.09)
            parts.append(feather('Crest'+str(j),start,end,.10,mats['cream'] if cid=='slacker' and j%3==0 else mats['body'],'Comb_01',.05))
    if cid=='flash':
        accessories.append(band('Headband',(0,-.005,z+.32),hx*.86,hy*.86,.12,mats['accent'],'AccessorySocket_Head'))
        for j in range(2):
            accessories.append(feather('HeadbandTail'+str(j),(hx*.7,.20,z+.3),(hx+.40+j*.12,.48+j*.15,z+.03-j*.20),.095,mats['accent'],'AccessorySocket_Head',.09))
    elif cid=='chubby':
        accessories.append(band('Collar',(0,0,z-.34),hx*.85,hy*.86,.06,mats['detail'],'AccessorySocket_Chest'))
        for side,sign in (('L',-1),('R',1)):
            accessories.append(sphere('Bow'+side,(sign*.17,-hy*.98,z-.32),(.18,.075,.13),mats['detail'],'AccessorySocket_Chest'))
        accessories.append(sphere('BowKnot',(0,-hy*1.06,z-.32),(.08,.08,.105),mats['detail'],'AccessorySocket_Chest'))
    else:
        accessories.append(band('Scarf',(0,0,z-.33),hx*.85,hy*.89,.13,mats['accent'],'AccessorySocket_Chest'))
        accessories.append(feather('ScarfTail',(.3,-.22,z-.30),(.40,-.37,z-.81),.12,mats['accent'],'AccessorySocket_Chest',-.09))
        accessories.append(band('GoggleStrap',(0,0,z+.32),hx*.89,hy*.87,.065,mats['detail'],'AccessorySocket_Head'))
        for side,sign in (('L',-1),('R',1)):
            accessories.append(sphere('GoggleRim'+side,(sign*.22,-hy*.77,z+.36),(.225,.075,.155),mats['rim'],'AccessorySocket_Head'))
            accessories.append(sphere('GoggleLens'+side,(sign*.22,-hy*.77-.062,z+.36),(.198,.038,.125),mats['glass'],'AccessorySocket_Head'))
    # Keep independently editable source objects in a hidden authoring collection.
    source=bpy.data.collections.new('SOURCE_COMPONENTS'); bpy.context.scene.collection.children.link(source)
    source.hide_render=True; source.hide_viewport=True
    for obj in parts+accessories:
        copy=obj.copy(); copy.data=obj.data.copy(); source.objects.link(copy)
        copy.name='SRC_'+obj.name
    body=join(parts,'CH_'+cid+'_Body_LOD0',rig)
    accessory=join(accessories,'CH_'+cid+'_Accessories_LOD0',rig)
    expression_shapes(body,eyes,lid_angle)
    base.create_blocking_actions(rig,p['motion_scale'])
    motion.add_missing_actions(rig,cid)
    motion.reset_pose(rig)
    # Ensure unrelated channels cannot retain the preceding action's pose.
    for action in bpy.data.actions:
        rig.animation_data.action=action
        paths={c.data_path for c in motion.action_fcurves(action)}
        for pb in rig.pose.bones:
            if pb.name=='Root': continue
            for attr,zero in (('location',(0,0,0)),('rotation_euler',(0,0,0)),('scale',(1,1,1))):
                if pb.path_from_id(attr) not in paths:
                    setattr(pb,attr,zero); pb.keyframe_insert(attr,frame=1,group=pb.name)
    motion.reset_pose(rig)
    return rig,body,accessory


def bake_materials(cid,objects):
    scene=bpy.context.scene
    scene.render.engine='CYCLES'; scene.cycles.samples=4
    scene.render.bake.margin=8; scene.render.bake.use_clear=False
    scene.render.bake.use_pass_direct=False; scene.render.bake.use_pass_indirect=False; scene.render.bake.use_pass_color=True
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects: obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(65),island_margin=.006)
    bpy.ops.object.mode_set(mode='OBJECT')
    for obj in objects: obj.data.uv_layers.active.name='UV0'
    materials={mat for obj in objects for mat in obj.data.materials}
    textures={}
    for label,bake_type in (('BaseColor','DIFFUSE'),('Normal','NORMAL'),('Mask','EMIT')):
        img=bpy.data.images.new('T_'+cid+'_'+label,2048,2048,alpha=True)
        img.colorspace_settings.name='sRGB' if label=='BaseColor' else 'Non-Color'
        output_links=[]
        for mat in materials:
            nodes,links=mat.node_tree.nodes,mat.node_tree.links
            tex=nodes.new('ShaderNodeTexImage'); tex.image=img; nodes.active=tex
            if label=='Mask':
                output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL')
                old=output.inputs['Surface'].links[0].from_socket
                emission=nodes.new('ShaderNodeEmission')
                emission.inputs['Color'].default_value=(mat['metallic'],mat['roughness'],0,1)
                links.new(emission.outputs[0],output.inputs['Surface'])
                output_links.append((mat,old,output,emission))
        bpy.ops.object.bake(type=bake_type)
        if label=='Mask':
            pixels=np.empty(2048*2048*4,dtype=np.float32); img.pixels.foreach_get(pixels)
            pixels=pixels.reshape(-1,4); pixels[:,3]=1-pixels[:,1]; pixels[:,1]=1; pixels[:,2]=0
            img.pixels.foreach_set(pixels.ravel()); img.update()
        path=OUT/'Textures'/('T_'+cid+'_'+label+'.png')
        img.filepath_raw=str(path); img.file_format='PNG'; img.save()
        for mat,old,output,emission in output_links:
            mat.node_tree.links.new(old,output.inputs['Surface']); mat.node_tree.nodes.remove(emission)
        textures[label]=img
    atlas=bpy.data.materials.new('MAT_'+cid+'_Atlas_WIP'); atlas.use_nodes=True
    nodes,links=atlas.node_tree.nodes,atlas.node_tree.links
    shader=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
    for label,img in textures.items():
        tex=nodes.new('ShaderNodeTexImage'); tex.image=img
        if label=='BaseColor': links.new(tex.outputs['Color'],shader.inputs['Base Color'])
        elif label=='Normal':
            norm=nodes.new('ShaderNodeNormalMap'); links.new(tex.outputs['Color'],norm.inputs['Color']); links.new(norm.outputs[0],shader.inputs['Normal'])
        else:
            separate=nodes.new('ShaderNodeSeparateColor'); links.new(tex.outputs['Color'],separate.inputs[0]); links.new(separate.outputs[0],shader.inputs['Metallic'])
            invert=nodes.new('ShaderNodeMath'); invert.operation='SUBTRACT'; invert.inputs[0].default_value=1
            links.new(tex.outputs['Alpha'],invert.inputs[1]); links.new(invert.outputs[0],shader.inputs['Roughness'])
    for obj in objects:
        obj.data.materials.clear(); obj.data.materials.append(atlas)
        for poly in obj.data.polygons: poly.material_index=0


def normalize(rig,objects,cid):
    factor=(1.08 if cid=='flash' else 1.0)/max(v.co.z for obj in objects for v in obj.data.vertices)
    matrix=Matrix.Scale(factor,4)
    for obj in objects: obj.data.transform(matrix,shape_keys=True)
    rig.data.transform(matrix)
    for action in bpy.data.actions:
        for curve in motion.action_fcurves(action):
            if curve.data_path.endswith('.location'):
                for point in curve.keyframe_points:
                    point.co.y*=factor; point.handle_left.y*=factor; point.handle_right.y*=factor
    return factor


def lod(obj,level,ratio):
    bpy.ops.object.select_all(action='DESELECT')
    copy=obj.copy(); copy.data=obj.data.copy(); bpy.context.collection.objects.link(copy)
    copy.name=obj.name.replace('LOD0','LOD'+str(level)); copy.select_set(True); bpy.context.view_layer.objects.active=copy
    motion.remove_shape_keys(copy)
    dec=copy.modifiers.new('Reduction','DECIMATE'); dec.ratio=ratio; dec.use_collapse_triangulate=True
    while copy.modifiers.find(dec.name)>0: bpy.ops.object.modifier_move_up(modifier=dec.name)
    bpy.ops.object.modifier_apply(modifier=dec.name)
    copy.hide_render=True; copy.hide_set(True)
    return copy


def ground_blocking(rig,objects):
    # COM location is in the bone's local basis, not Blender world XYZ. Bake
    # contact corrections through its pose matrix while keeping Root untouched.
    for action in list(bpy.data.actions):
        motion.reset_pose(rig); rig.animation_data.action=action
        a,b=map(int,action.frame_range)
        corrections=[]
        for frame in range(a,b+1):
            bpy.context.scene.frame_set(frame)
            depsgraph=bpy.context.evaluated_depsgraph_get(); depsgraph.update()
            minimum=1e9
            for obj in objects:
                evaluated=obj.evaluated_get(depsgraph); data=evaluated.to_mesh()
                coordinates=np.empty(len(data.vertices)*3,dtype=np.float32)
                data.vertices.foreach_get('co',coordinates)
                coords=coordinates.reshape(-1,3)
                matrix=np.array(evaluated.matrix_world)
                minimum=min(minimum,float((coords@matrix[2,:3]+matrix[2,3]).min()))
                evaluated.to_mesh_clear()
            com=rig.pose.bones['CenterOfMass']
            if minimum<.002:
                pose=com.matrix.copy(); pose.translation.z+=.002-minimum
                com.matrix=pose
            corrections.append((frame,tuple(com.location)))
        for frame,location in corrections:
            rig.pose.bones['CenterOfMass'].location=location
            rig.pose.bones['CenterOfMass'].keyframe_insert('location',frame=frame,group='CenterOfMass')
        for curve in motion.action_fcurves(action):
            if curve.data_path=='pose.bones["CenterOfMass"].location':
                for point in curve.keyframe_points: point.interpolation='LINEAR'
    motion.reset_pose(rig)


def render(cid,rig,body):
    if bpy.data.objects.get('PreviewCamera') is None:
        base.setup_preview_scene(cid,.46,.78)
    scene=bpy.context.scene
    scene.camera.location=(1.9,-3.5,1.65); base.look_at(scene.camera,(0,0,.56))
    scene.camera.data.type='ORTHO'; scene.camera.data.ortho_scale=1.45
    scene.render.resolution_x=1000; scene.render.resolution_y=1000
    scene.view_settings.view_transform='AgX'
    bpy.data.objects['KeyLight'].data.energy=585
    bpy.data.objects['FillLight'].data.energy=325
    for label,location in (('hero',(1.9,-3.5,1.4)),('side',(3.5,0,1.2)),('back',(1.9,3.5,1.4))):
        scene.camera.location=location; base.look_at(scene.camera,(0,0,.56))
        scene.render.filepath=str(OUT/'Preview'/f'CH_{cid}_{label}.png'); bpy.ops.render.render(write_still=True)
    scene.camera.location=(1.9,-3.5,1.4); base.look_at(scene.camera,(0,0,.56))
    body.data.shape_keys.key_blocks['Blink_L'].value=1
    body.data.shape_keys.key_blocks['Blink_R'].value=1
    scene.render.filepath=str(OUT/'Preview'/f'CH_{cid}_blink.png'); bpy.ops.render.render(write_still=True)
    body.data.shape_keys.key_blocks['Blink_L'].value=0; body.data.shape_keys.key_blocks['Blink_R'].value=0
    for label,action_name,frame in (('fall','Fall_Blocking',30),('sprint','Sprint_Blocking',8)):
        motion.reset_pose(rig); rig.animation_data.action=bpy.data.actions[action_name]
        scene.frame_set(frame)
        depsgraph=bpy.context.evaluated_depsgraph_get(); depsgraph.update()
        points=[]
        for obj in (body,bpy.data.objects['CH_'+cid+'_Accessories_LOD0']):
            evaluated=obj.evaluated_get(depsgraph); evaluated_mesh=evaluated.to_mesh()
            points.extend(evaluated.matrix_world@v.co for v in evaluated_mesh.vertices)
            evaluated.to_mesh_clear()
        center=Vector([(min(v[i] for v in points)+max(v[i] for v in points))*.5 for i in range(3)])
        scene.camera.location=center+Vector((1.9,-3.5,1.0)); base.look_at(scene.camera,center)
        scene.camera.data.ortho_scale=1.6
        scene.render.filepath=str(OUT/'Preview'/f'CH_{cid}_{label}.png'); bpy.ops.render.render(write_still=True)
    motion.reset_pose(rig)


def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--character',choices=list(PALETTES))
    parser.add_argument('--renders-only',action='store_true')
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    for folder in ('Source','FBX','Textures','Preview'): (OUT/folder).mkdir(parents=True,exist_ok=True)
    bpy.context.preferences.filepaths.save_version=0; bpy.context.preferences.filepaths.file_preview_type='NONE'
    for cid in ([args.character] if args.character else PALETTES):
        if args.renders_only:
            bpy.ops.wm.open_mainfile(filepath=str(OUT/'Source'/f'CH_{cid}_visual_wip_v3.blend'))
            render(cid,bpy.data.objects['CH_'+cid+'_Rig'],bpy.data.objects['CH_'+cid+'_Body_LOD0'])
            continue
        rig,body,accessory=create_character(cid)
        bake_materials(cid,(body,accessory))
        normalize(rig,(body,accessory),cid)
        ground_blocking(rig,(body,accessory))
        low=[lod(obj,level,ratio) for level,ratio in ((1,.58),(2,.285)) for obj in (body,accessory)]
        render(cid,rig,body)
        # Relative packed texture references make the .blend portable.
        for img in bpy.data.images:
            if img.source=='FILE' and img.filepath and Path(bpy.path.abspath(img.filepath)).exists(): img.pack()
        body['asset_stage']='ReferenceAligned_ProceduralWIP_v3'
        bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Source'/f'CH_{cid}_visual_wip_v3.blend'),compress=True)
        bpy.ops.object.select_all(action='DESELECT')
        for obj in (rig,body,accessory,*low): obj.hide_set(False); obj.select_set(True)
        bpy.context.view_layer.objects.active=rig
        bpy.ops.export_scene.fbx(filepath=str(OUT/'FBX'/f'CH_{cid}_visual_wip_v3.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=False,path_mode='STRIP')
        print('VISUAL_V3_BUILT',cid,flush=True)


if __name__=='__main__': main()
