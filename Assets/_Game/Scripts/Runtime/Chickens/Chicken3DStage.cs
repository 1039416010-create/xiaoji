using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Chickens
{
    public sealed class Chicken3DStage : MonoBehaviour
    {
        private const int RenderLayer = 30;
        [SerializeField] private RawImage _warmupOutput;
        [SerializeField] private RawImage _bettingOutput;
        [SerializeField] private RawImage _raceOutput;
        [SerializeField] private RawImage _settlementOutput;
        [SerializeField] private Material _materialTemplate;

        private readonly Dictionary<string, ProceduralChickenAvatar> _avatars = new(StringComparer.Ordinal);
        private readonly Dictionary<Color32, Material> _materials = new();
        private IReadOnlyList<string> _roster = Array.Empty<string>();
        private ChickenController[] _controllers = Array.Empty<ChickenController>();
        private Transform _modelRoot;
        private Camera _camera;
        private RenderTexture _renderTexture;
        private string _settlementChampion;

        public int StyleCount => Chicken3DStyleCatalog.All.Count;
        public string SettlementChampion => _settlementChampion;
        public int BoundControllerCount => _controllers.Length;

        public void Configure(RawImage warmupOutput, RawImage bettingOutput, RawImage raceOutput, RawImage settlementOutput, Material materialTemplate)
        {
            _warmupOutput = warmupOutput;
            _bettingOutput = bettingOutput;
            _raceOutput = raceOutput;
            _settlementOutput = settlementOutput;
            _materialTemplate = materialTemplate;
        }

        public void BindControllers(ChickenController[] controllers)
        {
            _controllers = controllers ?? throw new ArgumentNullException(nameof(controllers));
        }

        public void SetRoster(IReadOnlyList<string> roster)
        {
            if (roster == null || roster.Count != 5)
                throw new ArgumentException("Exactly five chicken IDs are required.", nameof(roster));
            _roster = roster.ToArray();
            foreach (var id in _roster)
                EnsureAvatar(id);
        }

        public void ShowSettlement(string championId)
        {
            _settlementChampion = championId;
            EnsureAvatar(championId);
        }

        public void ClearSettlement() => _settlementChampion = null;

        private void Awake()
        {
            BuildStage();
            AssignOutputTexture();
        }

        private void LateUpdate()
        {
            if (_modelRoot == null)
                return;
            var settlementVisible = _settlementOutput != null && _settlementOutput.gameObject.activeInHierarchy && !string.IsNullOrEmpty(_settlementChampion);
            if (settlementVisible)
                RenderSettlement();
            else
                RenderRoster();
        }

        private void RenderRoster()
        {
            HideAll();
            var useControllerState = _raceOutput != null && _raceOutput.gameObject.activeInHierarchy;
            for (var lane = 0; lane < _roster.Count; lane++)
            {
                var avatar = EnsureAvatar(_roster[lane]);
                avatar.SetVisible(true);
                var controller = lane < _controllers.Length ? _controllers[lane] : null;
                var progress = useControllerState && controller != null ? controller.NormalizedProgress : .035f;
                var state = useControllerState && controller != null && controller.IsPlaying ? controller.VisualState : ChickenVisualState.Warmup;
                var phase = useControllerState && controller != null ? controller.ActiveEventPhase : Mathf.Repeat(Time.unscaledTime * .35f + lane * .13f, 1f);
                var x = Mathf.Lerp(-9.2f, 9.25f, progress);
                var y = 3.05f - lane * 1.52f;
                avatar.SetPose(new Vector3(x, y, lane * .06f), .58f, state, phase, Time.unscaledTime + lane * .17f);
            }
        }

        private void RenderSettlement()
        {
            HideAll();
            var avatar = EnsureAvatar(_settlementChampion);
            avatar.SetVisible(true);
            avatar.SetPose(new Vector3(0f, -.05f, 0f), 1.18f, ChickenVisualState.Celebrate, Mathf.Repeat(Time.unscaledTime, 1f), Time.unscaledTime);
        }

        private ProceduralChickenAvatar EnsureAvatar(string id)
        {
            if (_avatars.TryGetValue(id, out var avatar))
                return avatar;
            var style = Chicken3DStyleCatalog.Get(id);
            avatar = new ProceduralChickenAvatar(_modelRoot, style, RenderLayer, MaterialFor(style.Body), MaterialFor(style.Accent), MaterialFor(new Color(.98f, .92f, .76f)), MaterialFor(new Color(.08f, .10f, .11f)), MaterialFor(new Color(1f, .62f, .12f)));
            _avatars.Add(id, avatar);
            return avatar;
        }

        private Material MaterialFor(Color color)
        {
            var key = (Color32)color;
            if (_materials.TryGetValue(key, out var material))
                return material;
            if (_materialTemplate == null)
                throw new InvalidOperationException("Chicken 3D material template is required so its shader is retained in Windows builds.");
            material = new Material(_materialTemplate) { color = color, name = $"RuntimeChicken_{ColorUtility.ToHtmlStringRGB(color)}" };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .34f);
            _materials.Add(key, material);
            return material;
        }

        private void BuildStage()
        {
            _modelRoot = new GameObject("Procedural3DModels").transform;
            _modelRoot.SetParent(transform, false);
            _modelRoot.gameObject.layer = RenderLayer;

            var cameraObject = new GameObject("Chicken3DCamera", typeof(Camera));
            AddUniversalCameraDataWhenAvailable(cameraObject);
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -25f);
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.layer = RenderLayer;
            _camera = cameraObject.GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 5.35f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _camera.cullingMask = 1 << RenderLayer;
            _camera.allowHDR = false;
            _camera.allowMSAA = false;
            _camera.depth = -20f;

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                _camera.enabled = false;
                return;
            }

            _renderTexture = new RenderTexture(1600, 800, 24, RenderTextureFormat.ARGB32)
            {
                name = "RT_Chicken3DStage",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            _renderTexture.Create();
            _camera.targetTexture = _renderTexture;

            RenderSettings.ambientLight = new Color(.58f, .55f, .48f);
            var key = new GameObject("Chicken3DKeyLight", typeof(Light));
            key.transform.SetParent(transform, false);
            key.transform.localRotation = Quaternion.Euler(35f, -35f, 0f);
            key.layer = RenderLayer;
            var light = key.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, .82f, .58f);
            light.intensity = 1.4f;
            light.cullingMask = 1 << RenderLayer;
            light.shadows = LightShadows.None;
        }

        private static void AddUniversalCameraDataWhenAvailable(GameObject cameraObject)
        {
            // Keep the runtime assembly independent from URP while still registering a camera
            // created at runtime with the active render pipeline in standalone builds.
            var cameraDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime",
                false);
            if (cameraDataType != null && cameraObject.GetComponent(cameraDataType) == null)
                cameraObject.AddComponent(cameraDataType);
        }

        private void AssignOutputTexture()
        {
            foreach (var output in new[] { _warmupOutput, _bettingOutput, _raceOutput, _settlementOutput })
                if (output != null)
                {
                    output.texture = _renderTexture;
                    output.color = Color.white;
                    output.raycastTarget = false;
                }
        }

        private void HideAll()
        {
            foreach (var avatar in _avatars.Values)
                avatar.SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
            foreach (var material in _materials.Values)
                if (material != null)
                    Destroy(material);
        }
    }
}
