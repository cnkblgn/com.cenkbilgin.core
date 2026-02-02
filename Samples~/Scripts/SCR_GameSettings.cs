using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Core.Misc
{
    using static CoreUtility;

    [Serializable]
    public class GameSettings
    {
        public Int2 ScreenResolution { get => (Int2)screenResolution; set => screenResolution = value; }

        [Header("_")] 
        public FullScreenMode FullscreenMode = FullScreenMode.ExclusiveFullScreen;
        [JsonIgnore, SerializeField] public Vector2Int screenResolution = new(1920, 1080);
        [Range(0.1f, 2.0f)] public float RenderScale = 1.0f;
        [Range(0, 3), Tooltip("0 - Bilinear, 1 - Nearest, 2 - FSR, 3 - STP")] public int UpscaleFilter = 0;
        public bool Vsync = false;
        [Range(24, 240)] public int FrameLimit = 60;

        [Header("_")]
        [Range(0, 2), Tooltip("0 - Off, 1 - TAA, 2 - MSAA x8")] public int AntialiasingMode = 1;
        [Range(0, 2), Tooltip("0 - Low, 1 - Medium, 2 - High")] public int TextureQuality = 3;
        [Range(0, 3), Tooltip("0 - Off, 1 - Low, 2 - Medium, 3 - High")] public int ShadowQuality = 3;

        [Header("_")]
        [Min(0)] public int LanguageIndex = 0;

        [Header("_")]
        [Range(0, 1)] public float MasterVolume = 1.0f;
        [Range(0, 1)] public float MusicVolume = 1.0f;
        [Range(0, 1)] public float EffectsVolume = 1.0f;
        [Range(0, 1)] public float AmbientVolume = 1.0f;

        [Header("_")]
        [Range(45, 90)] public float CameraFieldOfView = 60.0f;
        [Range(0, 10)] public float CameraSensitivity = 0.75f;
    }
}