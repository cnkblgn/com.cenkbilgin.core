#ifndef HLSLRSUVCOLOR_INCLUDED
#define HLSLRSUVCOLOR_INCLUDED

#include "HLSL_RSUVHelper.hlsl"

void Get_float(out float4 Color)
{
    uint data = getData();
    
    float useRSV = getBit(data, 0) ? 1.0 : 0.0;
    
    uint colorData = data & 0xFFFFFFFE;
    
    Color = decodeUintToFloat4(colorData);
    
    Color = lerp(float4(1, 1, 1, 1), Color, useRSV);
}
void Get_half(out half4 Color)
{
    Get_float(Color);
}
#endif