#ifndef HLSLRSUVCOLOR_INCLUDED
#define HLSLRSUVCOLOR_INCLUDED

#include "HLSL_RSUVHelper.hlsl"

void Get_float(out float4 Color)
{
    uint data = getData();  
    float useRSV = hasData(data) ? 1.0 : 0.0;
    
    Color = lerp(1.0.xxxx, decodeUintToFloat4(data & 0xFFFFFFFE), useRSV);
} 
void Get_half(out half4 Color) 
{
    Get_float(Color);
}
#endif