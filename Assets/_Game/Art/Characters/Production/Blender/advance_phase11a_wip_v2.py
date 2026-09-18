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
CHARACTER_IDS = ("flash", "chubby", "slacker")


def action_fcurves(action):
    for layer in action.layers:
        for strip in layer.strips:
            for channel_bag in strip.channelbags:
                yield from channel_bag.fcurves


def reset_pose(armature):
    armature.animation_data_create()
    armature.animation_data.action = None
    for bone in armature.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.location = (0, 0, 0)
        bone.rotation_euler = (0, 0, 0)
        bone.scale = (1, 1, 1)
    bpy.context.scene.frame_set(1)
    bpy.context.view_layer.update()


def add_action(armature, name, end_frame, keys):
    action = bpy.data.actions.get(name) or bpy.data.actions.new(name)
    action.use_fake_user = True
    armature.animation_data_create()
    armature.animation_data.action = action
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "XYZ"
        pose_bone.location = (0, 0, 0)
        pose_bone.rotation_euler = (0, 0, 0)
        pose_bone.scale = (1, 1, 1)
    for frame, instructions in keys.items():
        for bone_name, values in instructions.items():
            bone = armature.pose.bones[bone_name]
            if "rot" in values:
                bone.rotation_euler = values["rot"]
                bone.keyframe_insert("rotation_euler", frame=frame, group=bone_name)
            if "loc" in values:
                bone.location = values["loc"]
                bone.keyframe_insert("location", frame=frame, group=bone_name)
    action.frame_start = 1
    action.frame_end = end_frame
    action["blocking_only"] = True
    action["root_motion"] = False


def add_missing_actions(armature, character_id):
    energy = {"flash": 1.15, "chubby": 0.78, "slacker": 0.66}[character_id]
    add_action(armature, "Interfere_Blocking", 42, {
        1: {"Wing_L_Shoulder": {"rot": (0, 0, 0)}, "Leg_L_Hip": {"rot": (0, 0, 0)}},
        18: {"Body_01": {"rot": (math.radians(8), 0, math.radians(8))},
             "Wing_L_Shoulder": {"rot": (0, math.radians(48) * energy, math.radians(32))},
             "Leg_L_Hip": {"rot": (math.radians(-22), 0, math.radians(18))}},
        42: {"Body_01": {"rot": (0, 0, 0)}, "Wing_L_Shoulder": {"rot": (0, 0, 0)},
             "Leg_L_Hip": {"rot": (0, 0, 0)}},
    })
    add_action(armature, "Celebrate_Blocking", 72, {
        1: {"CenterOfMass": {"loc": (0, 0, 0)}, "Wing_L_Shoulder": {"rot": (0, 0, 0)},
            "Wing_R_Shoulder": {"rot": (0, 0, 0)}},
        20: {"CenterOfMass": {"loc": (0, 0, 0.14 * energy)},
             "Wing_L_Shoulder": {"rot": (0, math.radians(62), math.radians(22))},
             "Wing_R_Shoulder": {"rot": (0, math.radians(-62), math.radians(-22))},
             "Head": {"rot": (math.radians(-10), 0, 0)}},
        40: {"CenterOfMass": {"loc": (0, 0, 0.02)},
             "Wing_L_Shoulder": {"rot": (0, math.radians(32), math.radians(-18))},
             "Wing_R_Shoulder": {"rot": (0, math.radians(-32), math.radians(18))}},
        60: {"CenterOfMass": {"loc": (0, 0, 0.12 * energy)},
             "Wing_L_Shoulder": {"rot": (0, math.radians(68), math.radians(18))},
             "Wing_R_Shoulder": {"rot": (0, math.radians(-68), math.radians(-18))}},
        72: {"CenterOfMass": {"loc": (0, 0, 0)}, "Wing_L_Shoulder": {"rot": (0, 0, 0)},
             "Wing_R_Shoulder": {"rot": (0, 0, 0)}, "Head": {"rot": (0, 0, 0)}},
    })
    add_action(armature, "Lose_Blocking", 72, {
        1: {"Body_01": {"rot": (0, 0, 0)}, "Head": {"rot": (0, 0, 0)}},
        28: {"Body_01": {"rot": (math.radians(-8), 0, 0)},
             "Neck_01": {"rot": (math.radians(18), 0, 0)}, "Head": {"rot": (math.radians(22), 0, 0)},
             "Wing_L_Shoulder": {"rot": (math.radians(18), 0, math.radians(-10))},
             "Wing_R_Shoulder": {"rot": (math.radians(18), 0, math.radians(10))}},
        52: {"Head": {"rot": (math.radians(28), 0, math.radians(4))}},
        72: {"Head": {"rot": (math.radians(22), 0, math.radians(-4))}},
    })
    reset_pose(armature)


