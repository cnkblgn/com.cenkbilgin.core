#ifndef HLSLSNOW_INCLUDED
#define HLSLSNOW_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"
 
void Get_float(float3 _worldPosition, float3 _worldNormal, float _heightMask, float _heightContrast, float3 _snowColor, Texture2D _snowMap, SamplerState _snowSamp, float2 _snowOffset, float _snowScale, float _snowOpacity, float3 _baseColor, out float3 color)
{    
    float2 uv01 = _worldPosition.xz * _snowScale + _snowOffset;
    float2 uv02 = uv01 + Hash22(floor(_worldPosition.xz * 0.133));
    
    float3 color01 = _snowMap.Sample(_snowSamp, uv01).rgb;
    float3 color02 = _snowMap.Sample(_snowSamp, uv02).rgb;

    float upMask = saturate(dot(normalize(_worldNormal), float3(0, 1, 0)));
    float heightMask = pow(saturate(_heightMask), _heightContrast);   
    float mask = upMask * heightMask * _snowOpacity;

    float3 snowColor = color01 * color02 * _snowColor;
    
    color = lerp(_baseColor, snowColor, mask);
}

void Get_half(half3 _worldPosition, half3 _worldNormal, half _heightMask, half _heightContrast, half3 _snowColor, Texture2D _snowMap, SamplerState _snowSamp, half2 _snowOffset, half _snowScale, half _snowOpacity, half3 _baseColor, out half3 color)
{
    Get_float(_worldPosition, _worldNormal, _heightMask, _heightContrast, _snowColor, _snowMap, _snowSamp, _snowOffset, _snowScale, _snowOpacity, _baseColor, color);
}

#endif