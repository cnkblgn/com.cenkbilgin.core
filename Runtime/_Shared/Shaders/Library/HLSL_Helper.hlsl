#ifndef HLSLHELPER_INCLUDED
#define HLSLHELPER_INCLUDED

#define LUMA float3(0.299, 0.587, 0.114)
#define GRAY float3(0.34543, 0.65456, 0.287)
#define OFFSET float(0.0008)

static const float2 OFFSETS[9] =
{
    float2(-1, -1), float2(0, -1), float2(1, -1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, 1), float2(0, 1), float2(1, 1)
};

static const float4x4 Bayer4x4 = float4x4
(
    float4(0.0 / 16.0, 8.0 / 16.0, 2.0 / 16.0, 10.0 / 16.0),
    float4(12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0, 6.0 / 16.0),
    float4(3.0 / 16.0, 11.0 / 16.0, 1.0 / 16.0, 9.0 / 16.0),
    float4(15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0, 5.0 / 16.0)
);

float2 Hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}
void GetHash22_float(float2 _p, out float2 value)
{
    value = Hash22(_p);
}
void GetHash22_half(half2 _p, out half2 value)
{
    GetHash22_float(_p, value);
}

float GradientNoise(float2 _p)
{
    float2 i = floor(_p);
    float2 f = frac(_p);
    float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

    return lerp
    (
        lerp(dot(Hash22(i + float2(0, 0)), f - float2(0, 0)), dot(Hash22(i + float2(1, 0)), f - float2(1, 0)), u.x),
        lerp(dot(Hash22(i + float2(0, 1)), f - float2(0, 1)), dot(Hash22(i + float2(1, 1)), f - float2(1, 1)), u.x), u.y
    );
}
void GetGradientNoise_float(float2 _p, out float value)
{
    value = GradientNoise(_p);
}
void GetGradientNoise_half(float2 _p, out float value)
{
    GetGradientNoise_float(_p, value);
}

float FBM(float2 _p, float _persistence, float _lacunarity, int _octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < _octaves; i++)
    {
        value += amplitude * GradientNoise(_p * frequency);
        
        amplitude *= _persistence;
        frequency *= _lacunarity * 1.05;
    }
    return value;
}
void GetFBM_float(float2 _p, float _persistence, float _lacunarity, int _octaves, out float value)
{
    value = FBM(_p, _persistence, _lacunarity, _octaves);
}
void GetFBM_half(half2 _p, half _persistence, half _lacunarity, int _octaves, out half value)
{
    GetFBM_float(_p, _persistence, _lacunarity, _octaves, value);
}

float remap(float _in, float2 _inMinMax, float2 _outMinMax)
{
    return _outMinMax.x + (_in - _inMinMax.x) * (_outMinMax.y - _outMinMax.x) / (_inMinMax.y - _inMinMax.x);
}
float4 remap(float4 _in, float2 _inMinMax, float2 _outMinMax)
{
    return _outMinMax.x + (_in - _inMinMax.x) * (_outMinMax.y - _outMinMax.x) / (_inMinMax.y - _inMinMax.x);
}

#endif