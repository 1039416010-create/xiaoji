using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GroundChickenKing.Chickens
{
    public sealed class ProceduralChickenAvatar
    {
        private readonly Chicken3DStyle _style;
        private readonly Transform _root;
        private readonly Transform _bodyPivot;
        private readonly Transform _head;
        private readonly Transform _leftWing;
        private readonly Transform _rightWing;
        private readonly Transform _leftLeg;
        private readonly Transform _rightLeg;
        private readonly Transform _leftEye;
        private readonly Transform _rightEye;
        private readonly Transform _leftPupil;
        private readonly Transform _rightPupil;
        private readonly Renderer[] _accentRenderers;
        private readonly Vector3 _baseScale;

        public ProceduralChickenAvatar(Transform parent, Chicken3DStyle style, int renderLayer, Material body, Material accent, Material cream, Material dark, Material beak)
        {
            _style = style;
            _root = new GameObject($"Avatar3D_{style.Id}").transform;
            _root.SetParent(parent, false);
            SetLayer(_root.gameObject, renderLayer);

            _bodyPivot = new GameObject("Rig").transform;
            _bodyPivot.SetParent(_root, false);
            _baseScale = style.Proportions;

            var bodyShape = Shape(PrimitiveType.Sphere, "Body", _bodyPivot, body, new Vector3(0f, .05f, 0f), new Vector3(1.12f, 1.25f, .92f), renderLayer);
            Shape(PrimitiveType.Sphere, "Breast", bodyShape, cream, new Vector3(.12f, -.12f, -.48f), new Vector3(.72f, .84f, .16f), renderLayer);
            _head = Shape(PrimitiveType.Sphere, "Head", _bodyPivot, body, new Vector3(.52f, .88f, -.05f), new Vector3(.72f, .70f, .68f), renderLayer);
            Shape(PrimitiveType.Sphere, "CombA", _head, accent, new Vector3(-.12f, .56f, .02f), new Vector3(.20f, .28f, .18f), renderLayer);
            Shape(PrimitiveType.Sphere, "CombB", _head, accent, new Vector3(.12f, .60f, .02f), new Vector3(.22f, .34f, .20f), renderLayer);
            Shape(PrimitiveType.Sphere, "Wattle", _head, accent, new Vector3(.48f, -.25f, -.02f), new Vector3(.18f, .28f, .16f), renderLayer);
            Shape(PrimitiveType.Cube, "Beak", _head, beak, new Vector3(.62f, .02f, -.02f), new Vector3(.42f, .18f, .26f), renderLayer).localRotation = Quaternion.Euler(0f, 0f, -5f);

            _leftEye = Shape(PrimitiveType.Sphere, "EyeNear", _head, cream, new Vector3(.34f, .20f, -.50f), new Vector3(.22f, .25f, .10f), renderLayer);
            _rightEye = Shape(PrimitiveType.Sphere, "EyeFar", _head, cream, new Vector3(.34f, .20f, .50f), new Vector3(.22f, .25f, .10f), renderLayer);
            _leftPupil = Shape(PrimitiveType.Sphere, "PupilNear", _leftEye, dark, new Vector3(.12f, 0f, -.55f), new Vector3(.38f, .48f, .22f), renderLayer);
            _rightPupil = Shape(PrimitiveType.Sphere, "PupilFar", _rightEye, dark, new Vector3(.12f, 0f, .55f), new Vector3(.38f, .48f, .22f), renderLayer);

            _leftWing = Shape(PrimitiveType.Sphere, "WingNear", _bodyPivot, accent, new Vector3(-.06f, .10f, -.72f), new Vector3(.72f, .88f, .16f), renderLayer);
            _rightWing = Shape(PrimitiveType.Sphere, "WingFar", _bodyPivot, accent, new Vector3(-.06f, .10f, .72f), new Vector3(.72f, .88f, .16f), renderLayer);
            Shape(PrimitiveType.Sphere, "TailTop", _bodyPivot, accent, new Vector3(-.90f, .30f, .08f), new Vector3(.64f, .24f, .22f), renderLayer).localRotation = Quaternion.Euler(0f, 0f, 35f);
            Shape(PrimitiveType.Sphere, "TailBottom", _bodyPivot, body, new Vector3(-.92f, .05f, -.10f), new Vector3(.58f, .22f, .22f), renderLayer).localRotation = Quaternion.Euler(0f, 0f, 12f);

            _leftLeg = Limb("LegNear", _bodyPivot, beak, new Vector3(.30f, -.91f, -.35f), renderLayer);
            _rightLeg = Limb("LegFar", _bodyPivot, beak, new Vector3(-.28f, -.91f, .35f), renderLayer);
            BuildAccessory(style.Accessory, body, accent, cream, dark, beak, renderLayer);
            _accentRenderers = _root.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.sharedMaterial == accent)
                .ToArray();
            _bodyPivot.localScale = _baseScale;
        }

        public string ChickenId => _style.Id;
        public Transform Root => _root;

        public void SetVisible(bool visible) => _root.gameObject.SetActive(visible);

        public void SetPose(Vector3 position, float scale, ChickenVisualState state, float eventPhase, float time)
        {
            var showAccentDetails = state != ChickenVisualState.Celebrate;
            foreach (var accentRenderer in _accentRenderers)
                accentRenderer.enabled = showAccentDetails;
            _root.localPosition = position;
            _root.localScale = Vector3.one * scale;
            var cycle = time * 7f * _style.Gait;
            var stride = Mathf.Sin(cycle) * 42f;
            var wing = Mathf.Sin(cycle + .8f) * 16f;
            var bob = Mathf.Abs(Mathf.Sin(cycle)) * .13f;
            var lean = -5f - _style.Swagger * 8f;
            var yaw = 0f;
            var fall = 0f;
            var squash = Vector3.one;
            var eyeOpen = 1f;

            switch (state)
            {
                case ChickenVisualState.Idle:
                    stride = Mathf.Sin(time * 2f + _style.Swagger) * 5f;
                    wing = Mathf.Sin(time * 1.6f) * 4f;
                    bob = Mathf.Sin(time * 2f) * .035f;
                    lean = 0f;
                    break;
                case ChickenVisualState.Warmup:
                    stride = Mathf.Sin(time * 5f * _style.Gait) * 28f;
                    wing = -stride * .30f;
                    bob = Mathf.Abs(Mathf.Sin(time * 5f)) * .11f;
                    lean = Mathf.Sin(time * 2.5f) * 6f;
                    squash = new Vector3(1f + bob * .2f, 1f - bob * .18f, 1f);
                    break;
                case ChickenVisualState.Sprint:
                    stride *= 1.35f;
                    wing *= 1.4f;
                    lean = -18f;
                    squash = new Vector3(1.14f, .90f, 1f);
                    break;
                case ChickenVisualState.Stop:
                    stride = 0f;
                    wing = 22f;
                    lean = 13f;
                    squash = new Vector3(.94f, 1.06f, 1f);
                    break;
                case ChickenVisualState.Fall:
                    fall = -82f * Smooth(eventPhase);
                    stride = 30f * Mathf.Sin(eventPhase * Mathf.PI * 3f);
                    wing = 65f * Mathf.Sin(eventPhase * Mathf.PI);
                    bob = -.42f * Smooth(eventPhase);
                    eyeOpen = Mathf.Lerp(1f, .12f, Smooth(eventPhase));
                    break;
                case ChickenVisualState.Recover:
                    fall = Mathf.Lerp(-82f, 0f, Smooth(eventPhase));
                    stride = Mathf.Sin(eventPhase * Mathf.PI * 4f) * 18f;
                    wing = Mathf.Lerp(55f, 0f, eventPhase);
                    bob = Mathf.Lerp(-.42f, 0f, Smooth(eventPhase));
                    eyeOpen = Mathf.Lerp(.12f, 1f, Smooth(eventPhase));
                    break;
                case ChickenVisualState.Turn:
                    yaw = Mathf.Lerp(0f, 180f, Smooth(Mathf.Clamp01(eventPhase * 2f)));
                    stride = Mathf.Sin(cycle) * 36f;
                    lean = eventPhase < .5f ? 4f : -4f;
                    break;
                case ChickenVisualState.Interfere:
                    stride *= .55f;
                    wing = 72f * Mathf.Sin(Mathf.Clamp01(eventPhase) * Mathf.PI);
                    lean = 10f;
                    break;
                case ChickenVisualState.Celebrate:
                    stride = Mathf.Sin(time * 8f) * 12f;
                    wing = 0f;
                    bob = Mathf.Abs(Mathf.Sin(time * 5f)) * .12f;
                    lean = 0f;
                    break;
                case ChickenVisualState.Lose:
                    stride = 0f;
                    wing = -28f;
                    lean = 18f;
                    bob = -.12f;
                    eyeOpen = .45f;
                    squash = new Vector3(1.04f, .92f, 1f);
                    break;
            }

            _bodyPivot.localPosition = new Vector3(0f, bob, 0f);
            _bodyPivot.localScale = Vector3.Scale(_baseScale, squash);
            _bodyPivot.localRotation = Quaternion.Euler(0f, yaw, fall + lean);
            _leftLeg.localRotation = Quaternion.Euler(0f, 0f, stride);
            _rightLeg.localRotation = Quaternion.Euler(0f, 0f, -stride);
            _leftWing.localRotation = Quaternion.Euler(0f, 0f, wing);
            _rightWing.localRotation = Quaternion.Euler(0f, 0f, -wing);
            SetEyeOpen(_leftEye, _leftPupil, eyeOpen);
            SetEyeOpen(_rightEye, _rightPupil, eyeOpen);
            var look = state is ChickenVisualState.Fall or ChickenVisualState.Recover ? new Vector3(.08f, -.14f, 0f) : new Vector3(.08f, 0f, 0f);
            _leftPupil.localPosition = new Vector3(look.x, look.y, -.55f);
            _rightPupil.localPosition = new Vector3(look.x, look.y, .55f);
        }

        private static void SetEyeOpen(Transform eye, Transform pupil, float amount)
        {
            var eyeScale = eye.localScale;
            eyeScale.y = Mathf.Max(.03f, .25f * amount);
            eye.localScale = eyeScale;
            var pupilScale = pupil.localScale;
            pupilScale.y = Mathf.Max(.05f, .48f * amount);
            pupil.localScale = pupilScale;
        }

        private void BuildAccessory(ChickenAccessory accessory, Material body, Material accent, Material cream, Material dark, Material beak, int layer)
        {
            switch (accessory)
            {
                case ChickenAccessory.Headband:
                    Shape(PrimitiveType.Cube, "Headband", _head, accent, new Vector3(0f, .30f, 0f), new Vector3(.92f, .10f, .80f), layer);
                    Shape(PrimitiveType.Cube, "Ribbon", _head, accent, new Vector3(-.62f, .30f, .12f), new Vector3(.55f, .08f, .16f), layer).localRotation = Quaternion.Euler(0f, 0f, 18f);
                    break;
                case ChickenAccessory.BowTie:
                    Shape(PrimitiveType.Sphere, "BowA", _head, accent, new Vector3(.18f, -.48f, -.22f), new Vector3(.24f, .16f, .14f), layer);
                    Shape(PrimitiveType.Sphere, "BowB", _head, accent, new Vector3(.18f, -.48f, .22f), new Vector3(.24f, .16f, .14f), layer);
                    break;
                case ChickenAccessory.RunningShoes:
                    Shape(PrimitiveType.Sphere, "ShoeNear", _leftLeg, accent, new Vector3(.16f, -.78f, 0f), new Vector3(.42f, .15f, .30f), layer);
                    Shape(PrimitiveType.Sphere, "ShoeFar", _rightLeg, accent, new Vector3(.16f, -.78f, 0f), new Vector3(.42f, .15f, .30f), layer);
                    break;
                case ChickenAccessory.Scarf:
                    Shape(PrimitiveType.Cylinder, "Scarf", _head, accent, new Vector3(-.02f, -.48f, 0f), new Vector3(.50f, .10f, .50f), layer);
                    Shape(PrimitiveType.Cube, "ScarfTail", _head, accent, new Vector3(-.48f, -.58f, .12f), new Vector3(.50f, .12f, .18f), layer).localRotation = Quaternion.Euler(0f, 0f, -22f);
                    break;
                case ChickenAccessory.Sunglasses:
                    Glasses(_head, dark, layer);
                    break;
                case ChickenAccessory.LightningCrest:
                    Shape(PrimitiveType.Cube, "Bolt", _head, accent, new Vector3(-.10f, .67f, 0f), new Vector3(.16f, .50f, .16f), layer).localRotation = Quaternion.Euler(0f, 0f, -30f);
                    break;
                case ChickenAccessory.DumplingHat:
                    Shape(PrimitiveType.Sphere, "DumplingHat", _head, cream, new Vector3(-.05f, .61f, 0f), new Vector3(.66f, .24f, .64f), layer);
                    break;
                case ChickenAccessory.CaptainHat:
                    Shape(PrimitiveType.Cylinder, "CaptainHat", _head, dark, new Vector3(-.05f, .58f, 0f), new Vector3(.45f, .14f, .45f), layer);
                    Shape(PrimitiveType.Cube, "HatBill", _head, accent, new Vector3(.30f, .53f, 0f), new Vector3(.42f, .08f, .56f), layer);
                    break;
                case ChickenAccessory.SleepCap:
                    Shape(PrimitiveType.Sphere, "SleepCap", _head, accent, new Vector3(-.20f, .56f, 0f), new Vector3(.66f, .38f, .62f), layer);
                    Shape(PrimitiveType.Sphere, "Pom", _head, cream, new Vector3(-.62f, .74f, 0f), new Vector3(.18f, .18f, .18f), layer);
                    break;
                case ChickenAccessory.RocketPack:
                    Shape(PrimitiveType.Cylinder, "Rocket", _bodyPivot, dark, new Vector3(-.80f, .02f, .40f), new Vector3(.24f, .62f, .24f), layer).localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Shape(PrimitiveType.Sphere, "Flame", _bodyPivot, accent, new Vector3(-1.18f, .02f, .40f), new Vector3(.34f, .16f, .16f), layer);
                    break;
                case ChickenAccessory.NinjaMask:
                    Shape(PrimitiveType.Cube, "Mask", _head, dark, new Vector3(.22f, .18f, 0f), new Vector3(.66f, .22f, .80f), layer);
                    Shape(PrimitiveType.Cube, "MaskTail", _head, accent, new Vector3(-.56f, .15f, .10f), new Vector3(.48f, .10f, .14f), layer);
                    break;
                case ChickenAccessory.ScholarCap:
                    Shape(PrimitiveType.Cube, "ScholarCap", _head, dark, new Vector3(-.02f, .63f, 0f), new Vector3(.76f, .08f, .76f), layer);
                    Shape(PrimitiveType.Sphere, "Tassel", _head, accent, new Vector3(.42f, .48f, -.36f), new Vector3(.12f, .20f, .12f), layer);
                    break;
                case ChickenAccessory.LuckyCrown:
                    Shape(PrimitiveType.Cylinder, "Crown", _head, beak, new Vector3(-.05f, .66f, 0f), new Vector3(.42f, .22f, .42f), layer);
                    Shape(PrimitiveType.Sphere, "Gem", _head, accent, new Vector3(.15f, .78f, -.37f), new Vector3(.12f, .16f, .08f), layer);
                    break;
            }
        }

        private static void Glasses(Transform head, Material material, int layer)
        {
            Shape(PrimitiveType.Cube, "LensNear", head, material, new Vector3(.35f, .20f, -.53f), new Vector3(.30f, .25f, .06f), layer);
            Shape(PrimitiveType.Cube, "LensFar", head, material, new Vector3(.35f, .20f, .53f), new Vector3(.30f, .25f, .06f), layer);
            Shape(PrimitiveType.Cube, "Bridge", head, material, new Vector3(.38f, .20f, 0f), new Vector3(.08f, .06f, .82f), layer);
        }

        private static Transform Limb(string name, Transform parent, Material material, Vector3 position, int layer)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            SetLayer(pivot.gameObject, layer);
            Shape(PrimitiveType.Cylinder, "Shin", pivot, material, new Vector3(0f, -.34f, 0f), new Vector3(.11f, .34f, .11f), layer);
            Shape(PrimitiveType.Cube, "Foot", pivot, material, new Vector3(.20f, -.70f, 0f), new Vector3(.48f, .10f, .18f), layer);
            return pivot;
        }

        private static Transform Shape(PrimitiveType type, string name, Transform parent, Material material, Vector3 position, Vector3 scale, int layer)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            SetLayer(go, layer);
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go.transform;
        }

        private static void SetLayer(GameObject target, int layer) => target.layer = layer;
        private static float Smooth(float value) => value * value * (3f - 2f * value);
    }
}
