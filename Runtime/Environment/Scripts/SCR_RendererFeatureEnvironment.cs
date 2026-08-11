using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Environment
{
    internal sealed class RendererFeatureEnvironment : ScriptableRendererFeature
    {
        [Info("Use environment system to update global constants!\nRequires depth texture and injection point 'AfterOpaques'\nAlso this feature disables unity fog!")]
        [SerializeField, Required] private Material fogMaterial;
        [SerializeField, Required] private Material skyMaterial;

        private RendererPassEnvironment pass;

        public override void Create() => pass = new();

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
