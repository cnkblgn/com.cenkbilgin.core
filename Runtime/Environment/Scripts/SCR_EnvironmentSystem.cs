using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Environment
{
    [ExecuteAlways]
    public sealed class EnvironmentSystem : MonoBehaviour
    {
        private const float LIGHT_THRESHOLD = 0.0025f;
        private const float SHADOW_THRESHOLD = 0.050f;

        [Header("_")]
        [SerializeField, Required] private Light sunLight; 
        [SerializeField, Required] private Light moonLight;

        private static Light cachedSunLight;
        private static Light cachedMoonLight;
        private static bool hasInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            cachedSunLight = null;
            cachedMoonLight = null;
            hasInitialized = false;
        }

        private void Update()
        {
            cachedSunLight = sunLight;
            cachedMoonLight = moonLight;
            hasInitialized = true;

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                Refresh();
                return;
            }
#endif

            Refresh();
        }
        private void Reset() => OnRuntimeInitialize();
        private void OnDisable() => OnRuntimeInitialize();

        public static bool TryGetSettings(out Environment environment)
        {
            VolumeStack stack = VolumeManager.instance.stack;

            if (stack == null)
            {
                environment = null;
                return false;
            }

            environment = stack.GetComponent<Environment>();

            return environment != null;
        }
        public static bool TryGetSun(out Light sun)
        {
            sun = cachedSunLight;
            return hasInitialized && sun != null;
        }
        public static bool TryGetMoon(out Light moon)
        {
            moon = cachedMoonLight;
            return hasInitialized && moon != null;
        }

        private void Refresh()
        {
            if (!TryGetSettings(out Environment environment))
            {
                return;
            }

            static float getHeightIntensity(float directionY)
            {
                float height = Mathf.Clamp01(directionY);
                return height * height * (3f - 2f * height);
            }

            if (cachedSunLight != null)
            {
                Vector3 sunDirection = -cachedSunLight.transform.forward;
                environment.SunDirection.value = sunDirection;

                float multiplier = getHeightIntensity(sunDirection.y);
                float intensity = environment.SunIntensity.value * multiplier;

                cachedSunLight.color = environment.SunColor.value;
                cachedSunLight.intensity = intensity;
                cachedSunLight.enabled = intensity > LIGHT_THRESHOLD;
                cachedSunLight.shadows = intensity >= SHADOW_THRESHOLD ? LightShadows.Soft : LightShadows.None;
            }

            if (cachedMoonLight != null)
            {
                Vector3 moonDirection = -cachedMoonLight.transform.forward;
                environment.MoonDirection.value = moonDirection;

                float multiplier = getHeightIntensity(moonDirection.y);
                float intensity = environment.MoonIntensity.value * multiplier;

                cachedMoonLight.color = environment.MoonColor.value;
                cachedMoonLight.intensity = intensity;
                cachedMoonLight.enabled = intensity > LIGHT_THRESHOLD;
                cachedMoonLight.shadows = intensity >= SHADOW_THRESHOLD ? LightShadows.Soft : LightShadows.None;
            }
        }
    }
}
