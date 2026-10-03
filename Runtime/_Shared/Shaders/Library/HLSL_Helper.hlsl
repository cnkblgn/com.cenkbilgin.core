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

static const float Bayer4x4[16] =
{
    0.000000, 0.500000, 0.125000, 0.625000,
    0.750000, 0.250000, 0.875000, 0.375000,
    0.187500, 0.687500, 0.062500, 0.562500,
    0.937500, 0.437500, 0.812500, 0.312500
};

float3 Hash33(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return frac((p.xxy + p.yxx) * p.zyx);
}
void GetHash33_float(float3 _p, out float3 value)
{
    value = Hash33(_p);
}
void GetHash33_half(half3 _p, out half3 value)
{
    GetHash33_float(_p, value);
}

float Hash31(float3 _p)
{
    _p = frac(_p * 0.3183099 + 0.1);
    _p *= 17.0;
    return frac(_p.x * _p.y * _p.z * (_p.x + _p.y + _p.z));
}
void GetHash31_float(float3 _p, out float value)
{
    value = Hash31(_p);
}
void GetHash31_half(half3 _p, out half value)
{
    GetHash31_float(_p, value);
}

void GetSimpleNoise_float(float3 _p, out float value)
{
    float3 i = floor(_p);
    float3 f = frac(_p);

    float a = Hash31(i);
    float b = Hash31(i + float3(1, 0, 0));
    float c = Hash31(i + float3(0, 1, 0));
    float d = Hash31(i + float3(1, 1, 0));

    float u = f.x;
    float v = f.y;

    float x1 = lerp(a, b, u);
    float x2 = lerp(c, d, u);

    value = lerp(x1, x2, v);
}
void GetSimpleNoise_half(float3 _p, out float value)
{
    GetSimpleNoise_float(_p, value);
}

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