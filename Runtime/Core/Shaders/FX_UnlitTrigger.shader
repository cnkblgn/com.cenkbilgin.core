Shader "Hidden/FX_TriggerZone"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1,1,0,0.18)
        _ColorB ("Color B", Color) = (0,0,0,0.18)
        _StripeScale("Stripe Scale", Float) = 4
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _ColorA;
            half4 _ColorB; 
            float _StripeScale;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS).xyz; 

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float stripe = frac((IN.positionWS.x + IN.positionWS.y + IN.positionWS.z) * _StripeScale);

                return stripe < 0.5 ? _ColorA : _ColorB;
            }
            ENDHLSL
        }
    }
}
