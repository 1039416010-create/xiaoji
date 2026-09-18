from __future__ import annotations

import math
from pathlib import Path

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
PRODUCTION_DIR = SCRIPT_DIR.parent
SOURCE_DIR = PRODUCTION_DIR / "Source"
FBX_DIR = PRODUCTION_DIR / "FBX"
PREVIEW_DIR = PRODUCTION_DIR / "Preview"

for directory in (SOURCE_DIR, FBX_DIR, PREVIEW_DIR):
    directory.mkdir(parents=True, exist_ok=True)


PROFILES = {
    "flash": {
        "body": (0.58, 0.48, 0.88), "body_z": 1.23,
        "head": (0.43, 0.40, 0.46), "head_z": 2.10,
        "leg_x": 0.27, "leg_top": 0.86,
        "body_color": (0.94, 0.61, 0.10, 1.0),
        "cream_color": (0.98, 0.86, 0.58, 1.0),
        "accent_color": (0.78, 0.04, 0.05, 1.0),
        "detail_color": (0.08, 0.31, 0.72, 1.0),
        "motion_scale": 1.10,
    },
    "chubby": {
        "body": (0.91, 0.70, 0.86), "body_z": 1.11,
        "head": (0.50, 0.45, 0.48), "head_z": 1.92,
        "leg_x": 0.38, "leg_top": 0.54,
        "body_color": (0.92, 0.73, 0.40, 1.0),
        "cream_color": (0.98, 0.90, 0.73, 1.0),
        "accent_color": (0.08, 0.45, 0.37, 1.0),
        "detail_color": (0.72, 0.12, 0.08, 1.0),
        "motion_scale": 0.78,
    },
    "slacker": {
        "body": (0.69, 0.57, 0.80), "body_z": 1.12,
        "head": (0.48, 0.43, 0.50), "head_z": 1.98,
        "leg_x": 0.31, "leg_top": 0.72,
        "body_color": (0.36, 0.40, 0.46, 1.0),
        "cream_color": (0.70, 0.65, 0.59, 1.0),
        "accent_color": (0.72, 0.28, 0.12, 1.0),
        "detail_color": (0.07, 0.06, 0.06, 1.0),
        "motion_scale": 0.84,
    },
}


def clear_scene() -> None:
    bpy.ops.object.mode_set(mode="OBJECT") if bpy.context.object and bpy.context.object.mode != "OBJECT" else None
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (bpy.data.meshes, bpy.data.curves, bpy.data.armatures, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for item in list(collection):
            if item.users == 0:
                collection.remove(item)
    # Actions use a fake user so Blender keeps them after deleting the prior
    # character.  Clear them deliberately to prevent duplicated .001 clips and
    # accidental export of another character's animation set.
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)


def material(name: str, color: tuple[float, float, float, float], metallic: float = 0.0, roughness: float = 0.52) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    principled = next((node for node in mat.node_tree.nodes if node.type == "BSDF_PRINCIPLED"), None)
    if principled is None:
        principled = mat.node_tree.nodes.new("ShaderNodeBsdfPrincipled")
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Roughness"].default_value = roughness
    principled.inputs["Metallic"].default_value = metallic
    return mat


