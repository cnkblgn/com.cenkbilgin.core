using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Core.Graphics;
using Core.Localization;
using Core.UI;
using Core.Audio;

namespace Core.Misc
{
    using static CoreUtility;
    using static TaskUtility;

    [DisallowMultipleComponent]
    public class ManagerGameSettings : Manager<ManagerGameSettings>
    {
        public event Action<GameSettings> OnSettingsUpdated = null;
        public GameSettings Settings => settings;

        [Header("_")]
        [SerializeField] private GameSettings settings = new();

        private SerializeableFile<GameSettings> settingsFile = default;
        private readonly List<UIOptionBase> optionElements = new();
        private bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();

            InitializeSettings();
        }
        private void OnEnable()
        {
            ManagerCoreGame.OnAfterSceneChanged += OnAfterSceneChanged;

            this.WaitUntil(WaitGame, () =>
            {
                ManagerCoreUI.Instance.InsertSettingEvent(Instance.OnSettingsApply, Instance.OnSettingsChanged, Instance.OnSettingsChanged);
            });
        }
        private void OnDisable()
        {
            settingsFile.Save(true);

            ManagerCoreGame.OnAfterSceneChanged -= OnAfterSceneChanged;

            if (ManagerCoreUI.Instance != null)
            {
                ManagerCoreUI.Instance.RemoveSettingEvent(OnSettingsApply, OnSettingsChanged, OnSettingsChanged);
            }
        }

        private void OnAfterSceneChanged(string _) => UpdateSettings();
        private void OnSettingsChanged() => OnSettingsUpdated?.Invoke(settings);
        private void OnSettingsApply() { OnSettingsUpdated?.Invoke(settings); settingsFile.Save(true); }

        private void InitializeSettings()
        {
            if (isInitialized)
            {
                return;
            }

            settingsFile = new("settings");
            settingsFile.Load(true);
            settings = settingsFile.Data;

            isInitialized = true;

            ManagerCoreLocalization local = ManagerCoreLocalization.Instance;
            ManagerCoreUI ui = ManagerCoreUI.Instance;

            local.Set(settings.LanguageIndex);

            string off = local.Get("settings.optionOff");
            string on = local.Get("settings.optionOn");
            string low = local.Get("settings.optionLow");
            string medium = local.Get("settings.optionMedium");
            string high = local.Get("settings.optionHigh");

            ui.InsertSettingHeader(local.Get("settings.graphics"));
            {
                optionElements.Add(ui.InsertSettingButton((int)settings.FullscreenMode, (int)FullScreenMode.FullScreenWindow, local.Get("settings.optionFullscreenMode"), Enum.GetNames(typeof(FullScreenMode)), OnFullscreenModeApply, null));

                optionElements.Add(ui.InsertSettingButton(GetResolutionIndex(settings.ScreenResolution), Screen.resolutions.Length - 1, local.Get("settings.optionScreenResolution"), GetResolutionTypes(), OnResolutionApply, null));

                optionElements.Add(ui.InsertSettingButton(settings.UpscaleFilter, 0, local.Get("settings.optionUpscaleFilter"), Enum.GetValues(typeof(UpscalingFilterSelection)).Cast<UpscalingFilterSelection>().Where(f => f != UpscalingFilterSelection.Auto).Select(f => f.ToString()).ToArray(), OnUpscaleFilterApply, null));

                optionElements.Add(ui.InsertSettingSlider(settings.RenderScale, 1, local.Get("settings.optionRenderScale"), 0.1f, 2.0f, false, OnRenderScaleApply, null));

                optionElements.Add(ui.InsertSettingSlider(settings.FrameLimit, 1, local.Get("settings.optionFrameLimit"), 10F, 360, true, OnFrameLimitApply, null));

                optionElements.Add(ui.InsertSettingButton(settings.AntialiasingMode, 1, local.Get("settings.optionAntialiasing"), new string[] { off, "TAA", "MSAAx2", "MSAAx4", "MSAAx8" }, OnAntialiasingApply, null));

                optionElements.Add(ui.InsertSettingButton(settings.TextureQuality, 2, local.Get("settings.optionTextureQuality"), new string[] { low, medium, high }, OnTextureQualityApply, null));

                optionElements.Add(ui.InsertSettingButton(settings.ShadowQuality, 3, local.Get("settings.optionShadowQuality"), new string[] { off, low, medium, high }, OnShadowQualityApply, null));
            }

            ui.InsertSettingHeader(local.Get("settings.game"));
            {
                optionElements.Add(ui.InsertSettingButton(settings.LanguageIndex, 0, local.Get("settings.optionLanguageIndex"), local.Get(), OnLanguageApply, null));

                optionElements.Add(ui.InsertSettingSlider(settings.CameraFieldOfView, 1, local.Get("settings.optionCameraFieldOfView"), 45f, 90f, true, OnFieldOfViewApply, null));

                optionElements.Add(ui.InsertSettingSlider(settings.CameraSensitivity, 1, local.Get("settings.optionCameraSensitivity"), 0.1f, 10f, false, OnSensitivityApply, null));
            }

            ui.InsertSettingHeader(local.Get("settings.audio"));
            {
                optionElements.Add(ui.InsertSettingSlider(settings.MasterVolume, 1, local.Get("settings.optionMasterVolume"), 0f, 1f, false, OnMasterVolumeApply, null));

                optionElements.Add(ui.InsertSettingSlider(settings.MusicVolume, 1, local.Get("settings.optionMusicVolume"), 0f, 1f, false, OnMusicVolumeApply, null));
            }

            UpdateSettings();
        }
        private void UpdateSettings()
        {
            if (ManagerPlayer.Instance.Player != null)
            {
                ManagerCoreGraphics.Instance.SetMainCamera(ManagerPlayer.Instance.Player.GetComponentInChildren<Camera>());
            }

            foreach (UIOptionBase option in optionElements)
            {
                option.Apply();
            }

            OnSettingsUpdated?.Invoke(settings);
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
        private void OnUpscaleFilterApply(int value)
        {
            settings.UpscaleFilter = value;
            ManagerCoreGraphics.Instance.SetUpscaleFilter((UpscalingFilterSelection)(value + 1));
        }
        private void OnRenderScaleApply(float value)
        {
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
            ManagerCoreLocalization.Instance.Set(value);
        }
        private void OnFieldOfViewApply(float value)
        {
            settings.CameraFieldOfView = value;

            if (ManagerPlayer.Instance.Player != null && ManagerPlayer.Instance.Player.TryGetComponent(out MovementController controller))
            {
                controller.SetFieldOfView(value);
            }
        }
        private void OnSensitivityApply(float value)
        {
            settings.CameraSensitivity = value;

            if (ManagerPlayer.Instance.Player != null && ManagerPlayer.Instance.Player.TryGetComponent(out MovementController controller))
            {
                controller.SetSensitivity(value);
            }
        }
        private void OnMasterVolumeApply(float value)
        {
            settings.MasterVolume = value;
            ManagerCoreAudio.Instance.SetMasterVolumeBase(value);
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