using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererFeatureEnvironment : ScriptableRendererFeature
    {
        private const string FOG_SHADER_PATH = "Hidden/FX_FullscreenFog";
        private const string SKY_SHADER_PATH = "Hidden/FX_UnlitSky";

        [Info("Use environment system to update global constants!\nRequires depth texture and injection point 'AfterOpaques'\nAlso this feature disables unity fog!")]
        [ReadOnly] public bool hasInitialized;

        private Material fogMaterial;
        private Material skyMaterial;
        private RendererPassEnvironment pass;

        public override void Create()
        {
            Shader fogShader = Shader.Find(FOG_SHADER_PATH);
            Shader skyShader = Shader.Find(SKY_SHADER_PATH);

            if (fogShader != null)
            {
                fogMaterial = CoreUtils.CreateEngineMaterial(fogShader);
                hasInitialized = true;
            }
            else
            {
                Debug.LogError($"Fog shader [{FOG_SHADER_PATH}] not found?");
                hasInitialized = false;
                return;
            }

            if (skyShader != null)
            {
                skyMaterial = CoreUtils.CreateEngineMaterial(skyShader);
                hasInitialized = true;
            }
            else
            {
                Debug.LogError($"Sky shader [{SKY_SHADER_PATH}] not found?");
                hasInitialized = false;
                return;
            }

            pass = new();
        }
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(fogMaterial);
            CoreUtils.Destroy(skyMaterial);

            fogMaterial = null;
            skyMaterial = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (fogMaterial == null)
            {
                return;
            }

            if (skyMaterial == null)
            {
                return;
            }

            pass.Setup(fogMaterial, skyMaterial);
            renderer.EnqueuePass(pass);
        }
    }
}
