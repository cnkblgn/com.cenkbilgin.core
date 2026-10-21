#ifndef HLSLBLUR_INCLUDED
#define HLSLBLUR_INCLUDED

#include "HLSL_Helper.hlsl"

float4 BlurBox(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float _strength)
{
    uint width, height;  
    _tex2D.GetDimensions(width, height);

    float2 texelSize = 1.0 / float2(width, height);
    float2 texelOffset = texelSize * _strength;
    float4 color = 0.0;

    [unroll]
    for (int i = 0; i < 9; i++)
    {
        color += _tex2D.Sample(_samp2D, _uv + OFFSETS[i] * texelOffset);
    }

    return color * (1.0 / 9.0);
}
float4 BlurBoxDirectional(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float2 _direction, float _strength, int _radius)
{
    uint width, height;
    _tex2D.GetDimensions(width, height);
    
    float2 texelSize = 1.0 / float2(width, height);
    float2 texelOffset = texelSize * _strength * _direction;
    float4 color = 0.0;
    int count = 0;

    for (int i = -_radius; i <= _radius; i++)
    {
        color += _tex2D.Sample(_samp2D, _uv + texelOffset * i);
        count++;
    }

    return color / count;
}
 
float4 BlurGaussian(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float _strength, int _radius, float _scale)
{
    uint width, height, mipCount;
    _tex2D.GetDimensions(0, width, height, mipCount);

    float mipLevel = min(2.0, (float) (mipCount - 1));
    float2 sampleSize = float2(width, height) * _scale;
    float2 sampleUV = floor(_uv * sampleSize) / sampleSize;
    float2 texelOffset = 1.0 / sampleSize;

    float4 totalColor = 0.0;
    float totalWeight = 0.0;

    float gaussianSigma = 1.0 / (2.0 * _strength * _strength);

    for (int y = -_radius; y <= _radius; y++)
    {
        float weightY = exp(-(y * y) * gaussianSigma);

        for (int x = -_radius; x <= _radius; x++)
        {
            float weightX = exp(-(x * x) * gaussianSigma);
            float weight = weightX * weightY;

            totalColor += _tex2D.SampleLevel(_samp2D, sampleUV + float2(x, y) * texelOffset, mipLevel) * weight;
            totalWeight += weight;
        }
    }

    return totalColor / max(totalWeight, 0.0001);
}
float4 BlurGaussianDirectional(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float2 _direction, float _strength, int _radius)
{
    uint width, height, mipCount;
    _tex2D.GetDimensions(0, width, height, mipCount);

    float mipLevel = min(2.0, (float)(mipCount - 1));
    float mipScale = exp2(mipLevel);
    float2 texelSize = 1.0 / float2(width, height);
    float2 texelOffset = texelSize * mipScale * _strength * _direction;

    float sigma = max(_radius * 0.5, 0.5);
    float invSigma2 = 0.5 / (sigma * sigma);

    float4 totalColor = 0.0;
    float totalWeight = 0.0;

    for (int i = -_radius; i <= _radius; i++)
    {
        float x = (float)i;
        float weight = exp(-(x * x) * invSigma2);

        totalColor += _tex2D.SampleLevel(_samp2D, _uv + texelOffset * x, mipLevel) * weight;
        totalWeight += weight;
    }

    return totalColor / max(totalWeight, 0.0001);
}

void BlurBoxDirectional_float(Texture2D _tex2D, SamplerState _samp2D, float2 _uv, float2 _direction, float _strength, int _radius, out float4 color)
{
    color = BlurBoxDirectional(_tex2D, _samp2D, _uv, _direction, _strength, _radius);
}
void BlurBoxDirectional_half(Texture2D _tex2D, SamplerState _samp2D, half2 _uv, half2 _direction, half _strength, int _radius, out half4 color)
{
    BlurBoxDirectional_float(_tex2D, _samp2D, _uv, _direction, _strength, _radius, color);
}

void BlurBox_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float _strength, out float4 color)
{
    color = BlurBox(_tex2D, _samp2D, _uv, _strength);
}
void BlurBox_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half _strength, out half4 color)
{
    BlurBox_float(_tex2D, _samp2D, _uv, _strength, color);
}

void BlurGaussian_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, in float _strength, in int _radius, in float _scale, out float4 color)
{
    color = BlurGaussian(_tex2D, _samp2D, _uv, _strength, _radius, _scale);
}
void BlurGaussian_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, in half _strength, in int _radius, in half _scale, out half4 color)
{
    BlurGaussian_float(_tex2D, _samp2D, _uv, _strength, _radius, _scale, color);
}

void BlurGaussianDirectional_float(in Texture2D _tex2D, in SamplerState _samp2D, in float2 _uv, float2 _direction, in float _strength, in int _radius, out float4 color)
{
    color = BlurGaussianDirectional(_tex2D, _samp2D, _uv, _direction, _strength, _radius);
}
void BlurGaussianDirectional_half(in Texture2D _tex2D, in SamplerState _samp2D, in half2 _uv, half2 _direction, in half _strength, in int _radius, out half4 color)
{
    BlurGaussianDirectional_float(_tex2D, _samp2D, _uv, _direction, _strength, _radius, color);
}
#endif