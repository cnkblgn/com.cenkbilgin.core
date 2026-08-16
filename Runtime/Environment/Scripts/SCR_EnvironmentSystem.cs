using UnityEngine;

namespace Core.Environment
{
    [ExecuteAlways]
    public sealed class EnvironmentSystem : MonoBehaviour
    {
        private const float UPDATE_FPS = 24;
        private const float UPDATE_RATE = 1 / UPDATE_FPS;
        private const float LIGHT_THRESHOLD = 0.0025f;
        private const float SHADOW_THRESHOLD = 0.050f;

        [Header("_")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        [SerializeField] private EnvironmentSettings settings = new();

        internal static EnvironmentSettings cachedSettings = new();
        private static Texture2D cachedFogTexture;
        private static Texture2D cachedCloudTexture;
        private static Texture2D cachedCloudTextureB;
        private static Light cachedSunLight;
        private static Light cachedMoonLight;
        private static Vector3 sunDirection;
        private static Vector3 moonDirection;
        private static bool hasInitialized;
        private float timer;

        internal static readonly int _FOG_COLOR_ID = Shader.PropertyToID("_FOG_COLOR");
        internal static readonly int _FOG_DENSITY_ID = Shader.PropertyToID("_FOG_DENSITY");
        internal static readonly int _FOG_DISTANCE_START_ID = Shader.PropertyToID("_FOG_DISTANCE_START");
        internal static readonly int _FOG_DISTANCE_FALLOFF_ID = Shader.PropertyToID("_FOG_DISTANCE_FALLOFF");
        internal static readonly int _FOG_HEIGHT_START_ID = Shader.PropertyToID("_FOG_HEIGHT_START");
        internal static readonly int _FOG_HEIGHT_FALLOFF_ID = Shader.PropertyToID("_FOG_HEIGHT_FALLOFF");
        internal static readonly int _FOG_SCATTERING_ID = Shader.PropertyToID("_FOG_SCATTERING");
        internal static readonly int _FOG_NOISE_TEX_ID = Shader.PropertyToID("_FOG_NOISE_TEX");
        internal static readonly int _FOG_NOISE_STRENGTH_ID = Shader.PropertyToID("_FOG_NOISE_STRENGTH");
        internal static readonly int _FOG_NOISE_SCALE_ID = Shader.PropertyToID("_FOG_NOISE_SCALE");
        internal static readonly int _FOG_NOISE_SPEED_ID = Shader.PropertyToID("_FOG_NOISE_SPEED");
        internal static readonly int _ZENITH_COLOR_ID = Shader.PropertyToID("_ZENITH_COLOR");
        internal static readonly int _HORIZON_COLOR_ID = Shader.PropertyToID("_HORIZON_COLOR");
        internal static readonly int _HORIZON_THICKNESS_ID = Shader.PropertyToID("_HORIZON_THICKNESS");
        internal static readonly int _HORIZON_SOFTNESS_ID = Shader.PropertyToID("_HORIZON_SOFTNESS");
        internal static readonly int _SUN_COLOR_ID = Shader.PropertyToID("_SUN_COLOR");
        internal static readonly int _SUN_DIRECTION_ID = Shader.PropertyToID("_SUN_DIRECTION");
        internal static readonly int _SUN_SIZE_ID = Shader.PropertyToID("_SUN_SIZE");
        internal static readonly int _SUN_GLOW_ID = Shader.PropertyToID("_SUN_GLOW");
        internal static readonly int _MOON_COLOR_ID = Shader.PropertyToID("_MOON_COLOR");
        internal static readonly int _MOON_DIRECTION_ID = Shader.PropertyToID("_MOON_DIRECTION");
        internal static readonly int _MOON_SIZE_ID = Shader.PropertyToID("_MOON_SIZE");
        internal static readonly int _MOON_GLOW_ID = Shader.PropertyToID("_MOON_GLOW");
        internal static readonly int _CLOUD_TEX_A_ID = Shader.PropertyToID("_CLOUD_TEX_A");
        internal static readonly int _CLOUD_TEX_B_ID = Shader.PropertyToID("_CLOUD_TEX_B");
        internal static readonly int _CLOUD_BLEND_ID = Shader.PropertyToID("_CLOUD_BLEND");
        internal static readonly int _CLOUD_TINT_ID = Shader.PropertyToID("_CLOUD_TINT");
        internal static readonly int _CLOUD_COVERAGE_ID = Shader.PropertyToID("_CLOUD_COVERAGE");
        internal static readonly int _CLOUD_OPACITY_ID = Shader.PropertyToID("_CLOUD_OPACITY");
        internal static readonly int _CLOUD_FADE_ID = Shader.PropertyToID("_CLOUD_FADE");
        internal static readonly int _CLOUD_HEIGHT_ID = Shader.PropertyToID("_CLOUD_HEIGHT");
        internal static readonly int _CLOUD_CURVE_ID = Shader.PropertyToID("_CLOUD_CURVE");
        internal static readonly int _CLOUD_SCALE_ID = Shader.PropertyToID("_CLOUD_SCALE");
        internal static readonly int _CLOUD_SPEED_ID = Shader.PropertyToID("_CLOUD_SPEED");
        internal static readonly int _CLOUD_TURBULENCE_ID = Shader.PropertyToID("_CLOUD_TURBULENCE");
        internal static readonly int _CLOUD_TRANSMITTANCE_ID = Shader.PropertyToID("_CLOUD_TRANSMITTANCE");
        internal static readonly int _CLOUD_DARKNESS_ID = Shader.PropertyToID("_CLOUD_DARKNESS");
        internal static readonly int _CLOUD_RIM_WIDTH_ID = Shader.PropertyToID("_CLOUD_RIM_WIDTH");
        internal static readonly int _CLOUD_RIM_STRENGTH_ID = Shader.PropertyToID("_CLOUD_RIM_STRENGTH");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            cachedSettings = new();
            cachedFogTexture = null;
            cachedCloudTexture = null;
            cachedCloudTextureB = null;
            cachedSunLight = null;
            cachedMoonLight = null;
            hasInitialized = false;
        }

