#ifndef HLSLDITHER_INCLUDED
#define HLSLDITHER_INCLUDED

#include "HLSL_Helper.hlsl"

void Get_float(float3 _color, float2 _uv, float2 _texelSize, float strength, float size, out float3 color)
{
    int2 p = int2(_uv / _texelSize.xy * size);
    int x = p.x & 3;
    int y = p.y & 3;

    float d = (Bayer4x4[y][x] - 0.5) * strength;

    color = _color + d;
}
void Get_half(half3 _color, half2 _uv, half2 _texelSize, half strength, half size, out half3 color)
{
    Get_float(_color, _uv, _texelSize, strength, size, color);
}

#endif