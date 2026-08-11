#ifndef HLSLRSUVCOLOR_INCLUDED
#define HLSLRSUVCOLOR_INCLUDED

#include "HLSL_RSUVHelper.hlsl"

uint4 ColorToBytes(float4 c)
{
    c = saturate(c);
    
    return (uint4) (c * 255.0 + 0.5);
}
uint ColorRToByte(float4 c)
{
    return (uint) (saturate(c.r) * 255.0 + 0.5);
}
uint ColorGToByte(float4 c)
{
    return (uint) (saturate(c.g) * 255.0 + 0.5);
}
uint ColorBToByte(float4 c)
{
    return (uint) (saturate(c.b) * 255.0 + 0.5);
}
uint ColorAToByte(float4 c)
{
    return (uint) (saturate(c.a) * 255.0 + 0.5);
}

void Get_float(out float4 Color)
{
    Color = decodeUintToFloat4(getData());
}
void Get_half(out half4 Color)
{
    Get_float(Color);
}
void GetWithFlag_float(out float4 Color)
{
    float4 data = decodeUintToFloat4(getData());
    
    float enabled = data.a > 0.5 ? 1.0 : 0.0;

    Color = lerp(1.0.xxxx, float4(data.rgb, 1.0), enabled);
}
void GetWithFlag_half(out half4 Color) 
{
    GetWithFlag_float(Color);
}
#endif