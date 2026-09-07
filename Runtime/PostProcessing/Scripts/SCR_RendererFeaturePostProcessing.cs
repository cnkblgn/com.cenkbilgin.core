using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.PostProcessing
{
    internal sealed class RendererFeaturePostProcessing : ScriptableRendererFeature
    {
        private const string SHADER_PATH = "Hidden/FX_PostProcessing";

        [ReadOnly] public bool hasInitialized;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        private Material material;
        private RendererPassPostProcessing pass;

        public override void Create()
        {
            Shader shader = Shader.Find(SHADER_PATH);

            if (shader != null)
            {
                material = CoreUtils.CreateEngineMaterial(shader);
                hasInitialized = true;
            }
            else
            {
                Debug.LogError($"Fog shader [{SHADER_PATH}] not found?");
                hasInitialized = false;
                return;
            }

            pass = new();
        }
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            pass.Setup(material, renderPassEvent);
            renderer.EnqueuePass(pass);
        }
    }
}
