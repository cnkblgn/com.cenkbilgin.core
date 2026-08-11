#ifndef HLSLQUANTIZE_INCLUDED
#define HLSLQUANTIZE_INCLUDED

void Get_float(in float3 _color, in float _step, out float3 color)
{
    color = floor(_color * _step) / _step;
}
void Get_half(in half3 _color, in half _step, out half3 color)
{   
    Get_float(_color, _step, color);
}

#endif