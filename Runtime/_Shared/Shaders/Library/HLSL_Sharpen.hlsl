#ifndef HLSLSHARPEN_INCLUDED
#define HLSLSHARPEN_INCLUDED

#include "HLSL_Helper.hlsl"

void GetLuma_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float _offset, in float _strength, in float _clamp, out float3 color)
{
    float2 tD; _tex2D.GetDimensions(tD.x, tD.y);   
    float2 tS = 1 / tD;
    
    float3 baseColor = _tex2D.Sample(_samp2D, _uv).rgb;
    float3 blurColor;      

    blurColor = _tex2D.Sample(_samp2D, _uv + (tS / 3.0) * _offset).rgb; // North West
    blurColor += _tex2D.Sample(_samp2D, _uv + (-tS / 3.0) * _offset).rgb; // South East
    blurColor *= 0.5f;

    float3 tempColor = baseColor - blurColor;   
    float3 strength = ((LUMA * _strength) * 1.5f) * (0.5 / max(_clamp, 1e-5));
    float sharpness = saturate(dot(tempColor, strength)) * _clamp;

    color = baseColor + sharpness;
}
void GetLuma_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half _offset, in half _strength, in half _clamp, out half3 color)
{
    GetLuma_float(_tex2D, _samp2D, _uv, _offset, _strength, _clamp, color);
}
void GetDefault_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float2 _screenPos, in float _strength, out float3 color)
{
    float2 aUV = _screenPos + OFFSET;
    float2 bUV = _screenPos - OFFSET;

    float3 aCol = _tex2D.Sample(_samp2D, aUV).rgb * _strength;
    float3 bCol = _tex2D.Sample(_samp2D, bUV).rgb * _strength;
    float3 cCol = _tex2D.Sample(_samp2D, _uv).rgb;
 
    color = (cCol - aCol) + bCol;
}
void GetDefault_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half2 _screenPos, in half _strength, out half3 color)
{
    GetDefault_float(_tex2D, _samp2D, _uv, _screenPos, _strength, color);
}
#endif