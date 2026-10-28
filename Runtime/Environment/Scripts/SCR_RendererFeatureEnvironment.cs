using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererFeatureEnvironment : ScriptableRendererFeature
    {
        private const string FOG_SAMPLE_SHADER_PATH = "Hidden/FX_FullscreenFogSample";
        private const string FOG_BLUR_SHADER_PATH = "Hidden/FX_FullscreenFogBlur";
        private const string FOG_COMPOSITE_SHADER_PATH = "Hidden/FX_FullscreenFogComposite";
        private const string SKY_SHADER_PATH = "Hidden/FX_UnlitSky";

        [Info("Use environment system to update global constants!\nRequires depth texture and injection point 'AfterOpaques'\nAlso this feature disables unity fog!")]
        [ReadOnly] public bool hasInitialized;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        public bool hdr = true;

        private Material fogSampleMaterial;
        private Material fogBlurMaterial;
        private Material fogCompositeMaterial;
        private Material skyMaterial;
        private RendererPassEnvironment pass;

        public override void Create()
        {
            Shader fogSampleShader = Shader.Find(FOG_SAMPLE_SHADER_PATH);
            Shader fogBlurShader = Shader.Find(FOG_BLUR_SHADER_PATH);
            Shader fogCompositeShader = Shader.Find(FOG_COMPOSITE_SHADER_PATH);
            Shader skyShader = Shader.Find(SKY_SHADER_PATH);

            if (fogSampleShader == null)
            {
                Debug.LogError($"Fog Sample shader [{FOG_SAMPLE_SHADER_PATH}] not found.");
                hasInitialized = false;
                return;
            }

            if (fogBlurShader == null)
            {
                Debug.LogError($"Fog Blur shader [{FOG_BLUR_SHADER_PATH}] not found.");
                hasInitialized = false;
                return;
            }

            if (fogCompositeShader == null)
            {
                Debug.LogError($"Fog Composite shader [{FOG_COMPOSITE_SHADER_PATH}] not found.");
                hasInitialized = false;
                return;
            }

            if (skyShader == null)
            {
                Debug.LogError($"Sky shader [{SKY_SHADER_PATH}] not found.");
                hasInitialized = false;
                return;
            }

            fogSampleMaterial = CoreUtils.CreateEngineMaterial(fogSampleShader);
            fogBlurMaterial = CoreUtils.CreateEngineMaterial(fogBlurShader);
            fogCompositeMaterial = CoreUtils.CreateEngineMaterial(fogCompositeShader);
            skyMaterial = CoreUtils.CreateEngineMaterial(skyShader);

            pass = new RendererPassEnvironment();
            hasInitialized = true;
        }
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(fogSampleMaterial);
            CoreUtils.Destroy(fogBlurMaterial);
            CoreUtils.Destroy(fogCompositeMaterial);
            CoreUtils.Destroy(skyMaterial);

            fogSampleMaterial = null;
            fogBlurMaterial = null;
            fogCompositeMaterial = null;
            skyMaterial = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (fogSampleMaterial == null)
            {
                return;
            }

            if (fogBlurMaterial == null)
            {
                return;
            }

            if (fogCompositeMaterial == null)
            {
                return;
            }

            if (skyMaterial == null)
            {
                return;
            }

            pass.Setup(fogSampleMaterial, fogBlurMaterial, fogCompositeMaterial, skyMaterial, renderPassEvent, hdr ? GraphicsFormat.R16G16B16A16_SFloat : GraphicsFormat.R16G16B16A16_UNorm);
            renderer.EnqueuePass(pass);
        }
    }
}
