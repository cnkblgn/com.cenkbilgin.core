#ifndef HLSLVIBRANCE_INCLUDED
#define HLSLVIBRANCE_INCLUDED

#include "HLSL_Helper.hlsl"

void Get_float(in float3 _color, in float3 _balance, out float3 color)
{
    float l = dot(_color, LUMA);

    float maxC = max(_color.r, max(_color.g, _color.b));
    float minC = min(_color.r, min(_color.g, _color.b));
    float delta = maxC - minC;

    float3 t = float3(1, 1, 1) + (_balance * (1.0 - delta));

    color = lerp(l, _color.rgb, t);
}
void Get_half(in half3 _color, in half3 _balance, out half3 color)
{
    Get_float(_color, _balance, color);
}
#endif