using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererFeatureEnvironment : ScriptableRendererFeature
    {
        private const string FOG_SAMPLE_SHADER_PATH = "Hidden/FX_FullscreenFogSample";
        private const string FOG_COMPOSITE_SHADER_PATH = "Hidden/FX_FullscreenFogComposite";
        private const string SKY_SHADER_PATH = "Hidden/FX_UnlitSky";

        [Info("Use environment system to update global constants!\nRequires depth texture and injection point 'AfterOpaques'\nAlso this feature disables unity fog!")]
        [ReadOnly] public bool hasInitialized;

        private Material fogSampleMaterial;
        private Material fogCompositeMaterial;
        private Material skyMaterial;
        private RendererPassEnvironment pass;

        public override void Create()
        {
            Shader fogSampleShader = Shader.Find(FOG_SAMPLE_SHADER_PATH);
            Shader fogCompositeShader = Shader.Find(FOG_COMPOSITE_SHADER_PATH);
            Shader skyShader = Shader.Find(SKY_SHADER_PATH);

            if (fogSampleShader == null)
            {
                Debug.LogError($"Fog Sample shader [{FOG_SAMPLE_SHADER_PATH}] not found.");
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
            fogCompositeMaterial = CoreUtils.CreateEngineMaterial(fogCompositeShader);
            skyMaterial = CoreUtils.CreateEngineMaterial(skyShader);

            pass = new RendererPassEnvironment();
            hasInitialized = true;
        }
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(fogSampleMaterial);
            CoreUtils.Destroy(fogCompositeMaterial);
            CoreUtils.Destroy(skyMaterial);

            fogSampleMaterial = null;
            fogCompositeMaterial = null;
            skyMaterial = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (fogSampleMaterial == null)
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

            pass.Setup(fogSampleMaterial, fogCompositeMaterial, skyMaterial);
            renderer.EnqueuePass(pass);
        }
    }
}
