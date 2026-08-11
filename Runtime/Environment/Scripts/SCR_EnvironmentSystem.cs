using UnityEngine;

namespace Core.Environment
{
    [ExecuteAlways]
    public sealed class EnvironmentSystem : MonoBehaviour
    {
        private const float UPDATE_FPS = 24;
        private const float UPDATE_RATE = 1 / UPDATE_FPS;

        [Header("_")]
        [SerializeField, Required] private Light directionalLight;
        [SerializeField] private EnvironmentSettings settings;

        private static EnvironmentSettings cachedSettings;
        private static Texture2D cachedFogTexture;
        private static Texture2D cachedCloudTexture;
        private static Texture2D cachedCloudTextureB;
        private static Vector3 sunDirection;
        private static Vector3 moonDirection;
        private static readonly int _FOG_COLOR = Shader.PropertyToID("_FOG_COLOR");
        private static readonly int _FOG_DENSITY = Shader.PropertyToID("_FOG_DENSITY");
        private static readonly int _FOG_DISTANCE_START = Shader.PropertyToID("_FOG_DISTANCE_START");
        private static readonly int _FOG_DISTANCE_FALLOFF = Shader.PropertyToID("_FOG_DISTANCE_FALLOFF");
        private static readonly int _FOG_HEIGHT_START = Shader.PropertyToID("_FOG_HEIGHT_START");
        private static readonly int _FOG_HEIGHT_FALLOFF = Shader.PropertyToID("_FOG_HEIGHT_FALLOFF");
        private static readonly int _FOG_SCATTERING = Shader.PropertyToID("_FOG_SCATTERING");
        private static readonly int _FOG_NOISE_TEX = Shader.PropertyToID("_FOG_NOISE_TEX");
        private static readonly int _FOG_NOISE_STRENGTH = Shader.PropertyToID("_FOG_NOISE_STRENGTH");
        private static readonly int _FOG_NOISE_SCALE = Shader.PropertyToID("_FOG_NOISE_SCALE");
        private static readonly int _FOG_NOISE_SPEED = Shader.PropertyToID("_FOG_NOISE_SPEED");
        private static readonly int _ZENITH_COLOR = Shader.PropertyToID("_ZENITH_COLOR");
        private static readonly int _HORIZON_COLOR = Shader.PropertyToID("_HORIZON_COLOR");
        private static readonly int _HORIZON_THICKNESS = Shader.PropertyToID("_HORIZON_THICKNESS");
        private static readonly int _HORIZON_SOFTNESS = Shader.PropertyToID("_HORIZON_SOFTNESS");
        private static readonly int _SUN_COLOR = Shader.PropertyToID("_SUN_COLOR");
        private static readonly int _SUN_DIRECTION = Shader.PropertyToID("_SUN_DIRECTION");
        private static readonly int _SUN_SIZE = Shader.PropertyToID("_SUN_SIZE");
        private static readonly int _SUN_GLOW = Shader.PropertyToID("_SUN_GLOW");
        private static readonly int _MOON_COLOR = Shader.PropertyToID("_MOON_COLOR");
        private static readonly int _MOON_DIRECTION = Shader.PropertyToID("_MOON_DIRECTION");
        private static readonly int _MOON_SIZE = Shader.PropertyToID("_MOON_SIZE");
        private static readonly int _MOON_GLOW = Shader.PropertyToID("_MOON_GLOW");
        private static readonly int _CLOUD_TEX_A = Shader.PropertyToID("_CLOUD_TEX_A");
        private static readonly int _CLOUD_TEX_B = Shader.PropertyToID("_CLOUD_TEX_B");
        private static readonly int _CLOUD_BLEND = Shader.PropertyToID("_CLOUD_BLEND");
        private static readonly int _CLOUD_TINT = Shader.PropertyToID("_CLOUD_TINT");
        private static readonly int _CLOUD_COVERAGE = Shader.PropertyToID("_CLOUD_COVERAGE");
        private static readonly int _CLOUD_OPACITY = Shader.PropertyToID("_CLOUD_OPACITY");
        private static readonly int _CLOUD_FADE = Shader.PropertyToID("_CLOUD_FADE");
        private static readonly int _CLOUD_HEIGHT = Shader.PropertyToID("_CLOUD_HEIGHT");
        private static readonly int _CLOUD_CURVE = Shader.PropertyToID("_CLOUD_CURVE");
        private static readonly int _CLOUD_SCALE = Shader.PropertyToID("_CLOUD_SCALE");
        private static readonly int _CLOUD_SPEED = Shader.PropertyToID("_CLOUD_SPEED");
        private static readonly int _CLOUD_TURBULENCE = Shader.PropertyToID("_CLOUD_TURBULENCE");
        private static readonly int _CLOUD_TRANSMITTANCE = Shader.PropertyToID("_CLOUD_TRANSMITTANCE");
        private static readonly int _CLOUD_DARKNESS = Shader.PropertyToID("_CLOUD_DARKNESS");
        private static readonly int _CLOUD_RIM_WIDTH = Shader.PropertyToID("_CLOUD_RIM_WIDTH");
        private static readonly int _CLOUD_RIM_STRENGTH = Shader.PropertyToID("_CLOUD_RIM_STRENGTH");
        private static bool hasInitialized;
        private float timer;

