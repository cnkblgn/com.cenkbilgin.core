#ifndef HLSLRSUVUV_INCLUDED
#define HLSLRSUVUV_INCLUDED

#include "HLSL_RSUVHelper.hlsl"

void Get_float(out float2 offset, out float2 tiling)
{
    uint data = getData();

    uint uScale = (data >> 1) & 0x3FFu;
    uint uOffX = (data >> 11) & 0x3FFu;
    uint uOffY = (data >> 21) & 0x3FFu;

    float decodedScale = (uScale / 1023.0) * 8.0;
    float2 decodedOffset;
    decodedOffset.x = (uOffX / 1023.0) * 8.0 - 4.0;
    decodedOffset.y = (uOffY / 1023.0) * 8.0 - 4.0;

    float useRSV = hasData(data) ? 1.0 : 0.0;

    tiling = lerp(1.0.xx, float2(decodedScale, decodedScale), useRSV);
    offset = lerp(1.0.xx, decodedOffset, useRSV);
}
void Get_half(out half2 offset, out half2 tiling)
{
    Get_float(offset, tiling);
}

#endif