def material_vertex_indices(body, material_token):
    material_slots = {index for index, material in enumerate(body.data.materials) if material and material_token in material.name}
    return {vertex for polygon in body.data.polygons if polygon.material_index in material_slots for vertex in polygon.vertices}


def world_coordinate(body, index):
    return body.matrix_world @ body.data.vertices[index].co


def add_expression_shapes(body):
    if body.data.shape_keys:
        return
    basis = body.shape_key_add(name="Basis")
    eye_white = material_vertex_indices(body, "EyeWhite")
    pupils = material_vertex_indices(body, "_Eye") - eye_white
    beak_all = material_vertex_indices(body, "Beak")
    beak = {index for index in beak_all if world_coordinate(body, index).z > 1.25 and world_coordinate(body, index).y < -0.15}
    body_shell = material_vertex_indices(body, "Body") | material_vertex_indices(body, "Cream")
    body_shell = {index for index in body_shell if world_coordinate(body, index).z > 0.45}

    def side(indices, is_left):
        return {index for index in indices if (world_coordinate(body, index).x < 0) == is_left}

    def add_key(name, operation):
        key = body.shape_key_add(name=name, from_mix=False)
        # Blender keeps the newly-created key active while authoring.  Copy the
        # Basis explicitly so each expression is independent instead of
        # accumulating the previously authored key deltas.
        for index in range(len(basis.data)):
            key.data[index].co = basis.data[index].co
        operation(key)
        key.slider_min = 0.0
        key.slider_max = 1.0

    def blink(indices):
        def operation(key):
            center = sum(body.data.vertices[index].co.z for index in indices) / max(1, len(indices))
            for index in indices:
                key.data[index].co.z = center + (basis.data[index].co.z - center) * 0.14
        return operation

    add_key("Blink_L", blink(side(eye_white | pupils, True)))
    add_key("Blink_R", blink(side(eye_white | pupils, False)))

    def smile(key):
        for index in beak:
            world = world_coordinate(body, index)
            if world.z < 2.05:
                key.data[index].co.z += 0.035
                key.data[index].co.x *= 1.035
    add_key("Smile", smile)

    def surprise(key):
        median_z = sum(world_coordinate(body, index).z for index in beak) / max(1, len(beak))
        for index in beak:
            if world_coordinate(body, index).z < median_z:
                key.data[index].co.z -= 0.085
            else:
                key.data[index].co.z += 0.018
    add_key("Surprise", surprise)

    def sad(key):
        for index in pupils:
            key.data[index].co.z -= 0.035
        for index in eye_white:
            if world_coordinate(body, index).x < 0:
                key.data[index].co.z += key.data[index].co.x * 0.07
            else:
                key.data[index].co.z -= key.data[index].co.x * 0.07
    add_key("Sad", sad)

    def dizzy(key):
        for index in pupils:
            world = world_coordinate(body, index)
            key.data[index].co.x += 0.055 if world.x < 0 else -0.055
            key.data[index].co.z += 0.035 if world.x < 0 else -0.035
    add_key("Dizzy", dizzy)

    def grit(key):
        median_z = sum(world_coordinate(body, index).z for index in beak) / max(1, len(beak))
        for index in beak:
            key.data[index].co.z += 0.035 if world_coordinate(body, index).z < median_z else -0.025
            key.data[index].co.y += 0.012
    add_key("Grit", grit)

    def squash(key):
        center_z = sum(basis.data[index].co.z for index in body_shell) / max(1, len(body_shell))
        for index in body_shell:
            base = basis.data[index].co
            key.data[index].co.z = center_z + (base.z - center_z) * 0.91
            key.data[index].co.x = base.x * 1.045
            key.data[index].co.y = base.y * 1.035
    add_key("Squash", squash)

    body["expression_shapes"] = "Blink_L,Blink_R,Smile,Surprise,Sad,Dizzy,Grit,Squash"


