using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreGraphics : Manager<ManagerCoreGraphics>
    {
        public event Action<Int2> OnResolutionChanged = null;

        [Header("_")]
        [SerializeField, Required] private UniversalRenderPipelineAsset urpPipelineSettings = null;
#pragma warning disable 0414
        [SerializeField, Required] private UniversalRendererData urpRendererSettings = default;
#pragma warning restore 0414
        [SerializeField, Required] private Volume volumeController = default;

        [Header("_")]
        [SerializeField] private PoolGroupDecal[] decalGroup = null;
        [SerializeField, Required] private Transform decalContainer = null;
        [SerializeField, Required] private ParticleEmitter[] particleGroup = null;
        [SerializeField, Required] private Transform particleContainer = null;

        private readonly Dictionary<string, PoolSystemDecal> decalPool = new();
        private readonly Dictionary<string, PoolSystemParticle> particlePool = new();
        private VolumeProfile currentVolumeProfile = null;
        private Camera mainCamera = null;
        private Camera defaultCamera = null;
        private bool isCameraInitialized = false;

        protected override void Awake()
        {
            base.Awake();

            QualitySettings.maxQueuedFrames = 0;
            DebugManager.instance.enableRuntimeUI = true;
            DebugManager.instance.displayRuntimeUI = false;

            InitializeDecalPool();
            InitializeParticlePool();

            currentVolumeProfile = Instantiate(volumeController.sharedProfile);
            volumeController.profile = currentVolumeProfile;
        }
        private void OnEnable()
        {
            this.WaitUntil(() => ManagerCoreGame.Instance != null, null, () =>
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged += OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged += OnAfterSceneChanged;
            });
        }
        private void OnDisable()
        {
            if (ManagerCoreGame.Instance != null)
            {
                ManagerCoreGame.Instance.OnBeforeSceneChanged -= OnBeforeSceneChanged;
                ManagerCoreGame.Instance.OnAfterSceneChanged -= OnAfterSceneChanged;
            }
        }
        private void OnApplicationQuit() => urpPipelineSettings.renderScale = 1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var item in decalGroup)
            {
                if (item.Prefab == null)
                {
                    continue;
                }

                item.Name = item.Prefab.name;
            }
        }
