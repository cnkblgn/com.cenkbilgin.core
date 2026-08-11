#ifndef HLSLBLUR_INCLUDED
#define HLSLBLUR_INCLUDED

#include "HLSL_Helper.hlsl"

float4 blurBox(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float _strength)
{
    float4 c = 0;
	
    for (int i = 0; i < 9; i++)
    {
        float2 uv = OFFSETS[i] * _strength * 0.001;
        
        c += _tex2D.Sample(_samp2D, _uv + uv);
    }
	
    return c / 9;
}
float4 blurGaussian(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float _strength, int _radius, float _downsample)
{
    float2 tD;
    _tex2D.GetDimensions(tD.x, tD.y);

    float dF = _downsample;
    float2 dUV = floor(_uv * tD * dF) / (tD * dF);

    float4 c = float4(0, 0, 0, 0);
    float t = 0.0;
    
    float2 pS = 1.0 / (tD * dF);
    float d = 1 / (2.0 * _strength * _strength);

    for (int y = -_radius; y <= _radius; y++)
    {
        float wY = exp(-(y * y) * d);

        for (int x = -_radius; x <= _radius; x++)
        {
            float wX = exp(-(x * x) * d);
            float w = wX * wY;

            c += _tex2D.SampleLevel(_samp2D, dUV + (float2(x, y) * pS), 16) * w;
            t += w;
        }
    }

    if (t > 0.0)
    {
        c /= t;
    }
    
    return c;
}

void Compute_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float _strength, out float4 color)
{
    color = blurBox(_tex2D, _samp2D, _uv, _strength);
}
void Compute_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half _strength, out half4 color)
{
    Compute_float(_tex2D, _samp2D, _uv, _strength, color);
}
void Compute_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float _sigma, in int _radius, in float _downsample, out float4 color)
{
    color = blurGaussian(_tex2D, _samp2D, _uv, _sigma, _radius, _downsample);
}
void Compute_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half _sigma, in int _radius, in half _downsample, out half4 color)
{
    Compute_float(_tex2D, _samp2D, _uv, _sigma, _radius, _downsample, color);
}
#endif