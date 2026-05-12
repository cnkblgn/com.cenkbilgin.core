using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Core;
using Core.UI;
using Core.Audio;
using Core.Graphics;
using Core.Localization;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerSettings : Manager<ManagerSettings>
    {
        public static event Action<SettingsData> OnUpdated = null;

        [Header("_")]
        [SerializeField] private SettingsData settings = new();

        [Header("_")]
        [SerializeField, Required] private UISettingsController uiController = null;

        private SerializeableFile<SettingsData> thisSettings = default;

        private readonly List<UIOptionBase> optionElements = new();
        private bool isInitialized = false;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET() => OnUpdated = null;

        protected override void Awake()
        {
            base.Awake();

            if (uiController == null)
            {
                throw new NullReferenceException($"ManagerSettings.Awake() [{nameof(uiController)}]");
            }

            InitializeSettings();
        }
        private void OnEnable()
        {
            ManagerCoreGame.OnAfterSceneChanged += OnAfterSceneChanged;

            uiController.OnApply += OnSettingsApply;
            uiController.OnLoad += OnSettingsLoad;
        }
        private void OnDisable()
        {
            ManagerCoreGame.OnAfterSceneChanged -= OnAfterSceneChanged;

            uiController.OnApply -= OnSettingsApply;
            uiController.OnLoad -= OnSettingsLoad;

            thisSettings.Save(true);
        }
        private void Start()
        {
            ManagerCoreLocalization local = ManagerCoreLocalization.Instance;

            local.SetLanguage(settings.LanguageIndex);

            string off = local.Get("settings.optionOff");
            string on = local.Get("settings.optionOn");
            string low = local.Get("settings.optionLow");
            string medium = local.Get("settings.optionMedium");
            string high = local.Get("settings.optionHigh");

            uiController.InsertHeader(local.Get("settings.graphics"));
            {
                optionElements.Add(uiController.InsertButton((int)settings.FullscreenMode, (int)FullScreenMode.FullScreenWindow, local.Get("settings.optionFullscreenMode"), Enum.GetNames(typeof(FullScreenMode)), OnFullscreenModeApply, null));

                optionElements.Add(uiController.InsertButton(GetResolutionIndex(settings.ScreenResolution), Screen.resolutions.Length - 1, local.Get("settings.optionScreenResolution"), GetResolutionTypes(), OnResolutionApply, null));

                optionElements.Add(uiController.InsertToggle(settings.Vsync, false, local.Get("settings.optionVsync"), OnVsyncApply, null));

                optionElements.Add(uiController.InsertButton(settings.UpscaleFilter, 0, local.Get("settings.optionUpscaleFilter"), Enum.GetValues(typeof(UpscalingFilterSelection)).Cast<UpscalingFilterSelection>().Where(f => f != UpscalingFilterSelection.Auto).Select(f => f.ToString()).ToArray(), OnUpscaleFilterApply, null));

                optionElements.Add(uiController.InsertSlider(settings.RenderScale, 1, local.Get("settings.optionRenderScale"), 0.1f, 2.0f, false, OnRenderScaleApply, null));

                optionElements.Add(uiController.InsertSlider(settings.FrameLimit, 1, local.Get("settings.optionFrameLimit"), 10F, 360, true, OnFrameLimitApply, null));

                optionElements.Add(uiController.InsertButton(settings.AntialiasingMode, 2, local.Get("settings.optionAntialiasing"), new string[] { off, "TAA", "MSAAx2", "MSAAx4", "MSAAx8" }, OnAntialiasingApply, null));

                optionElements.Add(uiController.InsertButton(settings.TextureQuality, 2, local.Get("settings.optionTextureQuality"), new string[] { low, medium, high }, OnTextureQualityApply, null));

                optionElements.Add(uiController.InsertButton(settings.ShadowQuality, 3, local.Get("settings.optionShadowQuality"), new string[] { off, low, medium, high }, OnShadowQualityApply, null));
            }

            uiController.InsertHeader(local.Get("settings.game"));
            {
                optionElements.Add(uiController.InsertButton(settings.LanguageIndex, 0, local.Get("settings.optionLanguageIndex"), local.GetLanguages().ToArray(), OnLanguageApply, null));

                optionElements.Add(uiController.InsertSlider(settings.CameraFieldOfView, 1, local.Get("settings.optionCameraFieldOfView"), 45f, 90f, true, OnFieldOfViewApply, null));

                optionElements.Add(uiController.InsertSlider(settings.CameraSensitivity, 1, local.Get("settings.optionCameraSensitivity"), 0.1f, 10f, false, OnSensitivityApply, null));

                optionElements.Add(uiController.InsertToggle(settings.ShowKeys, false, local.Get("settings.optionShowKeys"), OnShowKeysApply, null));
            }

            uiController.InsertHeader(local.Get("settings.audio"));
            {
                optionElements.Add(uiController.InsertSlider(settings.MasterVolume, 1, local.Get("settings.optionMasterVolume"), 0f, 1f, false, OnMasterVolumeApply, null));

                optionElements.Add(uiController.InsertSlider(settings.GameVolume, 1, local.Get("settings.optionGameVolume"), 0f, 1f, false, OnGameVolumeApply, null));

                optionElements.Add(uiController.InsertSlider(settings.MusicVolume, 1, local.Get("settings.optionMusicVolume"), 0f, 1f, false, OnMusicVolumeApply, null));
            }

            UpdateSettings();
        }

        private void InitializeSettings()
        {
            if (isInitialized)
            {
                return;
            }

            thisSettings = new("settings");
            thisSettings.Load(true);

            settings = thisSettings.Data;

            isInitialized = true;
        }
        private void UpdateSettings()
        {
            if (ManagerPlayer.Player != null)
            {
                ManagerCoreGraphics.Instance.SetMainCamera(ManagerPlayer.Player.GetComponentInChildren<Camera>());
            }

            foreach (UIOptionBase option in optionElements)
            {
                option.Apply();
            }

            OnUpdated?.Invoke(settings);
        }

        private void OnAfterSceneChanged(string _)
        {
            UpdateSettings();
        }
        private void OnSettingsLoad()
        {

        }
        private void OnSettingsApply()
        {
            thisSettings.Save(true);
        }

        private void OnFullscreenModeApply(int value)
        {
            settings.FullscreenMode = (FullScreenMode)value;

            ManagerCoreGraphics.Instance.SetResolution(settings.ScreenResolution.x, settings.ScreenResolution.y, settings.FullscreenMode);
        }
        private void OnResolutionApply(int value)
        {
            settings.ScreenResolution = GetResolutionValue(value);

            ManagerCoreGraphics.Instance.SetResolution(settings.ScreenResolution.x, settings.ScreenResolution.y, settings.FullscreenMode);
        }
        private void OnVsyncApply(bool value)
        {
            settings.Vsync = value;

            ManagerCoreGraphics.Instance.SetVsync(value);
        }
        private void OnUpscaleFilterApply(int value)
        {
            settings.UpscaleFilter = value;

            ManagerCoreGraphics.Instance.SetUpscaleFilter((UpscalingFilterSelection)(value + 1));
        }
        private void OnRenderScaleApply(float value)
        {
            value = Mathf.Round(value * 100f) / 100f;

            settings.RenderScale = value;

            ManagerCoreGraphics.Instance.SetRenderScale(value);
        }
        private void OnFrameLimitApply(float value)
        {
            settings.FrameLimit = (int)value;

            ManagerCoreGraphics.Instance.SetFrameRate((int)value);
        }
        private void OnAntialiasingApply(int value)
        {
            settings.AntialiasingMode = value;

            ManagerCoreGraphics.Instance.SetAntialiasing(value == 1 ? AntialiasingMode.TemporalAntiAliasing : AntialiasingMode.SubpixelMorphologicalAntiAliasing);
            ManagerCoreGraphics.Instance.SetMultiSampling(value >= 2 ? (int)Mathf.Pow(2, value - 1) : 1);
        }
        private void OnTextureQualityApply(int value)
        {
            settings.TextureQuality = value;

            ManagerCoreGraphics.Instance.SetTextureQuality(value);
        }
        private void OnShadowQualityApply(int value)
        {
            settings.ShadowQuality = value;

            ManagerCoreGraphics.Instance.SetShadowQuality(value);
        }
        private void OnLanguageApply(int value)
        {
            settings.LanguageIndex = value;

            ManagerCoreLocalization.Instance.SetLanguage(value);
        }
        private void OnFieldOfViewApply(float value)
        {
            settings.CameraFieldOfView = value;

            if (ManagerPlayer.Player != null && ManagerPlayer.Player.TryGetComponent(out MovementController controller))
            {
                controller.SetFieldOfView(value);
            }
        }
        private void OnSensitivityApply(float value)
        {
            settings.CameraSensitivity = value;

            if (ManagerPlayer.Player != null && ManagerPlayer.Player.TryGetComponent(out MovementController controller))
            {
                controller.SetSensitivity(value);
            }
        }
        private void OnShowKeysApply(bool value)
        {
            settings.ShowKeys = value;
        }
        private void OnMasterVolumeApply(float value)
        {
            settings.MasterVolume = value;

            ManagerCoreAudio.Instance.SetMasterVolumeBase(value);
        }
        private void OnGameVolumeApply(float value)
        {
            settings.GameVolume = value;

            ManagerCoreAudio.Instance.SetGameVolumeBase(value);
        }
        private void OnMusicVolumeApply(float value)
        {
            settings.MusicVolume = value;

            ManagerCoreAudio.Instance.SetMusicVolumeBase(value);
        }
       
        private string[] GetResolutionTypes()
        {
            return Screen.resolutions.Select(r => $"{r.width}x{r.height} @{r.refreshRateRatio.value}Hz").ToArray();
        }
        private int GetResolutionIndex(Int2 resolution)
        {
            Resolution[] availableResolutions = Screen.resolutions;

            var sorted = availableResolutions.OrderBy(r => r.width).ThenBy(r => r.height).ToArray();
            var index = Array.FindIndex(sorted, res => res.width == resolution.x && res.height == resolution.y);

            if (index < 0)
            {
                Resolution current = Screen.currentResolution;
                index = Array.FindIndex(sorted, res => res.width == current.width && res.height == current.height);

                if (index < 0)
                {
                    index = sorted.Length - 1;
                }
            }

            return index;
        }
        private Int2 GetResolutionValue(int index)
        {
            Resolution[] availableResolutions = Screen.resolutions;

            Resolution currentResolution = availableResolutions[Mathf.Clamp(index, 0, availableResolutions.Length - 1)];

            return new(currentResolution.width, currentResolution.height);
        }
    }
}
