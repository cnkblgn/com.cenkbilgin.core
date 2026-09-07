#ifndef HLSLCLARITY_INCLUDED
#define HLSLCLARITY_INCLUDED

#include "HLSL_Helper.hlsl"

void Get_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float2 _screenPos, in float _strength, out float3 color)
{
    float2 aUV = _screenPos + OFFSET;
    float2 bUV = _screenPos - OFFSET;

    float3 aCol = _tex2D.Sample(_samp2D, aUV).rgb * _strength;
    float3 bCol = _tex2D.Sample(_samp2D, bUV).rgb * _strength;
    float3 cCol = _tex2D.Sample(_samp2D, _uv).rgb;
 
    color = (cCol - aCol) + bCol;
}
void Get_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half2 _screenPos, in half _strength, out half3 color)
{
    Get_float(_tex2D, _samp2D, _uv, _screenPos, _strength, color);
}
#endif