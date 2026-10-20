#ifndef HLSLRSUVHELPER_INCLUDED
#define HLSLRSUVHELPER_INCLUDED

uint GetData()
{
    return unity_RendererUserValue;
}

bool HasData(uint data)
{
    return (data & 1u) != 0;
}

bool GetBit(uint data, uint bitIndex)
{
    return ((data & (1 << bitIndex)) != 0);
}

float DecodeBitsToInt(uint data, int bitOffset, int bitCount)
{
    uint mask = (1u << bitCount) - 1u;
    uint value = (data >> bitOffset) & mask;
    return (int) value;
}

float4 DecodeUintToFloat4(uint data)
{
    float a = ((data >> 24) & 0xFF) / 255.0;
    float r = ((data >> 16) & 0xFF) / 255.0;
    float g = ((data >> 8) & 0xFF) / 255.0;
    float b = (data & 0xFF) / 255.0;

    return float4(r, g, b, a);
}
 
#endif