def prepare_uv(body):
    if not body.data.uv_layers:
        bpy.context.view_layer.objects.active = body
        body.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
    body.data.uv_layers.active.name = "UV0"


def remove_shape_keys(obj):
    if not obj.data.shape_keys:
        return
    for key in list(obj.data.shape_keys.key_blocks)[::-1]:
        obj.shape_key_remove(key)


def make_lod(body, suffix, ratio):
    duplicate = body.copy()
    duplicate.data = body.data.copy()
    duplicate.name = body.name.replace("_LOD0", suffix)
    bpy.context.collection.objects.link(duplicate)
    bpy.context.view_layer.objects.active = duplicate
    duplicate.select_set(True)
    remove_shape_keys(duplicate)
    modifier = duplicate.modifiers.new(f"Decimate_{suffix}", "DECIMATE")
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    while duplicate.modifiers.find(modifier.name) > 0:
        bpy.ops.object.modifier_move_up(modifier=modifier.name)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    duplicate["lod_level"] = suffix[-1]
    duplicate.hide_render = True
    duplicate.hide_set(True)
    return duplicate


def render_preview(character_id, armature, body, action_name, frame, shape_name, suffix):
    reset_pose(armature)
    for key in body.data.shape_keys.key_blocks:
        if key.name != "Basis":
            key.value = 0.0
    armature.animation_data.action = bpy.data.actions[action_name]
    bpy.context.scene.frame_set(frame)
    if shape_name:
        body.data.shape_keys.key_blocks[shape_name].value = 0.9
    bpy.context.scene.render.filepath = str(PREVIEW_DIR / f"CH_{character_id}_wip_v2_{suffix}.png")
    bpy.ops.render.render(write_still=True)


def export_character(character_id, armature, body, lods):
    reset_pose(armature)
    for key in body.data.shape_keys.key_blocks:
        if key.name != "Basis":
            key.value = 0.0
    body["asset_stage"] = "Phase11A_ProductionWIP_v2"
    body["root_motion_policy"] = "Unity owns race translation"
    body.name = f"CH_{character_id}_Body_LOD0"
    source_path = SOURCE_DIR / f"CH_{character_id}_production_wip_v2.blend"
    fbx_path = FBX_DIR / f"CH_{character_id}_production_wip_v2.fbx"
    bpy.ops.wm.save_as_mainfile(filepath=str(source_path), compress=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in (armature, body, *lods):
        obj.hide_set(False)
        obj.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(
        filepath=str(fbx_path), use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True, add_leaf_bones=False, bake_anim=True,
        bake_anim_use_all_actions=True, bake_anim_simplify_factor=0.0,
        axis_forward="-Z", axis_up="Y", use_mesh_modifiers=True,
    )


def process_character(character_id):
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE_DIR / f"CH_{character_id}_blockout_v1.blend"))
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
    armature = next(obj for obj in bpy.data.objects if obj.type == "ARMATURE")
    body = next(obj for obj in bpy.data.objects if obj.type == "MESH" and any(mod.type == "ARMATURE" for mod in obj.modifiers))
    # The blockout joins accessories into the skinned body, so their socket
    # bones must participate in deformation.  They remain rigid, but now follow
    # the body during falls, turns, and celebrations.
    for bone in armature.data.bones:
        if bone.name.startswith("AccessorySocket"):
            bone.use_deform = True
    body.name = f"CH_{character_id}_Body_LOD0"
    prepare_uv(body)
    add_expression_shapes(body)
    add_missing_actions(armature, character_id)
    lod1 = make_lod(body, "_LOD1", 0.75)
    lod2 = make_lod(body, "_LOD2", 0.45)
    render_preview(character_id, armature, body, "Sprint_Blocking", 8, "Grit", "sprint")
    render_preview(character_id, armature, body, "Fall_Blocking", 30, "Surprise", "fall")
    render_preview(character_id, armature, body, "Celebrate_Blocking", 20, "Smile", "celebrate")
    export_character(character_id, armature, body, (lod1, lod2))


def main():
    for character_id in CHARACTER_IDS:
        process_character(character_id)
    print("PHASE11A_WIP_V2_COMPLETE")


if __name__ == "__main__":
    main()
