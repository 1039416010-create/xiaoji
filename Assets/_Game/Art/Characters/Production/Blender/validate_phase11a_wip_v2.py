import json
import os

import bpy


ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SOURCE_DIR = os.path.join(ROOT, "Source")
FBX_DIR = os.path.join(ROOT, "FBX")
CHARACTER_IDS = ("flash", "chubby", "slacker")
EXPECTED_ACTIONS = {
    "Idle_Blocking", "Warmup_Blocking", "Run_Blocking", "Sprint_Blocking",
    "Stop_Blocking", "Fall_Blocking", "Recover_Blocking", "Turn_Blocking",
    "Interfere_Blocking", "Celebrate_Blocking", "Lose_Blocking",
}
EXPECTED_SHAPES = {
    "Basis", "Blink_L", "Blink_R", "Smile", "Surprise", "Sad", "Dizzy", "Grit", "Squash",
}


def action_fcurves(action):
    for layer in action.layers:
        for strip in layer.strips:
            for channel_bag in strip.channelbags:
                yield from channel_bag.fcurves


def triangle_count(obj):
    return sum(max(0, len(polygon.vertices) - 2) for polygon in obj.data.polygons)


def validate(character_id, expected_hierarchy):
    blend_path = os.path.join(SOURCE_DIR, f"CH_{character_id}_production_wip_v2.blend")
    fbx_path = os.path.join(FBX_DIR, f"CH_{character_id}_production_wip_v2.fbx")
    bpy.ops.wm.open_mainfile(filepath=blend_path)

    armatures = [obj for obj in bpy.data.objects if obj.type == "ARMATURE"]
    assert len(armatures) == 1, f"{character_id}: expected one armature"
    armature = armatures[0]
    hierarchy = [(bone.name, bone.parent.name if bone.parent else None) for bone in armature.data.bones]
    if expected_hierarchy is not None:
        assert hierarchy == expected_hierarchy, f"{character_id}: skeleton hierarchy differs"
    assert all(bone.use_deform for bone in armature.data.bones if bone.name.startswith("AccessorySocket")), f"{character_id}: socket does not deform joined accessory"

    meshes = {obj.name.rsplit("_", 1)[-1]: obj for obj in bpy.data.objects if obj.type == "MESH" and "_Body_LOD" in obj.name}
    assert set(meshes) == {"LOD0", "LOD1", "LOD2"}, f"{character_id}: LOD set mismatch"
    assert all(any(mod.type == "ARMATURE" for mod in obj.modifiers) for obj in meshes.values()), f"{character_id}: LOD lacks armature modifier"
    triangles = [triangle_count(meshes[name]) for name in ("LOD0", "LOD1", "LOD2")]
    assert triangles[0] > triangles[1] > triangles[2] >= 3000, f"{character_id}: invalid LOD triangle progression"
    assert all(obj.data.uv_layers and obj.data.uv_layers.active.name == "UV0" for obj in meshes.values()), f"{character_id}: UV0 missing"

    lod0 = meshes["LOD0"]
    assert lod0.data.shape_keys, f"{character_id}: expression shapes missing"
    shapes = {key.name for key in lod0.data.shape_keys.key_blocks}
    assert shapes == EXPECTED_SHAPES, f"{character_id}: expression shape set mismatch"
    basis = lod0.data.shape_keys.key_blocks["Basis"]
    max_shape_delta = 0.0
    for shape in lod0.data.shape_keys.key_blocks:
        if shape.name == "Basis":
            continue
        delta = max((shape.data[index].co - basis.data[index].co).length for index in range(len(basis.data)))
        assert 0.001 < delta < 0.5, f"{character_id}: {shape.name} deformation out of bounds ({delta})"
        max_shape_delta = max(max_shape_delta, delta)

    actions = {action.name for action in bpy.data.actions}
    assert actions == EXPECTED_ACTIONS, f"{character_id}: action set mismatch"
    root_curves = [
        curve.data_path
        for action in bpy.data.actions
        for curve in action_fcurves(action)
        if 'pose.bones["Root"].location' in curve.data_path
    ]
    assert not root_curves, f"{character_id}: Root contains translation keys"
    assert os.path.getsize(fbx_path) > 1_000_000, f"{character_id}: FBX missing or empty"

    return hierarchy, {
        "character": character_id,
        "bones": len(hierarchy),
        "lod_triangles": triangles,
        "shape_keys": len(shapes) - 1,
        "max_shape_delta": round(max_shape_delta, 4),
        "actions": len(actions),
        "root_translation_curves": len(root_curves),
        "fbx_bytes": os.path.getsize(fbx_path),
    }


def main():
    expected_hierarchy = None
    results = []
    for character_id in CHARACTER_IDS:
        expected_hierarchy, result = validate(character_id, expected_hierarchy)
        results.append(result)
    print("PHASE11A_WIP_V2_VALIDATION_PASS")
    print(json.dumps(results, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
