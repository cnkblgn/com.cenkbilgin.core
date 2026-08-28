using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererPassEnvironment : ScriptableRenderPass
    {
        private const string PASS_NAME = "FogPass";
        private const string TEXTURE_NAME = "CameraColor" + "_" + PASS_NAME;

        private Material fogMaterial;
        private Material skyMaterial;

        public RendererPassEnvironment()
        {
            requiresIntermediateTexture = false;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }
        private class PassData { public TextureHandle Source; public Material Material; }

        public void Setup(Material fogMaterial, Material skyMaterial)
        {
            this.fogMaterial = fogMaterial;
            this.skyMaterial = skyMaterial;

            RenderSettings.fog = false;
            RenderSettings.skybox = this.skyMaterial;
        }

        private static void ExecutePass(PassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
#if UNITY_EDITOR
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.cameraType == CameraType.SceneView)
            {
                SceneView sceneView = SceneView.lastActiveSceneView;

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
            TextureDesc description = renderGraph.GetTextureDesc(source);
            description.name = TEXTURE_NAME;
            description.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(description);

            using var builder = renderGraph.AddRasterRenderPass<PassData>(PASS_NAME, out var passData);
            passData.Source = source;
            passData.Material = fogMaterial;

            builder.UseTexture(passData.Source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

            resourceData.cameraColor = destination;
        }
    }
}