def make_armature(profile: dict, name: str) -> bpy.types.Object:
    data = bpy.data.armatures.new(f"{name}_Rig")
    armature = bpy.data.objects.new(f"{name}_Rig", data)
    bpy.context.collection.objects.link(armature)
    armature.show_in_front = True
    armature.data.display_type = "BBONE"
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    body_z = profile["body_z"]
    head_z = profile["head_z"]
    leg_top = profile["leg_top"]
    leg_x = profile["leg_x"]

    bones: dict[str, bpy.types.EditBone] = {}

    def add(bone_name: str, head, tail, parent: str | None = None, connected: bool = False):
        bone = data.edit_bones.new(bone_name)
        bone.head = head
        bone.tail = tail
        if parent:
            bone.parent = bones[parent]
            bone.use_connect = connected
        bones[bone_name] = bone
        return bone

    add("Root", (0, 0, 0), (0, 0, 0.18))
    add("CenterOfMass", (0, 0, 0.18), (0, 0, body_z * 0.55), "Root")
    add("Body_01", (0, 0, body_z * 0.55), (0, 0, body_z), "CenterOfMass", True)
    add("Body_02", (0, 0, body_z), (0, 0, body_z + 0.42), "Body_01", True)
    add("Neck_01", (0, 0, body_z + 0.42), (0, 0, head_z - 0.30), "Body_02", True)
    add("Neck_02", (0, 0, head_z - 0.30), (0, 0, head_z - 0.10), "Neck_01", True)
    add("Head", (0, 0, head_z - 0.10), (0, 0, head_z + 0.30), "Neck_02", True)
    add("Beak_Upper", (0, -0.22, head_z + 0.03), (0, -0.62, head_z + 0.05), "Head")
    add("Beak_Lower", (0, -0.22, head_z - 0.03), (0, -0.55, head_z - 0.10), "Head")
    for side, x in (("L", -0.18), ("R", 0.18)):
        add(f"Eye_{side}", (x, -0.18, head_z + 0.10), (x, -0.40, head_z + 0.10), "Head")
        add(f"LidUpper_{side}", (x, -0.22, head_z + 0.18), (x, -0.40, head_z + 0.18), "Head")
    add("Comb_01", (0, 0, head_z + 0.27), (0, 0, head_z + 0.49), "Head")
    add("Comb_02", (0, 0, head_z + 0.49), (0, 0.01, head_z + 0.65), "Comb_01", True)
    add("Comb_03", (0, 0.01, head_z + 0.65), (0, 0.04, head_z + 0.78), "Comb_02", True)
    for side, sign in (("L", -1.0), ("R", 1.0)):
        shoulder_x = sign * profile["body"][0] * 0.55
        add(f"Wing_{side}_Shoulder", (shoulder_x, 0, body_z + 0.28), (sign * 0.73, 0.02, body_z + 0.15), "Body_02")
        add(f"Wing_{side}_Elbow", (sign * 0.73, 0.02, body_z + 0.15), (sign * 1.02, 0.04, body_z - 0.02), f"Wing_{side}_Shoulder", True)
        add(f"Wing_{side}_Wrist", (sign * 1.02, 0.04, body_z - 0.02), (sign * 1.18, -0.02, body_z - 0.20), f"Wing_{side}_Elbow", True)
        for index in range(1, 4):
            add(f"Wing_{side}_Feather_{index:02d}", (sign * (0.82 + index * 0.08), 0.02, body_z + 0.02), (sign * (1.08 + index * 0.09), 0.01, body_z - 0.22 - index * 0.05), f"Wing_{side}_Wrist")
        add(f"Leg_{side}_Hip", (sign * leg_x, 0, leg_top), (sign * leg_x, 0, leg_top * 0.66), "CenterOfMass")
        add(f"Leg_{side}_Knee", (sign * leg_x, 0, leg_top * 0.66), (sign * leg_x, 0, 0.28), f"Leg_{side}_Hip", True)
        add(f"Leg_{side}_Ankle", (sign * leg_x, 0, 0.28), (sign * leg_x, 0, 0.09), f"Leg_{side}_Knee", True)
        add(f"Leg_{side}_Toe_A", (sign * leg_x, 0, 0.09), (sign * leg_x, -0.34, 0.04), f"Leg_{side}_Ankle")
        add(f"Leg_{side}_Toe_B", (sign * leg_x, 0, 0.09), (sign * (leg_x + 0.18), -0.22, 0.04), f"Leg_{side}_Ankle")
    add("Tail_01", (0, 0.25, body_z), (0, 0.55, body_z + 0.08), "Body_01")
    add("Tail_02", (0, 0.55, body_z + 0.08), (0, 0.85, body_z + 0.22), "Tail_01", True)
    add("TailFan_L", (0, 0.72, body_z + 0.15), (-0.24, 1.10, body_z + 0.40), "Tail_02")
    add("TailFan_C", (0, 0.72, body_z + 0.15), (0, 1.18, body_z + 0.48), "Tail_02")
    add("TailFan_R", (0, 0.72, body_z + 0.15), (0.24, 1.10, body_z + 0.40), "Tail_02")
    add("AccessorySocket_Head", (0, 0, head_z + 0.26), (0, 0.05, head_z + 0.46), "Head")
    add("AccessorySocket_Back", (0, 0.24, body_z + 0.30), (0, 0.46, body_z + 0.30), "Body_02")
    add("AccessorySocket_Chest", (0, -0.30, body_z + 0.25), (0, -0.50, body_z + 0.25), "Body_02")

    bpy.ops.object.mode_set(mode="OBJECT")
    for bone in data.bones:
        bone.use_deform = not bone.name.startswith("AccessorySocket")
    armature["gck_rig_version"] = "1.0"
    armature["root_motion_policy"] = "Root remains stationary; Unity owns race translation"
    return armature


