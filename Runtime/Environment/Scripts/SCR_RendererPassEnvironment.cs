using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererPassEnvironment : ScriptableRenderPass
    {
        public const int MAX_ENVIRONMENT_LIGHTS = 16;
        private const string FOG_SAMPLE_PASS_NAME = "_FogSamplePass";
        private const string FOG_SAMPLE_TEXTURE_NAME = "_FogSampleTexture";
        private const string FOG_BLUR_H_PASS_NAME = "_FogBlurHorizontalPass";
        private const string FOG_BLUR_H_TEXTURE_NAME = "_FogBlurHorizontalTexture";
        private const string FOG_BLUR_V_PASS_NAME = "_FogBlurVerticalPass";
        private const string FOG_BLUR_V_TEXTURE_NAME = "_FogBlurVerticalTexture";
        private const string FOG_COMPOSITE_PASS_NAME = "_FogCompositePass";
        private const string FOG_COMPOSITE_TEXTURE_NAME = "_FogCompositeTexture";

        private static readonly int environmentTimeID = Shader.PropertyToID("_ENVIRONMENT_TIME");
        private static readonly int environmentLightCountID = Shader.PropertyToID("_ENVIRONMENT_LIGHT_COUNT");
        private static readonly int environmentLightPositionID = Shader.PropertyToID("_ENVIRONMENT_LIGHT_POSITION");
        private static readonly int environmenLightColorID = Shader.PropertyToID("_ENVIRONMENT_LIGHT_COLOR");
        private static readonly int fogSampleTexID = Shader.PropertyToID(FOG_SAMPLE_TEXTURE_NAME);
        private static readonly int fogColorID = Shader.PropertyToID("_FOG_COLOR");
        private static readonly int fogDensityID = Shader.PropertyToID("_FOG_DENSITY");
        private static readonly int fogOpacityID = Shader.PropertyToID("_FOG_OPACITY");
        private static readonly int fogDistanceStartID = Shader.PropertyToID("_FOG_DISTANCE_START");
        private static readonly int fogDistanceFalloffID = Shader.PropertyToID("_FOG_DISTANCE_FALLOFF");
        private static readonly int fogHeightStartID = Shader.PropertyToID("_FOG_HEIGHT_START");
        private static readonly int fogHeightFalloffID = Shader.PropertyToID("_FOG_HEIGHT_FALLOFF");
        private static readonly int fogSunInfluenceID = Shader.PropertyToID("_FOG_SUN_INFLUENCE");
        private static readonly int fogSunScatteringID = Shader.PropertyToID("_FOG_SUN_SCATTERING");
        private static readonly int fogRampTexID = Shader.PropertyToID("_FOG_RAMP_TEX");
        private static readonly int fogRampOpacityID = Shader.PropertyToID("_FOG_RAMP_OPACITY");
        private static readonly int fogRampScaleID = Shader.PropertyToID("_FOG_RAMP_SCALE");
        private static readonly int fogNoiseTexID = Shader.PropertyToID("_FOG_NOISE_TEX");
        private static readonly int fogNoiseOpacityID = Shader.PropertyToID("_FOG_NOISE_OPACITY");
        private static readonly int fogNoiseFadeID = Shader.PropertyToID("_FOG_NOISE_FADE");
        private static readonly int fogNoiseScaleID = Shader.PropertyToID("_FOG_NOISE_SCALE");
        private static readonly int fogBlurStrengthID = Shader.PropertyToID("_FOG_BLUR_STRENGTH");
        private static readonly int fogBlurDirectionID = Shader.PropertyToID("_FOG_BLUR_DIRECTION");
        private static readonly int fogBlurRadiusID = Shader.PropertyToID("_FOG_BLUR_RADIUS");
        private static readonly int skyZenithColorID = Shader.PropertyToID("_ZENITH_COLOR");
        private static readonly int skyHorizonColorID = Shader.PropertyToID("_HORIZON_COLOR");
        private static readonly int skyHorizonThicknessID = Shader.PropertyToID("_HORIZON_THICKNESS");
        private static readonly int skyHorizonSoftnessID = Shader.PropertyToID("_HORIZON_SOFTNESS");
        private static readonly int sunColorID = Shader.PropertyToID("_SUN_COLOR");
        private static readonly int sunDirectionID = Shader.PropertyToID("_SUN_DIRECTION");
        private static readonly int sunSizeID = Shader.PropertyToID("_SUN_SIZE");
        private static readonly int sunGlowID = Shader.PropertyToID("_SUN_GLOW");
        private static readonly int sunPowerID = Shader.PropertyToID("_SUN_POWER");
        private static readonly int moonColorID = Shader.PropertyToID("_MOON_COLOR");
        private static readonly int moonDirectionID = Shader.PropertyToID("_MOON_DIRECTION");
        private static readonly int moonSizeID = Shader.PropertyToID("_MOON_SIZE");
        private static readonly int moonGlowID = Shader.PropertyToID("_MOON_GLOW");
        private static readonly int moonPowerID = Shader.PropertyToID("_MOON_POWER");
        private static readonly int cloudBaseTexID = Shader.PropertyToID("_CLOUD_BASE_TEX");
        private static readonly int cloudTargetTexID = Shader.PropertyToID("_CLOUD_TARGET_TEX");
        private static readonly int cloudBlendID = Shader.PropertyToID("_CLOUD_BLEND");
        private static readonly int cloudColorID = Shader.PropertyToID("_CLOUD_COLOR");
        private static readonly int cloudCoverageID = Shader.PropertyToID("_CLOUD_COVERAGE");
        private static readonly int cloudOpacityID = Shader.PropertyToID("_CLOUD_OPACITY");
        private static readonly int cloudFadeID = Shader.PropertyToID("_CLOUD_FADE");
        private static readonly int cloudHeightID = Shader.PropertyToID("_CLOUD_HEIGHT");
        private static readonly int cloudCurveID = Shader.PropertyToID("_CLOUD_CURVE");
        private static readonly int cloudScaleID = Shader.PropertyToID("_CLOUD_SCALE");
        private static readonly int cloudTurbulenceID = Shader.PropertyToID("_CLOUD_TURBULENCE");
        private static readonly int cloudTransmittanceID = Shader.PropertyToID("_CLOUD_TRANSMITTANCE");
        private static readonly int cloudDarknessID = Shader.PropertyToID("_CLOUD_DARKNESS");
        private static readonly int cloudRimWidthID = Shader.PropertyToID("_CLOUD_RIM_WIDTH");
        private static readonly int cloudRimStrengthID = Shader.PropertyToID("_CLOUD_RIM_STRENGTH");
        private static readonly int starColorID = Shader.PropertyToID("_STAR_COLOR");
        private static readonly int starAmountID = Shader.PropertyToID("_STAR_AMOUNT");
        private static readonly int starDensityID = Shader.PropertyToID("_STAR_DENSITY");
        private static readonly int starSizeID = Shader.PropertyToID("_STAR_SIZE");
        private static readonly int starBrightnessID = Shader.PropertyToID("_STAR_BRIGHTNESS");
        private static readonly int starAnimationID = Shader.PropertyToID("_STAR_ANIMATION");

        private readonly EnvironmentLight[] selectedLights = new EnvironmentLight[MAX_ENVIRONMENT_LIGHTS];
        private readonly Plane[] frustumPlanes = new Plane[6];
        private readonly Vector4[] lightPositions = new Vector4[MAX_ENVIRONMENT_LIGHTS];
        private readonly Vector4[] lightColors = new Vector4[MAX_ENVIRONMENT_LIGHTS];
        private readonly float[] lightScores = new float[MAX_ENVIRONMENT_LIGHTS];
        private GraphicsFormat compositeColorFormat;
        private Material fogSampleMaterial;
        private Material fogBlurMaterial;
        private Material fogCompositeMaterial;
        private Material skyMaterial;
        private Vector4 time;
        private int lastFrame = -1;

        public RendererPassEnvironment()
        {
            requiresIntermediateTexture = false;
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }
        private class DefaultPassData
        {
            public TextureHandle Source;
            public Material Material;
        }
        private class BlurPassData
        {
            public TextureHandle Source;
            public Material Material;
            public Vector4 Direction;
        }

        public void Setup(Material fogSampleMaterial, Material fogBlurMaterial, Material fogCompositeMaterial, Material skyMaterial, RenderPassEvent renderPassEvent, GraphicsFormat colorFormat)
        {
            this.fogSampleMaterial = fogSampleMaterial;
            this.fogBlurMaterial = fogBlurMaterial;
            this.fogCompositeMaterial = fogCompositeMaterial;
            this.skyMaterial = skyMaterial;
            this.renderPassEvent = renderPassEvent;
            this.compositeColorFormat = colorFormat;

            RenderSettings.skybox = this.skyMaterial;
            RenderSettings.fog = false;
        }

        private void ApplyTime(Environment environment)
        {
            if (Time.frameCount != lastFrame)
            {
                lastFrame = Time.frameCount;

                float delta = Time.deltaTime;

                time.x = (time.x + delta * environment.FogNoiseSpeed.value) % 100f;
                time.y = (time.y + delta * environment.CloudSpeed.value) % 100f;
                time.z = (time.z + delta * environment.StarAnimationSpeed.value) % 100f;
            }

            Shader.SetGlobalVector(environmentTimeID, time);
        }
        private void ApplyLight(Environment environment, UniversalCameraData cameraData)
        {
            int lights = GetLights(cameraData.camera, cameraData.worldSpaceCameraPos, Mathf.Clamp((int)environment.EnvironmentLights.value, 0, MAX_ENVIRONMENT_LIGHTS));

            for (int i = 0; i < lights; i++)
            {
                EnvironmentLight light = selectedLights[i];
                Vector3 position = light.Position;
                Color color = light.Color.linear * light.Intensity;

                lightPositions[i] = new Vector4(position.x, position.y, position.z, light.Range);
                lightColors[i] = new Vector4(color.r, color.g, color.b, 0f);
            }

            Shader.SetGlobalFloat(environmentLightCountID, lights);

            if (lights > 0)
            {
                Shader.SetGlobalVectorArray(environmentLightPositionID, lightPositions);
                Shader.SetGlobalVectorArray(environmenLightColorID, lightColors);
            }
        }
        private void ApplyAmbient(Environment environment)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = environment.AmbientSkyColor.value;
            RenderSettings.ambientEquatorColor = environment.AmbientEquatorColor.value;
            RenderSettings.ambientGroundColor = environment.AmbientGroundColor.value;
        }
        private void ApplySky(Environment environment)
        {
            Shader.SetGlobalColor(skyZenithColorID, environment.SkyZenithColor.value);
            Shader.SetGlobalColor(skyHorizonColorID, environment.SkyHorizonColor.value);
            Shader.SetGlobalFloat(skyHorizonThicknessID, environment.SkyHorizonThickness.value);
            Shader.SetGlobalFloat(skyHorizonSoftnessID, environment.SkyHorizonSmoothness.value);

            Shader.SetGlobalColor(sunColorID, environment.SunColor.value);
            Shader.SetGlobalVector(sunDirectionID, -environment.SunDirection.value);
            Shader.SetGlobalFloat(sunSizeID, environment.SunSize.value);
            Shader.SetGlobalFloat(sunGlowID, environment.SunGlow.value);
            Shader.SetGlobalFloat(sunPowerID, environment.SunPower.value);

            Shader.SetGlobalColor(moonColorID, environment.MoonColor.value);
            Shader.SetGlobalVector(moonDirectionID, -environment.MoonDirection.value);
            Shader.SetGlobalFloat(moonSizeID, environment.MoonSize.value);
            Shader.SetGlobalFloat(moonGlowID, environment.MoonGlow.value);
            Shader.SetGlobalFloat(moonPowerID, environment.MoonPower.value);

            bool hasCloud = environment.CloudBaseTexture.value != null || environment.CloudTargetTexture.value != null;

            Shader.SetGlobalTexture(cloudBaseTexID, environment.CloudBaseTexture.value);
            Shader.SetGlobalTexture(cloudTargetTexID, environment.CloudTargetTexture.value != null ? environment.CloudTargetTexture.value : environment.CloudBaseTexture.value);
            Shader.SetGlobalFloat(cloudBlendID, environment.CloudBlend.value);
            Shader.SetGlobalColor(cloudColorID, environment.CloudColor.value);
            Shader.SetGlobalFloat(cloudOpacityID, hasCloud ? environment.CloudOpacity.value : 0);
            Shader.SetGlobalFloat(cloudCoverageID, environment.CloudCoverage.value);
            Shader.SetGlobalFloat(cloudFadeID, environment.CloudFade.value);
            Shader.SetGlobalFloat(cloudHeightID, environment.CloudHeight.value);
            Shader.SetGlobalFloat(cloudCurveID, environment.CloudCurve.value);
            Shader.SetGlobalFloat(cloudScaleID, environment.CloudScale.value);
            Shader.SetGlobalFloat(cloudTurbulenceID, environment.CloudTurbulance.value);
            Shader.SetGlobalFloat(cloudTransmittanceID, environment.CloudTransmittance.value);
            Shader.SetGlobalFloat(cloudDarknessID, environment.CloudDarkness.value);
            Shader.SetGlobalFloat(cloudRimWidthID, environment.CloudRimWidth.value);
            Shader.SetGlobalFloat(cloudRimStrengthID, environment.CloudRimStrength.value);

            Shader.SetGlobalColor(starColorID, environment.StarColor.value);
            Shader.SetGlobalFloat(starAmountID, environment.StarAmount.value);
            Shader.SetGlobalFloat(starDensityID, environment.StarDensity.value);
            Shader.SetGlobalFloat(starSizeID, environment.StarSize.value);
            Shader.SetGlobalFloat(starBrightnessID, environment.StarBrightness.value);
            Shader.SetGlobalFloat(starAnimationID, environment.StarAnimationStrength.value);
        }
        private void ApplyFog(Environment environment)
        {
            Shader.SetGlobalColor(fogColorID, environment.FogColor.value);
            Shader.SetGlobalFloat(fogDensityID, environment.FogDensity.value);
            Shader.SetGlobalFloat(fogOpacityID, environment.FogDensity.value > 0 ? 1f : 0f);
            Shader.SetGlobalFloat(fogDistanceStartID, environment.FogDistanceStart.value);
            Shader.SetGlobalFloat(fogDistanceFalloffID, environment.FogDistanceFalloff.value);
            Shader.SetGlobalFloat(fogHeightStartID, environment.FogHeightStart.value);
            Shader.SetGlobalFloat(fogHeightFalloffID, environment.FogHeightFalloff.value);
            Shader.SetGlobalFloat(fogSunInfluenceID, environment.FogSunInfluence.value);
            Shader.SetGlobalFloat(fogSunScatteringID, environment.FogSunScattering.value);
            Shader.SetGlobalTexture(fogRampTexID, environment.FogRampTexture.value);
            Shader.SetGlobalFloat(fogRampOpacityID, environment.FogRampOpacity.value);
            Shader.SetGlobalFloat(fogRampScaleID, environment.FogRampScale.value);
            Shader.SetGlobalTexture(fogNoiseTexID, environment.FogNoiseTexture.value);
            Shader.SetGlobalFloat(fogNoiseOpacityID, environment.FogNoiseOpacity.value);
            Shader.SetGlobalFloat(fogNoiseFadeID, environment.FogNoiseFade.value);
            Shader.SetGlobalFloat(fogNoiseScaleID, environment.FogNoiseScale.value);
            fogBlurMaterial.SetFloat(fogBlurStrengthID, environment.FogBlurStrength.value);
            fogBlurMaterial.SetFloat(fogBlurRadiusID, environment.FogBlurRadius.value);
        }

        private int GetLights(Camera camera, Vector3 cameraPosition, int max)
        {
            IReadOnlyList<EnvironmentLight> lights = EnvironmentLightDatabase.Lights;

            if (max <= 0 || lights.Count == 0)
            {
                return 0;
            }

            GeometryUtility.CalculateFrustumPlanes(camera.projectionMatrix * camera.worldToCameraMatrix, frustumPlanes);

            int count = 0;

            for (int i = 0; i < lights.Count; i++)
            {
                EnvironmentLight light = lights[i];

                if (light.Intensity <= 0f)
                {
                    continue;
                }

                Vector3 position = light.Position;
                float range = light.Range;

                if (!IsLightVisible(position, range))
                {
                    continue;
                }

                float distanceSq = (position - cameraPosition).sqrMagnitude;
                float score = light.Intensity * range * range / (distanceSq + 1f);

                int index = count < max ? count : max - 1;

                if (count >= max && score <= lightScores[index])
                {
                    continue;
                }

                if (count < max)
                {
                    count++;
                }

                while (index > 0 && lightScores[index - 1] < score)
                {
                    lightScores[index] = lightScores[index - 1];
                    selectedLights[index] = selectedLights[index - 1];
                    index--;
                }

                lightScores[index] = score;
                selectedLights[index] = light;
            }

            return count;
        }
        private bool IsLightVisible(Vector3 center, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustumPlanes[i].GetDistanceToPoint(center) < -radius)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ExecuteDefaultPass(DefaultPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
        private static void ExecuteBlurPass(BlurPassData data, RasterGraphContext context)
        {
            data.Material.SetVector(fogBlurDirectionID, data.Direction);
            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

#if UNITY_EDITOR
            if (cameraData.cameraType == CameraType.Preview)
            {
                return;
            }

            if (cameraData.cameraType == CameraType.Reflection)
            {
                return;
            }

            if (cameraData.cameraType == CameraType.SceneView)
            {
                UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;

                if (sceneView == null)
                {
                    return;
                }

                bool showFog = !sceneView.sceneViewState.showFog;

                if (showFog)
                {
                    return;
                }
            }

            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }
#endif
            Environment environment = VolumeManager.instance.stack.GetComponent<Environment>();

            if (!environment.Enabled.value)
            {
                return;
            }

            ApplyTime(environment);
            ApplyAmbient(environment);
            ApplySky(environment);
            ApplyLight(environment, cameraData);

            if (environment.FogDensity.value <= 0.0001f)
            {
                return;
            }

            ApplyFog(environment);

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            TextureHandle depth = resourceData.cameraDepthTexture;

            if (!source.IsValid() || !depth.IsValid())
            {
                return;
            }

            int divisor = 1 << (int)environment.FogResolution.value;
            bool blur = environment.FogBlurStrength.value > 0.01f;

            TextureDesc fogDescription = renderGraph.GetTextureDesc(source);
            fogDescription.name = FOG_SAMPLE_TEXTURE_NAME;
            fogDescription.width = Mathf.Max(1, fogDescription.width / divisor);
            fogDescription.height = Mathf.Max(1, fogDescription.height / divisor);
            fogDescription.depthBufferBits = DepthBits.None;
            fogDescription.msaaSamples = MSAASamples.None;
            fogDescription.filterMode = FilterMode.Bilinear;
            fogDescription.clearBuffer = false;
            fogDescription.colorFormat = compositeColorFormat;
            fogDescription.autoGenerateMips = false;
            fogDescription.useMipMap = false;
            TextureHandle fogTexture = renderGraph.CreateTexture(fogDescription);

            using (var builder = renderGraph.AddRasterRenderPass<DefaultPassData>(FOG_SAMPLE_PASS_NAME, out var passData))
            {
                passData.Source = source;
                passData.Material = fogSampleMaterial;
                builder.UseTexture(source, AccessFlags.Read);
                builder.UseTexture(depth, AccessFlags.Read);
                builder.SetRenderAttachment(fogTexture, 0);

                if (!blur)
                {
                    builder.SetGlobalTextureAfterPass(fogTexture, fogSampleTexID);
                }

                builder.SetRenderFunc((DefaultPassData data, RasterGraphContext context) => ExecuteDefaultPass(data, context));
            }

            TextureHandle fogResult = fogTexture;

            if (blur)
            {
                TextureDesc blurHorizontalDescription = fogDescription;
                blurHorizontalDescription.name = FOG_BLUR_H_TEXTURE_NAME;
                TextureHandle blurHorizontalTexture = renderGraph.CreateTexture(blurHorizontalDescription);

                using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>(FOG_BLUR_H_PASS_NAME, out var passData))
                {
                    passData.Source = fogTexture;
                    passData.Material = fogBlurMaterial;
                    passData.Direction = new Vector4(1, 0, 0, 0);
                    builder.UseTexture(fogTexture, AccessFlags.Read);
                    builder.UseTexture(depth, AccessFlags.Read);
                    builder.SetRenderAttachment(blurHorizontalTexture, 0);
                    builder.SetRenderFunc((BlurPassData data, RasterGraphContext context) => ExecuteBlurPass(data, context));
                }

                TextureDesc blurVerticalDescription = fogDescription;
                blurVerticalDescription.name = FOG_BLUR_V_TEXTURE_NAME;
                TextureHandle blurVerticalTexture = renderGraph.CreateTexture(blurVerticalDescription);

                using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>(FOG_BLUR_V_PASS_NAME, out var passData))
                {
                    passData.Source = blurHorizontalTexture;
                    passData.Material = fogBlurMaterial;
                    passData.Direction = new Vector4(0, 1, 0, 0);
                    builder.UseTexture(blurHorizontalTexture, AccessFlags.Read);
                    builder.UseTexture(depth, AccessFlags.Read);
                    builder.SetRenderAttachment(blurVerticalTexture, 0);
                    builder.SetGlobalTextureAfterPass(blurVerticalTexture, fogSampleTexID);
                    builder.SetRenderFunc((BlurPassData data, RasterGraphContext context) => ExecuteBlurPass(data, context));
                }

                fogResult = blurVerticalTexture;
            }

            TextureDesc outputDescription = renderGraph.GetTextureDesc(source);
            outputDescription.name = FOG_COMPOSITE_TEXTURE_NAME;
            outputDescription.depthBufferBits = DepthBits.None;
            outputDescription.msaaSamples = MSAASamples.None;
            TextureHandle outputTexture = renderGraph.CreateTexture(outputDescription);

            using (var builder = renderGraph.AddRasterRenderPass<DefaultPassData>(FOG_COMPOSITE_PASS_NAME, out var passData))
            {
                passData.Source = source;
                passData.Material = fogCompositeMaterial;
                builder.UseTexture(source, AccessFlags.Read);
                builder.UseTexture(fogResult, AccessFlags.Read);
                builder.SetRenderAttachment(outputTexture, 0);
                builder.SetRenderFunc((DefaultPassData data, RasterGraphContext context) => ExecuteDefaultPass(data, context));
            }

            resourceData.cameraColor = outputTexture;
        }
    }
}
