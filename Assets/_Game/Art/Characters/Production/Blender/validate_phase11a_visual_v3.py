"""Validate actual evaluated meshes and the FBX round trip, not only filenames."""
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True

import bpy
from mathutils import Matrix

sys.path.insert(0,str(Path(__file__).resolve().parent))
import advance_phase11a_wip_v2 as motion

ROOT=Path(__file__).resolve().parent.parent/'VisualV3'
EXPECTED_SHAPES={'Basis','Blink_L','Blink_R','Smile','Shock','Determined','Sad','Dizzy','Cheek_Puff','Beak_Corner_Up'}
EXPECTED_ACTIONS={'Idle_Blocking','Warmup_Blocking','Run_Blocking','Sprint_Blocking','Stop_Blocking','Fall_Blocking','Recover_Blocking','Turn_Blocking','Interfere_Blocking','Celebrate_Blocking','Lose_Blocking'}


def bounds(obj):
    dg=bpy.context.evaluated_depsgraph_get(); dg.update()
    evaluated=obj.evaluated_get(dg); mesh=evaluated.to_mesh()
    points=[evaluated.matrix_world@v.co for v in mesh.vertices]
    assert all(math.isfinite(c) for point in points for c in point),obj.name+': nonfinite mesh'
    result=([min(v[i] for v in points) for i in range(3)],[max(v[i] for v in points) for i in range(3)])
    evaluated.to_mesh_clear()
    return result


