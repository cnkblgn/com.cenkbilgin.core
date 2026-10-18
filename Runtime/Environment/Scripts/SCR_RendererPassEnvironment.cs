using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererPassEnvironment : ScriptableRenderPass
    {
        private const string FOG_SAMPLE_PASS_NAME = "_FogSamplePass";
        private const string FOG_SAMPLE_TEXTURE_NAME = "_FogSampleTexture";
        private const string FOG_COMPOSITE_PASS_NAME = "_FogCompositePass";
        private const string FOG_COMPOSITE_TEXTURE_NAME = "_FogCompositeTexture";
        private static readonly int FOG_SAMPLE_TEXTURE_ID = Shader.PropertyToID(FOG_SAMPLE_TEXTURE_NAME);

        private Material fogSampleMaterial;
        private Material fogCompositeMaterial;
        private Material skyMaterial;

        public RendererPassEnvironment()
        {
            requiresIntermediateTexture = false;

            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }
        private class PassData 
        { 
            public TextureHandle Source; 
            public Material Material;
        }

        public void Setup(Material fogSampleMaterial, Material fogCompositeMaterial, Material skyMaterial)
        {
            this.fogSampleMaterial = fogSampleMaterial;
            this.fogCompositeMaterial = fogCompositeMaterial;
            this.skyMaterial = skyMaterial;

            RenderSettings.fog = false;
            RenderSettings.skybox = this.skyMaterial;
        }

        private static void ExecutePass(PassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
#if UNITY_EDITOR
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

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

            if (EnvironmentSystem.cachedSettings.Fog.Density <= 0.01f)
            {
                return;
            }

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

            int divisor = 1;

            if (EnvironmentSystem.TryGetSettings(out EnvironmentSettings settings))
            {
                divisor = 1 << (int)settings.FogResolution;
            }

            if (settings.FogMode == FogMode.OFF)
            {
                return;
            }

            TextureDesc fogDescription = renderGraph.GetTextureDesc(source);
            fogDescription.name = FOG_SAMPLE_TEXTURE_NAME;
            fogDescription.width = Mathf.Max(1, fogDescription.width / divisor);
            fogDescription.height = Mathf.Max(1, fogDescription.height / divisor);
            fogDescription.depthBufferBits = DepthBits.None;
            fogDescription.msaaSamples = MSAASamples.None;
            fogDescription.filterMode = FilterMode.Bilinear;
            fogDescription.clearBuffer = false;
            fogDescription.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            TextureHandle fogTexture = renderGraph.CreateTexture(fogDescription);

            // 2. Sis hesabý.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(FOG_SAMPLE_PASS_NAME, out var passData))
            {
                passData.Source = source;
                passData.Material = fogSampleMaterial;   
                builder.UseTexture(source, AccessFlags.Read);
                builder.UseTexture(depth, AccessFlags.Read);
                builder.SetRenderAttachment(fogTexture, 0);
                builder.SetGlobalTextureAfterPass(fogTexture, FOG_SAMPLE_TEXTURE_ID);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }

            // 3. Tam çözünürlüklü nihai kamera texture'ý.
            TextureDesc outputDescription = renderGraph.GetTextureDesc(source);
            outputDescription.name = FOG_COMPOSITE_TEXTURE_NAME;
            outputDescription.depthBufferBits = DepthBits.None;
            outputDescription.msaaSamples = MSAASamples.None;
            TextureHandle outputTexture = renderGraph.CreateTexture(outputDescription);

            // 4. Orijinal sahne + volumetric sis.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(FOG_COMPOSITE_PASS_NAME, out var passData))
            {
                passData.Source = source;
                passData.Material = fogCompositeMaterial;
                builder.UseTexture(source, AccessFlags.Read);
                builder.UseTexture(fogTexture, AccessFlags.Read);
                builder.SetRenderAttachment(outputTexture, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }

            resourceData.cameraColor = outputTexture;
        }
    }
}
