import json
import os

import bpy


ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SOURCE_DIR = os.path.join(ROOT, "Source")
FBX_DIR = os.path.join(ROOT, "FBX")
CHARACTER_IDS = ("flash", "chubby", "slacker")
EXPECTED_ACTIONS = {
    "Idle_Blocking",
    "Warmup_Blocking",
    "Run_Blocking",
    "Sprint_Blocking",
    "Stop_Blocking",
    "Fall_Blocking",
    "Recover_Blocking",
    "Turn_Blocking",
}


def action_fcurves(action):
    for layer in action.layers:
        for strip in layer.strips:
            for channel_bag in strip.channelbags:
                yield from channel_bag.fcurves


def validate_character(character_id, expected_hierarchy):
    blend_path = os.path.join(SOURCE_DIR, f"CH_{character_id}_blockout_v1.blend")
    fbx_path = os.path.join(FBX_DIR, f"CH_{character_id}_blockout_v1.fbx")
    bpy.ops.wm.open_mainfile(filepath=blend_path)

    armatures = [obj for obj in bpy.data.objects if obj.type == "ARMATURE"]
    skinned = [
        obj for obj in bpy.data.objects
        if obj.type == "MESH" and any(mod.type == "ARMATURE" for mod in obj.modifiers)
    ]
    assert len(armatures) == 1, f"{character_id}: expected one armature"
    assert len(skinned) == 1, f"{character_id}: expected one skinned mesh"

    armature = armatures[0]
    mesh = skinned[0]
    hierarchy = [(bone.name, bone.parent.name if bone.parent else None) for bone in armature.data.bones]
    if expected_hierarchy is not None:
        assert hierarchy == expected_hierarchy, f"{character_id}: shared skeleton mismatch"

    action_names = {action.name for action in bpy.data.actions}
    assert action_names == EXPECTED_ACTIONS, f"{character_id}: action set mismatch"
    root_curves = [
        (action.name, curve.data_path)
        for action in bpy.data.actions
        for curve in action_fcurves(action)
        if 'pose.bones["Root"].location' in curve.data_path
    ]
    assert not root_curves, f"{character_id}: Root contains translation keys"

    posed_bones = []
    for pose_bone in armature.pose.bones:
        has_location = pose_bone.location.length > 0.00001
        has_rotation = sum(abs(axis) for axis in pose_bone.rotation_euler) > 0.00001
        has_scale = any(abs(axis - 1.0) > 0.00001 for axis in pose_bone.scale)
        if has_location or has_rotation or has_scale:
            posed_bones.append(pose_bone.name)
    assert not posed_bones, f"{character_id}: file is not saved in rest pose"
    assert len(mesh.data.vertices) >= 1000, f"{character_id}: mesh is unexpectedly empty"
    assert len(mesh.vertex_groups) >= 20, f"{character_id}: insufficient skin groups"
    assert os.path.getsize(fbx_path) > 100_000, f"{character_id}: FBX export missing or empty"

    return hierarchy, {
        "character": character_id,
        "bones": len(hierarchy),
        "vertices": len(mesh.data.vertices),
        "triangles": sum(max(0, len(poly.vertices) - 2) for poly in mesh.data.polygons),
        "materials": len(mesh.data.materials),
        "skin_groups": len(mesh.vertex_groups),
        "actions": len(action_names),
        "root_translation_curves": len(root_curves),
        "fbx_bytes": os.path.getsize(fbx_path),
    }


def main():
    expected_hierarchy = None
    results = []
    for character_id in CHARACTER_IDS:
        expected_hierarchy, result = validate_character(character_id, expected_hierarchy)
        results.append(result)
    print("PHASE11A_VALIDATION_PASS")
    print(json.dumps(results, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