def add_sphere(name, location, scale, mat, bone_name, parts):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=16, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    group = obj.vertex_groups.new(name=bone_name)
    group.add(range(len(obj.data.vertices)), 1.0, "REPLACE")
    parts.append(obj)
    return obj


def add_cylinder(name, start, end, radius, mat, bone_name, parts):
    start_v, end_v = Vector(start), Vector(end)
    direction = end_v - start_v
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=radius, depth=direction.length, location=(start_v + end_v) * 0.5)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = direction.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.data.materials.append(mat)
    group = obj.vertex_groups.new(name=bone_name)
    group.add(range(len(obj.data.vertices)), 1.0, "REPLACE")
    parts.append(obj)
    return obj


def join_skinned_parts(parts, armature, name):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    body = bpy.context.object
    body.name = name
    body.parent = armature
    modifier = body.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    body["asset_stage"] = "Phase11A_Blockout"
    return body


def build_character(character_id: str, profile: dict):
    clear_scene()
    name = f"CH_{character_id}"
    armature = make_armature(profile, name)
    mats = {
        "body": material(f"MAT_{name}_Body", profile["body_color"]),
        "cream": material(f"MAT_{name}_Cream", profile["cream_color"]),
        "accent": material(f"MAT_{name}_Accent", profile["accent_color"]),
        "detail": material(f"MAT_{name}_Detail", profile["detail_color"], metallic=0.08),
        "beak": material(f"MAT_{name}_Beak", (0.95, 0.38, 0.05, 1.0), roughness=0.42),
        "eye": material(f"MAT_{name}_Eye", (0.025, 0.018, 0.012, 1.0), roughness=0.18),
        "white": material(f"MAT_{name}_EyeWhite", (0.95, 0.94, 0.88, 1.0), roughness=0.30),
    }
    parts = []
    body_z, head_z = profile["body_z"], profile["head_z"]
    bx, by, bz = profile["body"]
    hx, hy, hz = profile["head"]
    add_sphere(f"{name}_Body", (0, 0, body_z), (bx, by, bz), mats["body"], "Body_01", parts)
    add_sphere(f"{name}_Chest", (0, -by * 0.78, body_z + 0.03), (bx * 0.68, by * 0.20, bz * 0.67), mats["cream"], "Body_02", parts)
    add_sphere(f"{name}_Head", (0, -0.02, head_z), (hx, hy, hz), mats["body"], "Head", parts)
    add_sphere(f"{name}_BeakUpper", (0, -hy * 0.93, head_z + 0.01), (0.22, 0.23, 0.12), mats["beak"], "Beak_Upper", parts)
    add_sphere(f"{name}_BeakLower", (0, -hy * 0.88, head_z - 0.10), (0.18, 0.18, 0.08), mats["beak"], "Beak_Lower", parts)
    for side, sign in (("L", -1), ("R", 1)):
        eye_x = sign * hx * 0.43
        add_sphere(f"{name}_Eye_{side}", (eye_x, -hy * 0.77, head_z + 0.12), (0.15, 0.09, 0.18), mats["white"], f"Eye_{side}", parts)
        add_sphere(f"{name}_Pupil_{side}", (eye_x, -hy * 0.86, head_z + 0.12), (0.075, 0.035, 0.095), mats["eye"], f"Eye_{side}", parts)
        shoulder_x = sign * bx * 0.72
        add_sphere(f"{name}_WingUpper_{side}", (shoulder_x, 0.0, body_z + 0.22), (0.22, by * 0.58, bz * 0.54), mats["body"], f"Wing_{side}_Shoulder", parts)
        add_sphere(f"{name}_WingLower_{side}", (sign * (bx + 0.18), 0.02, body_z - 0.08), (0.17, by * 0.45, bz * 0.44), mats["cream"], f"Wing_{side}_Elbow", parts)
        leg_x, leg_top = sign * profile["leg_x"], profile["leg_top"]
        add_cylinder(f"{name}_LegUpper_{side}", (leg_x, 0, leg_top), (leg_x, 0, leg_top * 0.58), 0.065, mats["beak"], f"Leg_{side}_Hip", parts)
        add_cylinder(f"{name}_LegLower_{side}", (leg_x, 0, leg_top * 0.58), (leg_x, 0, 0.10), 0.055, mats["beak"], f"Leg_{side}_Knee", parts)
        add_cylinder(f"{name}_ToeA_{side}", (leg_x, 0, 0.08), (leg_x, -0.30, 0.04), 0.035, mats["beak"], f"Leg_{side}_Toe_A", parts)
        add_cylinder(f"{name}_ToeB_{side}", (leg_x, 0, 0.08), (leg_x + sign * 0.18, -0.20, 0.04), 0.032, mats["beak"], f"Leg_{side}_Toe_B", parts)
    for index, (x, scale) in enumerate(((-0.22, (0.18, 0.42, 0.50)), (0, (0.20, 0.48, 0.58)), (0.22, (0.18, 0.42, 0.50)))):
        add_sphere(f"{name}_Tail_{index}", (x, by * 0.86, body_z + 0.27), scale, mats["body"], ("TailFan_L", "TailFan_C", "TailFan_R")[index], parts)
    comb_scale = 1.25 if character_id == "flash" else 0.9
    for index, z in enumerate((head_z + hz * 0.80, head_z + hz * 1.05, head_z + hz * 1.27), 1):
        add_sphere(f"{name}_Comb_{index}", (0, 0, z), (0.11 * comb_scale, 0.12, 0.17), mats["accent"], f"Comb_{index:02d}", parts)

    if character_id == "flash":
        add_sphere(f"{name}_Headband", (0, -0.01, head_z + 0.20), (hx * 1.04, hy * 1.03, 0.075), mats["accent"], "AccessorySocket_Head", parts)
        for side, sign in (("L", -1), ("R", 1)):
            add_sphere(f"{name}_Lightning_{side}", (sign * (bx * 0.75), -by * 0.40, body_z + 0.25), (0.07, 0.05, 0.28), mats["detail"], f"Wing_{side}_Shoulder", parts)
    elif character_id == "chubby":
        add_sphere(f"{name}_BowCenter", (0, -by * 1.03, body_z + 0.48), (0.13, 0.08, 0.13), mats["accent"], "AccessorySocket_Chest", parts)
        add_sphere(f"{name}_BowL", (-0.17, -by * 1.01, body_z + 0.48), (0.20, 0.07, 0.14), mats["accent"], "AccessorySocket_Chest", parts)
        add_sphere(f"{name}_BowR", (0.17, -by * 1.01, body_z + 0.48), (0.20, 0.07, 0.14), mats["accent"], "AccessorySocket_Chest", parts)
    elif character_id == "slacker":
        add_sphere(f"{name}_Scarf", (0, -by * 0.90, body_z + 0.40), (bx * 0.72, 0.08, 0.13), mats["accent"], "AccessorySocket_Chest", parts)
        for side, sign in (("L", -1), ("R", 1)):
            add_sphere(f"{name}_Goggle_{side}", (sign * hx * 0.40, -hy * 0.72, head_z + hz * 0.56), (0.18, 0.08, 0.13), mats["detail"], "AccessorySocket_Head", parts)

    body = join_skinned_parts(parts, armature, f"{name}_Body_SKIN")
    create_blocking_actions(armature, profile["motion_scale"])
    setup_preview_scene(character_id, body_z, head_z)
    return armature, body