        private void Awake()
        {
            cachedSettings = settings;
            hasInitialized = true;
        }
        private void Update()
        {
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
        private void OnDestroy() => hasInitialized = false;

        public static EnvironmentSettings GetSettings()
        {
            if (!hasInitialized)
            {
                Debug.LogError("EnvironmentSystem is not found in scene! please assign it!");
                return EnvironmentSettings.Default;
            }

            return cachedSettings;
        }

        private void Refresh()
        {
            ApplyCelestial(settings, directionalLight);
            ApplyAmbient(settings);
            ApplyFog(settings);
            ApplySky(settings);
        }

        private static void ApplyCelestial(EnvironmentSettings settings, Light directionalLight)
        {
            if (directionalLight == null)
            {
                Debug.LogWarning("Please assign directional light!");
                return;
            }

            Vector3 direction = -directionalLight.transform.forward;
            bool isDay = Vector3.Dot(direction, Vector3.up) > -0.1f;

            sunDirection = direction;
            moonDirection = -direction;

            directionalLight.color = isDay ? settings.Sun.Color : settings.Moon.Color;
            directionalLight.intensity = isDay ? settings.Sun.Intensity : settings.Moon.Intensity;
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

            Shader.SetGlobalColor(_FOG_COLOR, fog.Color);
            Shader.SetGlobalFloat(_FOG_DENSITY, fog.Density);
            Shader.SetGlobalFloat(_FOG_DISTANCE_START, fog.DistanceStart);
            Shader.SetGlobalFloat(_FOG_DISTANCE_FALLOFF, fog.DistanceFalloff);
            Shader.SetGlobalFloat(_FOG_HEIGHT_START, fog.HeightStart);
            Shader.SetGlobalFloat(_FOG_HEIGHT_FALLOFF, fog.HeightFalloff);
            Shader.SetGlobalFloat(_FOG_SCATTERING, fog.Scattering);

            if (cachedFogTexture != texture)
            {
                Shader.SetGlobalTexture(_FOG_NOISE_TEX, texture);
                cachedFogTexture = texture;
            }

            Shader.SetGlobalFloat(_FOG_NOISE_STRENGTH, fog.NoiseStrength);
            Shader.SetGlobalFloat(_FOG_NOISE_SPEED, fog.NoiseTurbulance);
            Shader.SetGlobalFloat(_FOG_NOISE_SCALE, fog.NoiseScale);
        }
        private static void ApplySky(EnvironmentSettings settings)
        {
            SkySettings sky = settings.Sky;
            CelestialSettings sun = settings.Sun;
            CelestialSettings moon = settings.Moon;
            CloudSettings cloud = settings.Cloud;

            Texture2D cloudTexture = settings.CloudTexture;
            Texture2D cloudTextureB = settings.CloudTextureB != null ? settings.CloudTextureB : settings.CloudTexture;

            Shader.SetGlobalColor(_ZENITH_COLOR, sky.ZenithColor);
            Shader.SetGlobalColor(_HORIZON_COLOR, sky.HorizonColor);
            Shader.SetGlobalFloat(_HORIZON_THICKNESS, sky.HorizonThickness);
            Shader.SetGlobalFloat(_HORIZON_SOFTNESS, sky.HorizonSmoothness);
            Shader.SetGlobalColor(_SUN_COLOR, sun.Color * sun.Power);
            Shader.SetGlobalVector(_SUN_DIRECTION, -sunDirection);
            Shader.SetGlobalFloat(_SUN_SIZE, sun.Size);
            Shader.SetGlobalFloat(_SUN_GLOW, sun.Glow);
            Shader.SetGlobalColor(_MOON_COLOR, moon.Color * moon.Power);
            Shader.SetGlobalVector(_MOON_DIRECTION, -moonDirection);
            Shader.SetGlobalFloat(_MOON_SIZE, moon.Size);
            Shader.SetGlobalFloat(_MOON_GLOW, moon.Glow);

            if (cachedCloudTexture != cloudTexture)
            {
                Shader.SetGlobalTexture(_CLOUD_TEX_A, cloudTexture);
                cachedCloudTexture = cloudTexture;
            }

            if (cachedCloudTextureB != cloudTextureB)
            {
                Shader.SetGlobalTexture(_CLOUD_TEX_B, cloudTextureB);
                cachedCloudTextureB = cloudTextureB;
            }

            Shader.SetGlobalFloat(_CLOUD_BLEND, settings.Blend);
            Shader.SetGlobalColor(_CLOUD_TINT, cloud.Tint);
            Shader.SetGlobalFloat(_CLOUD_COVERAGE, cloud.Coverage);
            Shader.SetGlobalFloat(_CLOUD_OPACITY, cloud.Opacity);
            Shader.SetGlobalFloat(_CLOUD_FADE, cloud.Fade);
            Shader.SetGlobalFloat(_CLOUD_HEIGHT, cloud.Height);
            Shader.SetGlobalFloat(_CLOUD_CURVE, cloud.Curve);
            Shader.SetGlobalFloat(_CLOUD_SCALE, cloud.Scale);
            Shader.SetGlobalFloat(_CLOUD_SPEED, cloud.Speed);
            Shader.SetGlobalFloat(_CLOUD_TURBULENCE, cloud.Turbulance);
            Shader.SetGlobalFloat(_CLOUD_TRANSMITTANCE, cloud.Transmittance);
            Shader.SetGlobalFloat(_CLOUD_DARKNESS, cloud.Darkness);
            Shader.SetGlobalFloat(_CLOUD_RIM_WIDTH, cloud.RimWidth);
            Shader.SetGlobalFloat(_CLOUD_RIM_STRENGTH, cloud.RimStrength);
        }
    }
}