#endif

        private void OnBeforeSceneChanged(string scene) { ResetDecalPool(); ResetParticlePool(); }
        private void OnAfterSceneChanged(string scene) { }

        private void InitializeDecalPool()
        {
            foreach (var item in decalGroup)
            {
                PoolSystemDecal system = new();
                system.Initialize(item.Prefab, decalContainer, PoolType.RING_BUFFER, item.Prefab.name, item.Count);
                decalPool[item.Prefab.name] = system;
            }
        }
        private void ResetDecalPool()
        {
            foreach (var item in decalPool.Values)
            {
                item.Reset();
            }
        }
        private void InitializeParticlePool()
        {
            foreach (var item in particleGroup)
            {
                PoolSystemParticle system = new();
                system.Initialize(item, particleContainer, PoolType.SINGLE, item.name, 1);
                particlePool[item.name] = system;
            }
        }
        private void ResetParticlePool()
        {
            foreach (var item in particlePool.Values)
            {
                item.Reset();
            }
        }

        public ParticleEmitter SpawnParticle(ParticleEmitter particle, Vector3 position, Quaternion rotation, Vector3 normal)
        {
            if (particle == null)
            {
                LogError("ManagerCoreGraphics.SpawnParticle() particle == null");
                return null;
            }

            if (!particlePool.TryGetValue(particle.name, out PoolSystemParticle pool))
            {
                return null;
            }

            return pool.Spawn(position, rotation, normal);
        }
        public DecalEmitter SpawnDecal(DecalEmitter decal, Transform parent, Vector3 position, Quaternion rotation, float scale)
        {
            if (decal == null)
            {
                LogError("ManagerCoreGraphics.SpawnDecal() decal == null");
                return null;
            }

            if (!decalPool.TryGetValue(decal.name, out PoolSystemDecal pool))
            {
                return null;
            }

            return pool.Spawn(parent, position, rotation, scale);
        }
        public DecalEmitter SpawnDecal(DecalEmitter[] decal, Transform parent, Vector3 position, Quaternion rotation, float scale)
        {
            if (decal == null)
            {
                return null;
            }

            return SpawnDecal(decal[UnityEngine.Random.Range(0, decal.Length)], parent, position, rotation, scale);
        }

        public bool GetVolumeComponent<T>(out T component) where T : VolumeComponent => volumeController.profile.TryGet(typeof(T), out component);

        public UpscalingFilterSelection GetUpscaleFilter() => urpPipelineSettings.upscalingFilter;
        public void SetUpscaleFilter(UpscalingFilterSelection upscaleFilter) => urpPipelineSettings.upscalingFilter = upscaleFilter;

        public float GetRenderScale() => urpPipelineSettings.renderScale;
        public void SetRenderScale(float value) => urpPipelineSettings.renderScale = value;

        public bool GetMultiSampling() => urpPipelineSettings.msaaSampleCount > 1;
        public void SetMultiSampling(bool value)
        {
            urpPipelineSettings.msaaSampleCount = value ? 8 : 1;
        }

        public AntialiasingMode GetAntialiasing()
        {
            if (GetMainCamera() != null)
            {
                if (mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
                {
                    return cameraData.antialiasing;
                }
            }

            return AntialiasingMode.None;
        }
        public void SetAntialiasing(AntialiasingMode antialiasingMode)
        {
            if (GetMainCamera() != null)
            {
                if (mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
                {
                    cameraData.antialiasing = antialiasingMode;
                    cameraData.antialiasingQuality = AntialiasingQuality.High;
                    cameraData.stopNaN = true;
                    cameraData.dithering = true;
                    cameraData.renderPostProcessing = true;
                }
            }
        }

        public int GetFrameRate() => Application.targetFrameRate;
        public void SetFrameRate(int value) => Application.targetFrameRate = value;

        public int GetVysnc() => QualitySettings.vSyncCount;
        public void SetVsync(bool value) => QualitySettings.vSyncCount = value ? 1 : 0;

        public int GetShadowQuality()
        {
            int value = urpPipelineSettings.mainLightShadowmapResolution;

            return urpPipelineSettings.mainLightShadowmapResolution = value == 4096 ? 3 : value == 2048 ? 2 : value == 1024 ? 1 : 0;      
        }
        public void SetShadowQuality(int value)
        {
            if (GetMainCamera() != null)
            {
                if (mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
                {
                    cameraData.renderShadows = value > 0;
                }
            }

            urpPipelineSettings.mainLightShadowmapResolution = value == 3 ? 4096 : value == 2 ? 2048 : value == 1 ? 1024 : 512;
            urpPipelineSettings.additionalLightsShadowmapResolution = value == 3 ? 4096 : value == 2 ? 2048 : value == 1 ? 1024 : 512;
        }

        public int GetTextureQuality() => QualitySettings.globalTextureMipmapLimit;
        public void SetTextureQuality(int quality) => QualitySettings.globalTextureMipmapLimit = 2 - Mathf.Clamp(quality, 0, 2);

        public Resolution GetResolution() => Screen.currentResolution;
        public void SetResolution(int width, int height, FullScreenMode mode)
        {
            Resolution resolution = GetResolution();
            FullScreenMode fullScreenMode = Screen.fullScreenMode;

            if (resolution.width == width && resolution.height == height && fullScreenMode == mode)
            {
                return;
            }

            Screen.SetResolution(width, height, mode);
            OnResolutionChanged?.Invoke(new(width, height));
        }

        private void InitializeMainCamera()
        {
            isCameraInitialized = true;

            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                AudioListener mainListener = FindAnyObjectByType<AudioListener>();

                if (mainListener != null)
                {
                    mainListener.TryGetComponent(out mainCamera);
                }
            }

            if (mainCamera == null)
            {
                LogError("ManagerCoreGraphics.GetMainCamera() mainCamera == null");
            }

            defaultCamera = mainCamera;
        }
        public Camera GetMainCamera()
        {
            if (!isCameraInitialized)
            {
                InitializeMainCamera();
            }

            if (mainCamera == null)
            {
                LogError("ManagerCoreGraphics.GetMainCamera() mainCamera == null");
            }

            return mainCamera;
        }
        public void SetMainCamera(Camera targetCamera)
        {
            if (!isCameraInitialized)
            {
                InitializeMainCamera();
            }

            if (mainCamera == null || defaultCamera == null)
            {
                return;
            }

            if (targetCamera == mainCamera)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = defaultCamera;
            }

            DisableCamera(mainCamera);

            CopyCameraSettings(defaultCamera, targetCamera);

            EnableCamera(targetCamera);

            mainCamera = targetCamera;
        }
        private void EnableCamera(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }    

            if (targetCamera.TryGetComponent(out AudioListener listener))
            {
                listener.enabled = true;
            }

            targetCamera.enabled = true;
        }
        private void DisableCamera(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }

            if (targetCamera.TryGetComponent(out AudioListener listener))
            {
                listener.enabled = false;
            }

            targetCamera.enabled = false;
        }
        private void CopyCameraSettings(Camera sourceCamera, Camera targetCamera)
        {
            if (sourceCamera == null || targetCamera == null)
            {
                return;
            }

            targetCamera.useOcclusionCulling = sourceCamera.useOcclusionCulling;
            targetCamera.cullingMask = sourceCamera.cullingMask;
            targetCamera.clearFlags = sourceCamera.clearFlags;
            targetCamera.backgroundColor = sourceCamera.backgroundColor;

            UniversalAdditionalCameraData sourceData = sourceCamera.GetUniversalAdditionalCameraData();
            UniversalAdditionalCameraData targetData = targetCamera.GetUniversalAdditionalCameraData();

            targetData.renderPostProcessing = sourceData.renderPostProcessing;
            targetData.antialiasing = sourceData.antialiasing;
            targetData.stopNaN = sourceData.stopNaN;
            targetData.dithering = sourceData.dithering;
            targetData.renderShadows = sourceData.renderShadows;
            targetData.requiresColorOption = sourceData.requiresColorOption;
            targetData.requiresDepthOption = sourceData.requiresDepthOption;
        }
    }
}