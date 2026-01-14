#ifndef HLSLRSUVHELPER_INCLUDED
#define HLSLRSUVHELPER_INCLUDED

uint getData()
{
    return unity_RendererUserValue;
}
bool getBit(uint data, uint bitIndex)
{
    return ((data & (1 << bitIndex)) != 0);
}
float decodeBitsToInt(uint data, int bitOffset, int bitCount)
{
    uint mask = (1u << bitCount) - 1u;
    uint value = (data >> bitOffset) & mask;
    return (int) value;
}
float4 decodeUintToFloat4(uint data)
{
    float a = ((data >> 24) & 0xFF) / 255.0;
    float r = ((data >> 16) & 0xFF) / 255.0;
    float g = ((data >> 8) & 0xFF) / 255.0;
    float b = (data & 0xFF) / 255.0;

    return float4(r, g, b, a);
}

#endif