def triangles(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def indices(obj,tag):
    group=obj.vertex_groups['part_'+tag]
    return [v.index for v in obj.data.vertices if any(g.group==group.index for g in v.groups)]


def main():
    results=[]; skeleton=None
    for cid in ('flash','chubby','slacker'):
        bpy.ops.wm.open_mainfile(filepath=str(ROOT/'Source'/f'CH_{cid}_visual_wip_v3.blend'))
        rig=bpy.data.objects['CH_'+cid+'_Rig']
        hierarchy=[(b.name,b.parent.name if b.parent else None) for b in rig.data.bones]
        if skeleton is None: skeleton=hierarchy
        assert hierarchy==skeleton
        meshes=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('CH_'+cid)]
        body=bpy.data.objects['CH_'+cid+'_Body_LOD0']; accessory=bpy.data.objects['CH_'+cid+'_Accessories_LOD0']
        assert len(meshes)==6
        assert set(a.name for a in bpy.data.actions)==EXPECTED_ACTIONS
        keys=body.data.shape_keys.key_blocks
        assert {k.name for k in keys}==EXPECTED_SHAPES
        assert all(abs(k.value)<1e-6 for k in keys if k.name!='Basis'),'Non-neutral expressions on load'
        height=bounds(body)[1][2]
        assert abs(height-(1.08 if cid=='flash' else 1.0))<.015
        totals=[sum(triangles(m) for m in meshes if m.name.endswith('LOD'+str(i))) for i in range(3)]
        assert totals[0]>totals[1]>totals[2]>3000
        for obj in meshes:
            assert obj.scale.x==obj.scale.y==obj.scale.z==1
            assert len(obj.data.materials)==1
            assert obj.data.uv_layers.active.name=='UV0'
            for vert in obj.data.vertices:
                weights=[g.weight for g in vert.groups if obj.vertex_groups[g.group].name in rig.data.bones]
                assert weights and abs(sum(weights)-1)<1e-4,(obj.name,vert.index,weights)
        for label in ('BaseColor','Normal','Mask'):
            img=bpy.data.images.load(str(ROOT/'Textures'/f'T_{cid}_{label}.png'),check_existing=True)
            assert tuple(img.size)==(2048,2048)
        basis=keys['Basis']
        for side in ('L','R'):
            blink=keys['Blink_'+side]
            for part in ('Eye_','Iris_','Pupil_','Glint_'):
                assert all((blink.data[i].co-basis.data[i].co).length<1e-7 for i in indices(body,part+side)), 'Blink deforms eyeball'
            assert max((blink.data[i].co-basis.data[i].co).length for i in indices(body,'Lid_'+side))>.08
            eye_front=min(basis.data[i].co.y for i in indices(body,'Eye_'+side))
            pupil_front=min(basis.data[i].co.y for i in indices(body,'Pupil_'+side))
            assert pupil_front<eye_front-.004,'Pupil is buried'
        # Sample the actual deformed mesh at the start/middle/end of every clip.
        minimum_z=1e9; maximum_extent=0
        for action in bpy.data.actions:
            motion.reset_pose(rig); rig.animation_data.action=action
            for curve in motion.action_fcurves(action):
                assert not curve.data_path.startswith('pose.bones["Root"]'), 'Animated Root'
            a,b=action.frame_range
            for frame in (int(a),int((a+b)/2),int(b)):
                bpy.context.scene.frame_set(frame)
                root=rig.pose.bones['Root'].matrix@rig.data.bones['Root'].matrix_local.inverted()
                assert max(abs(root[r][c]-Matrix.Identity(4)[r][c]) for r in range(4) for c in range(4))<1e-5
                for obj in (body,accessory):
                    low,high=bounds(obj)
                    minimum_z=min(minimum_z,low[2]); extent=max(high[i]-low[i] for i in range(3))
                    maximum_extent=max(maximum_extent,extent)
                    assert extent<1.8,'Exploding deformation'
                    assert low[2]>-.002, f'{cid}/{action.name}/{frame}: below ground {low[2]}'
        motion.reset_pose(rig)
        for key in keys:
            if key.name=='Basis': continue
            key.value=1; low,high=bounds(body)
            assert max(high[i]-low[i] for i in range(3))<1.8
            key.value=0
        # Import what was exported; checking the .blend alone misses morph loss.
        source_counts={o.name:len(o.data.vertices) for o in meshes}
        source_triangles={o.name:triangles(o) for o in meshes}
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=str(ROOT/'FBX'/f'CH_{cid}_visual_wip_v3.fbx'))
        imported=[o for o in bpy.data.objects if o.type=='MESH']
        assert len(imported)==6,'Missing exported LOD'
        imported_body=bpy.data.objects['CH_'+cid+'_Body_LOD0']
        assert imported_body.data.shape_keys is not None,'FBX lost morph targets'
        assert {k.name for k in imported_body.data.shape_keys.key_blocks}==EXPECTED_SHAPES
        for obj in imported:
            assert len(obj.data.vertices)==source_counts[obj.name]
            assert triangles(obj)==source_triangles[obj.name]
            assert any(m.type=='ARMATURE' and m.object for m in obj.modifiers)
        # FBX animation stacks carry both armature and morph channels. Blender
        # exposes those as separate Actions, not duplicate animation takes.
        rig_actions=[a for a in bpy.data.actions if a.name.startswith('CH_'+cid+'_Rig|')]
        morph_actions=[a for a in bpy.data.actions if a.name.startswith('Key|')]
        assert len(rig_actions)==11 and {a.name.rsplit('|',1)[-1] for a in rig_actions}==EXPECTED_ACTIONS
        assert len(morph_actions)==11 and {a.name.rsplit('|',1)[-1] for a in morph_actions}==EXPECTED_ACTIONS
        results.append({'character':cid,'bones':len(hierarchy),'height_m':round(height,4),'lod_triangles':totals,'expression_shapes':9,'clips':11,'sampled_poses':33,'minimum_sampled_z':round(minimum_z,4),'maximum_sampled_extent':round(maximum_extent,4),'fbx_roundtrip':'PASS'})
    report={'status':'PASS_ENGINEERING_CHECKS_ONLY','characters':results,'limitations':['No Unity import verification in this check','Contact penetration and foot sliding require animation refinement','UVs automatically packed; manual art review outstanding','Mask G is constant 1, not baked occlusion','Normal uses procedural surface bump, not a high-poly sculpt bake','This is procedural WIP, not final handcrafted retopology']}
    (ROOT/'validation_report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False,indent=2),flush=True)


if __name__=='__main__': main()
