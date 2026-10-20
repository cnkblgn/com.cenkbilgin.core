using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Environment
{
    public enum FogResolution : int
    {
        FULL,
        HALF,
        QUARTER,
        EIGHT,
    }

    [Serializable]
    [VolumeComponentMenu("Environment")]
    public sealed class Environment : VolumeComponent
    {
        public BoolParameter Enabled = new(false, true);

        [Header("Ambient")]
        public ColorParameter AmbientSkyColor = new(Color.aliceBlue, true, false, true);
        public ColorParameter AmbientEquatorColor = new(Color.whiteSmoke, true, false, true);
        public ColorParameter AmbientGroundColor = new(Color.gray, true, false, true);

        [Header("Sky")]
        public ColorParameter SkyZenithColor = new(new(0, 0.15f, 0.50f), true, false, true);
        public ColorParameter SkyHorizonColor = new(new(0.3f, 0.4f, 0.5f), true, false, true);
        public ClampedFloatParameter SkyHorizonThickness = new(0, 0, 1);
        public ClampedFloatParameter SkyHorizonSmoothness = new(1f, 0, 1);

        [Header("Stars")]
        public ColorParameter StarColor = new(Color.white);
        public ClampedFloatParameter StarAmount = new(0, 0, 1);
        public ClampedFloatParameter StarSize = new(0.10f, 0, 1);
        public MinFloatParameter StarDensity = new(256, 0);
        public MinFloatParameter StarBrightness = new(4f, 0);
        public ClampedFloatParameter StarAnimationStrength = new(0.75f, 0, 1);
        public MinFloatParameter StarAnimationSpeed = new(2f, 0);

        [Header("Sun")]
        public ColorParameter SunColor = new(Color.softYellow);
        public Vector3Parameter SunDirection = new(new(0.5f, 0.25f, 0f));
        public MinFloatParameter SunIntensity = new(1, 0);
        public MinFloatParameter SunPower = new(12, 0);
        public MinFloatParameter SunSize = new(0.001f, 0);
        public MinFloatParameter SunGlow = new(4096, 1);

        [Header("Moon")]
        public ColorParameter MoonColor = new(Color.black);
        public Vector3Parameter MoonDirection = new(new(-0.5f, -0.25f, 0f));
        public MinFloatParameter MoonIntensity = new(0, 0);
        public MinFloatParameter MoonPower = new(12, 0);
        public MinFloatParameter MoonSize = new(0.001f, 0);
        public MinFloatParameter MoonGlow = new(4096, 1);

        [Header("Clouds")]
        [Info("R -> Mask, G -> +Distortion, B -> -Distortion")]
        public TextureParameter CloudBaseTexture = new(default, true);
        public TextureParameter CloudTargetTexture = new(default, true);
        public ClampedFloatParameter CloudBlend = new(0f, 0, 1);
        public ColorParameter CloudColor = new(Color.wheat, false, false, true);
        public ClampedFloatParameter CloudOpacity = new(1, 0, 1);
        public ClampedFloatParameter CloudCoverage = new(1, 0, 1);
        public ClampedFloatParameter CloudFade = new(1, 0, 1);
        public MinFloatParameter CloudHeight = new(8, 0);
        public MinFloatParameter CloudCurve = new(128, 0);
        public ClampedFloatParameter CloudScale = new(0.15f, 0, 1);
        public MinFloatParameter CloudSpeed = new(0.5f, 0);
        public MinFloatParameter CloudTurbulance = new(0.1f, 0);
        public ClampedFloatParameter CloudTransmittance = new(0.15f, 0, 4);
        public ClampedFloatParameter CloudDarkness = new(0.50f, 0, 1);
        public ClampedFloatParameter CloudRimWidth = new(0.035f, 0, 1);
        public MinFloatParameter CloudRimStrength = new(5, 0);

        [Header("Fog")]
        public EnumParameter<FogResolution> FogResolution = new(Core.Environment.FogResolution.HALF);
        public MinFloatParameter FogBlurStrength = new(1.5f, 0);
        public ClampedFloatParameter FogBlurRadius = new(4, 1, 8);
        public ColorParameter FogColor = new(Color.wheat, false, false, true);
        public ClampedFloatParameter FogDensity = new(1, 0, 1);
        public MinFloatParameter FogDistanceStart = new(256f, 0);
        public MinFloatParameter FogDistanceFalloff = new(1.0f, 0);
        public MinFloatParameter FogHeightStart = new(512f, 0);
        public MinFloatParameter FogHeightFalloff = new(1.0f, 0);
        public ClampedFloatParameter FogScattering = new(1.0f, 0, 2);
        public TextureParameter FogNoiseTexture = new(default);
        public ClampedFloatParameter FogNoiseOpacity = new(1.0f, 0, 1);
        public ClampedFloatParameter FogNoiseModulation = new(1.0f, 0, 1);
        public ClampedFloatParameter FogNoiseOffset = new(0, 0, 1);
        public ClampedFloatParameter FogNoiseSpeed = new(0.20f, 0, 1);
        public ClampedFloatParameter FogNoiseScale = new(0.25f, 0, 1);
        public TextureParameter FogRampTexture = new(default);
        public ClampedFloatParameter FogRampOpacity = new(0f, 0, 1);
        public ClampedFloatParameter FogRampScale = new(0.5f, 0, 1);
    }
}
