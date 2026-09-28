#ifndef HLSLDITHER_INCLUDED
#define HLSLDITHER_INCLUDED

#include "HLSL_Helper.hlsl"

void Get_float(float3 _color, float2 _uv, float2 _texelSize, float _strength, float _size, float _spread, int _steps, out float3 color)
{
    float luma = dot(_color, LUMA);

    uint2 p = (uint2) floor(_uv / (_texelSize * _size)) & 3u;

    uint a = p.x ^ p.y;
    uint idx = ((a & 1u) << 3) | ((p.y & 1u) << 2) | (a & 2u) | ((p.y >> 1) & 1u);
    
    float dither = idx * (1.0 / 16.0);

    float offset = saturate(luma + (dither - 0.5) * _spread);
    float quantize = floor(offset * _steps + 0.5) / _steps;

    // lerp(1, r, s) = 1 + s * (r - 1)
    color = _color * (1.0 + _strength * (quantize * rcp(max(luma, 0.0001)) - 1.0));
}
void Get_half(half3 _color, half2 _uv, half2 _texelSize, half _strength, half _size, half _spread, int _steps, out half3 color)
{
    Get_float(_color, _uv, _texelSize, _strength, _size, _spread, _steps, color);
}
#endif