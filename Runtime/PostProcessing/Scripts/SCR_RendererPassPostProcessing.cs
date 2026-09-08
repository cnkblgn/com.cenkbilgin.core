using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.PostProcessing
{
    internal sealed class RendererPassPostProcessing : ScriptableRenderPass
    {
        private const string PASS_NAME = "PostProcessingPass";
        private const string TEXTURE_NAME = "CameraColor" + "_" + PASS_NAME;

        private static readonly int ClarifyIntensityID = Shader.PropertyToID("_ClarifyIntensity");
        private static readonly int VibranceIntensityID = Shader.PropertyToID("_VibranceIntensity");
        private static readonly int VibranceBalanceID = Shader.PropertyToID("_VibranceBalance");
        private static readonly int SharpenStrengthID = Shader.PropertyToID("_SharpenStrength");
        private static readonly int SharpenOffsetID = Shader.PropertyToID("_SharpenOffset");
        private static readonly int SharpenClampID = Shader.PropertyToID("_SharpenClamp");
        private static readonly int QuantizeStepsID = Shader.PropertyToID("_QuantizeSteps");
        private static readonly int DitherStrengthID = Shader.PropertyToID("_DitherStrength");
        private static readonly int DitherSizeID = Shader.PropertyToID("_DitherSize");
        private static LocalKeyword ClarifyKeyword;
        private static LocalKeyword VibranceKeyword;
        private static LocalKeyword SharpenKeyword;
        private static LocalKeyword QuantizeKeyword;
        private static LocalKeyword DitherKeyword;
        private Material material;

        public RendererPassPostProcessing()
        {
            requiresIntermediateTexture = false;

            ConfigureInput(ScriptableRenderPassInput.Color);
        }
        private class PassData 
        { 
            public TextureHandle Source; 
            public Material Material;

            public bool ClarityEnabled;
            public float ClarityIntensity;

            public bool VibranceEnabled;
            public float VibranceIntensity;
            public Vector3 VibranceBalance;

            public bool SharpenEnabled;
            public float SharpenStrength;
            public float SharpenOffset;
            public float SharpenClamp;

            public bool QuantizeEnabled;
            public int QuantizeSteps;

            public bool DitherEnabled;
            public float DitherStrength;
            public float DitherSize;
        }

        public void Setup(Material material, RenderPassEvent renderPassEvent)
        {
            this.material = material;
            this.renderPassEvent = renderPassEvent;

            ClarifyKeyword = new(material.shader, "_CLARIFY");
            VibranceKeyword = new(material.shader, "_VIBRANCE");
            SharpenKeyword = new(material.shader, "_SHARPEN");
            QuantizeKeyword = new(material.shader, "_QUANTIZE");
            DitherKeyword = new(material.shader, "_DITHER");
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            data.Material.SetKeyword(ClarifyKeyword, data.ClarityEnabled);
            data.Material.SetFloat(ClarifyIntensityID, data.ClarityIntensity);

            data.Material.SetKeyword(VibranceKeyword, data.VibranceEnabled);
            data.Material.SetFloat(VibranceIntensityID, data.VibranceIntensity);
            data.Material.SetVector(VibranceBalanceID, data.VibranceBalance);

            data.Material.SetKeyword(SharpenKeyword, data.SharpenEnabled);
            data.Material.SetFloat(SharpenStrengthID, data.SharpenStrength);
            data.Material.SetFloat(SharpenOffsetID, data.SharpenOffset);
            data.Material.SetFloat(SharpenClampID, data.SharpenClamp);

            data.Material.SetKeyword(QuantizeKeyword, data.QuantizeEnabled);
            data.Material.SetFloat(QuantizeStepsID, data.QuantizeSteps);

            data.Material.SetKeyword(DitherKeyword, data.DitherEnabled);
            data.Material.SetFloat(DitherStrengthID, data.DitherStrength);
            data.Material.SetFloat(DitherSizeID, data.DitherSize);

            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            VolumeStack stack = VolumeManager.instance.stack;
            Clarity clarity = stack.GetComponent<Clarity>();
            Vibrance vibrance = stack.GetComponent<Vibrance>();
            Sharpen sharpen = stack.GetComponent<Sharpen>();
            Quantize quantize = stack.GetComponent<Quantize>();
            Dither dither = stack.GetComponent<Dither>();

            TextureHandle source = resourceData.activeColorTexture;
            TextureDesc description = renderGraph.GetTextureDesc(source);
            description.name = TEXTURE_NAME;
            description.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(description);

            using var builder = renderGraph.AddRasterRenderPass(PASS_NAME, out PassData passData);
            {
                passData.Source = source;
                passData.Material = material;

                passData.ClarityEnabled = clarity.active && clarity.Enabled.value && clarity.Intensity.value != 0;
                passData.ClarityIntensity = clarity.Intensity.value;

                passData.VibranceEnabled = vibrance.active && vibrance.Enabled.value && vibrance.Intensity.value != 0;
                passData.VibranceIntensity = vibrance.Intensity.value;
                passData.VibranceBalance = vibrance.Balance.value;

                passData.SharpenEnabled = sharpen.active && sharpen.Enabled.value && sharpen.Strength.value != 0;
                passData.SharpenStrength = sharpen.Strength.value;
                passData.SharpenOffset = sharpen.Offset.value;
                passData.SharpenClamp = sharpen.Clamp.value;

                passData.QuantizeEnabled = quantize.active && quantize.Enabled.value;
                passData.QuantizeSteps = quantize.Steps.value;

                passData.DitherEnabled = dither.active && dither.Enabled.value && dither.Strength.value != 0;
                passData.DitherStrength = dither.Strength.value;
                passData.DitherSize = dither.Size.value;
            }
            
            builder.UseTexture(passData.Source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

            resourceData.cameraColor = destination;
        }
    }
}