        private void Update()
        {
            cachedSunLight = sunLight;
            cachedMoonLight = moonLight;

            if (!hasInitialized)
            {
                cachedSettings = settings;
                hasInitialized = true;
            }

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                Refresh();
                return;
            }
#endif

            timer += Time.deltaTime;

            if (timer >= UPDATE_RATE)
            {
                timer %= UPDATE_RATE;
                Refresh();
            }
        }
        private void OnDisable() => OnRuntimeInitialize();

        private static bool IsValid()
        {
            if (!hasInitialized)
            {
                Debug.LogWarning("EnvironmentSystem is not found in scene! please assign it!");
            }

            return hasInitialized;
        }
        public static bool TryGetSun(out Light sun)
        {
            sun = cachedSunLight;

            return IsValid();
        }
        public static bool TryGetMoon(out Light moon)
        {
            moon = cachedMoonLight;

            return IsValid();
        }
        public static bool TryGetSettings(out EnvironmentSettings settings)
        {
            settings = cachedSettings;

            return IsValid();
        }

        private void Refresh()
        {
            ApplyCelestial(settings);
            ApplyAmbient(settings);
            ApplyFog(settings);
            ApplySky(settings);
        }
        private static void ApplyCelestial(EnvironmentSettings settings)
        {
            static float GetIntensity(float direction)
            {
                float height = Mathf.Clamp01(direction);
                return height * height * (3f - 2f * height);
            }

            if (cachedSunLight != null)
            {
                sunDirection = -cachedSunLight.transform.forward;

                float multiplier = GetIntensity(sunDirection.y);
                float intensity = settings.Sun.Intensity * multiplier;

                cachedSunLight.color = settings.Sun.Color;
                cachedSunLight.intensity = intensity;
                cachedSunLight.enabled = intensity > LIGHT_THRESHOLD;
                cachedSunLight.shadows = intensity >= SHADOW_THRESHOLD ? LightShadows.Soft : LightShadows.None;
            }

            if (cachedMoonLight != null)
            {
                moonDirection = -cachedMoonLight.transform.forward;

                float multiplier = GetIntensity(moonDirection.y);
                float intensity = settings.Moon.Intensity * multiplier;

                cachedMoonLight.color = settings.Moon.Color;
                cachedMoonLight.intensity = intensity;
                cachedMoonLight.enabled = intensity > LIGHT_THRESHOLD;
                cachedMoonLight.shadows = intensity >= SHADOW_THRESHOLD ? LightShadows.Soft : LightShadows.None;
            }
        }
        private static void ApplyAmbient(EnvironmentSettings settings)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = settings.Ambient.SkyColor;
            RenderSettings.ambientEquatorColor = settings.Ambient.EquatorColor;
            RenderSettings.ambientGroundColor = settings.Ambient.GroundColor;
        }
        private static void ApplyFog(EnvironmentSettings settings)
        {
            FogSettings fog = settings.Fog;
            Texture2D texture = settings.FogTexture;

            if (cachedFogTexture != texture)
            {
                Shader.SetGlobalTexture(_FOG_NOISE_TEX_ID, texture);
                cachedFogTexture = texture;
            }

            Shader.SetGlobalColor(_FOG_COLOR_ID, fog.Color);
            Shader.SetGlobalFloat(_FOG_DENSITY_ID, fog.Density);
            Shader.SetGlobalFloat(_FOG_DISTANCE_START_ID, fog.DistanceStart);
            Shader.SetGlobalFloat(_FOG_DISTANCE_FALLOFF_ID, fog.DistanceFalloff);
            Shader.SetGlobalFloat(_FOG_HEIGHT_START_ID, fog.HeightStart);
            Shader.SetGlobalFloat(_FOG_HEIGHT_FALLOFF_ID, fog.HeightFalloff);
            Shader.SetGlobalFloat(_FOG_SCATTERING_ID, fog.Scattering);
            Shader.SetGlobalFloat(_FOG_NOISE_STRENGTH_ID, fog.NoiseStrength);
            Shader.SetGlobalFloat(_FOG_NOISE_SPEED_ID, fog.NoiseTurbulance);
            Shader.SetGlobalFloat(_FOG_NOISE_SCALE_ID, fog.NoiseScale);
        }
        private static void ApplySky(EnvironmentSettings settings)
        {
            SkySettings sky = settings.Sky;
            CelestialSettings sun = settings.Sun;
            CelestialSettings moon = settings.Moon;
            CloudSettings cloud = settings.Cloud;

            Texture2D cloudTextureA = settings.CloudTexture;
            Texture2D cloudTextureB = settings.CloudTextureB != null ? settings.CloudTextureB : settings.CloudTexture;

            if (cachedCloudTexture != cloudTextureA)
            {
                Shader.SetGlobalTexture(_CLOUD_TEX_A_ID, cloudTextureA);
                cachedCloudTexture = cloudTextureA;
            }

            if (cachedCloudTextureB != cloudTextureB)
            {
                Shader.SetGlobalTexture(_CLOUD_TEX_B_ID, cloudTextureB);
                cachedCloudTextureB = cloudTextureB;
            }

            Shader.SetGlobalColor(_ZENITH_COLOR_ID, sky.ZenithColor);
            Shader.SetGlobalColor(_HORIZON_COLOR_ID, sky.HorizonColor);
            Shader.SetGlobalFloat(_HORIZON_THICKNESS_ID, sky.HorizonThickness);
            Shader.SetGlobalFloat(_HORIZON_SOFTNESS_ID, sky.HorizonSmoothness);
            Shader.SetGlobalColor(_SUN_COLOR_ID, sun.Color * sun.Power);
            Shader.SetGlobalVector(_SUN_DIRECTION_ID, -sunDirection);
            Shader.SetGlobalFloat(_SUN_SIZE_ID, sun.Size);
            Shader.SetGlobalFloat(_SUN_GLOW_ID, sun.Glow);
            Shader.SetGlobalColor(_MOON_COLOR_ID, moon.Color * moon.Power);
            Shader.SetGlobalVector(_MOON_DIRECTION_ID, -moonDirection);
            Shader.SetGlobalFloat(_MOON_SIZE_ID, moon.Size);
            Shader.SetGlobalFloat(_MOON_GLOW_ID, moon.Glow);

            Shader.SetGlobalFloat(_CLOUD_BLEND_ID, settings.Blend);
            Shader.SetGlobalColor(_CLOUD_TINT_ID, cloud.Tint);
            Shader.SetGlobalFloat(_CLOUD_COVERAGE_ID, cloud.Coverage);
            Shader.SetGlobalFloat(_CLOUD_OPACITY_ID, cloud.Opacity);
            Shader.SetGlobalFloat(_CLOUD_FADE_ID, cloud.Fade);
            Shader.SetGlobalFloat(_CLOUD_HEIGHT_ID, cloud.Height);
            Shader.SetGlobalFloat(_CLOUD_CURVE_ID, cloud.Curve);
            Shader.SetGlobalFloat(_CLOUD_SCALE_ID, cloud.Scale);
            Shader.SetGlobalFloat(_CLOUD_SPEED_ID, cloud.Speed);
            Shader.SetGlobalFloat(_CLOUD_TURBULENCE_ID, cloud.Turbulance);
            Shader.SetGlobalFloat(_CLOUD_TRANSMITTANCE_ID, cloud.Transmittance);
            Shader.SetGlobalFloat(_CLOUD_DARKNESS_ID, cloud.Darkness);
            Shader.SetGlobalFloat(_CLOUD_RIM_WIDTH_ID, cloud.RimWidth);
            Shader.SetGlobalFloat(_CLOUD_RIM_STRENGTH_ID, cloud.RimStrength);
        }
    }
}