def create_blocking_actions(armature, motion_scale: float):
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="POSE")
    fps = 24
    bpy.context.scene.render.fps = fps

    def action(name, end_frame, keys):
        act = bpy.data.actions.new(name)
        act.use_fake_user = True
        armature.animation_data_create()
        armature.animation_data.action = act
        for pose_bone in armature.pose.bones:
            pose_bone.rotation_mode = "XYZ"
            pose_bone.rotation_euler = (0, 0, 0)
            pose_bone.location = (0, 0, 0)
        for frame, instructions in keys.items():
            for bone_name, values in instructions.items():
                bone = armature.pose.bones[bone_name]
                if "rot" in values:
                    bone.rotation_euler = values["rot"]
                    bone.keyframe_insert("rotation_euler", frame=frame, group=bone_name)
                if "loc" in values:
                    bone.location = values["loc"]
                    bone.keyframe_insert("location", frame=frame, group=bone_name)
        act.frame_start = 1
        act.frame_end = end_frame
        act["blocking_only"] = True
        act["root_motion"] = False
        return act

    wing_up = math.radians(38) * motion_scale
    leg_swing = math.radians(32) * motion_scale
    action("Idle_Blocking", 48, {
        1: {"Body_02": {"rot": (math.radians(-2), 0, 0)}},
        24: {"Body_02": {"rot": (math.radians(3), 0, 0)}},
        48: {"Body_02": {"rot": (math.radians(-2), 0, 0)}},
    })
    action("Warmup_Blocking", 72, {
        1: {"Wing_L_Shoulder": {"rot": (0, 0, 0)}, "Wing_R_Shoulder": {"rot": (0, 0, 0)}},
        36: {"Wing_L_Shoulder": {"rot": (0, wing_up, math.radians(20))}, "Wing_R_Shoulder": {"rot": (0, -wing_up, math.radians(-20))}, "Body_01": {"rot": (math.radians(8), 0, 0)}},
        72: {"Wing_L_Shoulder": {"rot": (0, 0, 0)}, "Wing_R_Shoulder": {"rot": (0, 0, 0)}, "Body_01": {"rot": (0, 0, 0)}},
    })
    run_keys = {}
    for frame, sign in ((1, 1), (6, -1), (11, 1), (16, -1), (21, 1)):
        run_keys[frame] = {
            "Leg_L_Hip": {"rot": (sign * leg_swing, 0, 0)}, "Leg_R_Hip": {"rot": (-sign * leg_swing, 0, 0)},
            "Wing_L_Shoulder": {"rot": (-sign * leg_swing * 0.45, 0, 0)}, "Wing_R_Shoulder": {"rot": (sign * leg_swing * 0.45, 0, 0)},
            "CenterOfMass": {"loc": (0, 0, 0.04 if sign > 0 else 0)},
        }
    action("Run_Blocking", 21, run_keys)
    sprint_keys = {}
    for frame, sign in ((1, 1), (4, -1), (8, 1), (11, -1), (15, 1)):
        sprint_keys[frame] = {
            "Leg_L_Hip": {"rot": (sign * leg_swing * 1.35, 0, 0)}, "Leg_R_Hip": {"rot": (-sign * leg_swing * 1.35, 0, 0)},
            "Body_01": {"rot": (math.radians(15), 0, 0)}, "CenterOfMass": {"loc": (0, 0, 0.06 if sign > 0 else 0.01)},
        }
    action("Sprint_Blocking", 15, sprint_keys)
    action("Stop_Blocking", 36, {
        1: {"Body_01": {"rot": (math.radians(15), 0, 0)}},
        18: {"Body_01": {"rot": (math.radians(-18), 0, 0)}, "Wing_L_Shoulder": {"rot": (0, wing_up, 0)}, "Wing_R_Shoulder": {"rot": (0, -wing_up, 0)}},
        36: {"Body_01": {"rot": (0, 0, 0)}},
    })
    action("Fall_Blocking", 30, {
        1: {"CenterOfMass": {"rot": (0, 0, 0), "loc": (0, 0, 0)}},
        15: {"CenterOfMass": {"rot": (math.radians(25), math.radians(25), 0), "loc": (0, 0, -0.12)}},
        30: {"CenterOfMass": {"rot": (math.radians(70), math.radians(70), 0), "loc": (0, 0, -0.34)}},
    })
    action("Recover_Blocking", 36, {
        1: {"CenterOfMass": {"rot": (math.radians(70), math.radians(70), 0), "loc": (0, 0, -0.34)}},
        18: {"CenterOfMass": {"rot": (math.radians(25), math.radians(20), 0), "loc": (0, 0, -0.12)}},
        36: {"CenterOfMass": {"rot": (0, 0, 0), "loc": (0, 0, 0)}},
    })
    action("Turn_Blocking", 36, {
        1: {"CenterOfMass": {"rot": (0, 0, 0)}},
        18: {"CenterOfMass": {"rot": (0, 0, math.radians(90))}},
        36: {"CenterOfMass": {"rot": (0, 0, math.radians(180))}},
    })
    armature.animation_data.action = None
    # Clearing an action does not clear the evaluated pose in Blender.  Reset
    # every bone explicitly so saved files, FBX exports, and preview renders all
    # use the authored rest pose instead of the last frame of Turn_Blocking.
    for pose_bone in armature.pose.bones:
        pose_bone.location = (0, 0, 0)
        pose_bone.rotation_euler = (0, 0, 0)
        pose_bone.scale = (1, 1, 1)
    bpy.context.scene.frame_set(1)
    bpy.context.view_layer.update()
    bpy.ops.object.mode_set(mode="OBJECT")


def look_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_preview_scene(character_id: str, body_z: float, head_z: float):
    bpy.ops.mesh.primitive_plane_add(size=20, location=(0, 0, 0))
    floor = bpy.context.object
    floor.name = "PreviewFloor"
    floor.data.materials.append(material("MAT_PreviewFloor", (0.16, 0.18, 0.20, 1.0), roughness=0.72))
    bpy.ops.object.camera_add(location=(4.2, -7.8, 3.4))
    camera = bpy.context.object
    camera.name = "PreviewCamera"
    camera.data.lens = 58
    look_at(camera, (0, 0, (body_z + head_z) * 0.48))
    bpy.context.scene.camera = camera
    bpy.ops.object.light_add(type="AREA", location=(-3.8, -4.2, 6.2))
    key = bpy.context.object
    key.name = "KeyLight"
    key.data.energy = 900
    key.data.shape = "DISK"
    key.data.size = 4.0
    look_at(key, (0, 0, body_z))
    bpy.ops.object.light_add(type="AREA", location=(4.0, -1.0, 3.8))
    fill = bpy.context.object
    fill.name = "FillLight"
    fill.data.energy = 500
    fill.data.size = 3.0
    look_at(fill, (0, 0, body_z + 0.3))
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.035, 0.04, 0.05)
    scene.render.filepath = str(PREVIEW_DIR / f"CH_{character_id}_blockout_v1.png")


def save_and_export(character_id: str, armature: bpy.types.Object, body: bpy.types.Object):
    blend_path = SOURCE_DIR / f"CH_{character_id}_blockout_v1.blend"
    fbx_path = FBX_DIR / f"CH_{character_id}_blockout_v1.fbx"
    bpy.context.scene.frame_set(1)
    bpy.ops.render.render(write_still=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path), compress=True)
    bpy.ops.object.select_all(action="DESELECT")
    armature.select_set(True)
    body.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(
        filepath=str(fbx_path), use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True, add_leaf_bones=False, bake_anim=True,
        bake_anim_use_all_actions=True, bake_anim_simplify_factor=0.0,
        axis_forward="-Z", axis_up="Y",
    )


def build_template():
    clear_scene()
    armature = make_armature(PROFILES["flash"], "GCK_Chicken")
    create_blocking_actions(armature, 1.0)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_DIR / "GCK_ChickenRig_Template_v1.blend"), compress=True)


def main():
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
    for character_id, profile in PROFILES.items():
        armature, body = build_character(character_id, profile)
        save_and_export(character_id, armature, body)
    build_template()
    print("PHASE11A_BUILD_COMPLETE")


if __name__ == "__main__":
    main()
