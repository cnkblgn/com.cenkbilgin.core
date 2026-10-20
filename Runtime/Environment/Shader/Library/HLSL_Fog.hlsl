#ifndef HLSLFOG_INCLUDED
#define HLSLFOG_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"
#include "../../../_Shared/Shaders/Library/HLSL_Light.hlsl"
#include "../../../_Shared/Shaders/Library/HLSL_Depth.hlsl"
#include "HLSL_Environment.hlsl"

struct FogSettings
{
    float density;
    float distanceStart;
    float distanceFalloff;
    float heightStart;
    float heightFalloff;
};
    
struct FogDensity
{
    float distance;
    float value;
};


float3 GetFogScatter(float3 _worldPosition, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, float _scattering)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);
    float scatter = pow(saturate(dot(direction, -_lightDirection)), 4.0);
    
    return _lightColor * scatter * _scattering;
}

float3 GetFogAmbient(float3 _worldPosition, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, float3 _ambientColor)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);

    float sunDot = saturate(dot(direction, -_lightDirection));
    sunDot = smoothstep(0.0, 1.0, sunDot); 
    
    float backDot = saturate(dot(direction, _lightDirection));
    backDot = pow(backDot, 4.0);
    
    float3 sunsetColor = _lightColor * 1.5;
    float3 baseColor = _ambientColor * 0.7;
    float3 backColor = _lightColor * 0.8;
    
    float3 result = lerp(baseColor, sunsetColor, sunDot);
    result = lerp(result, backColor, backDot * 0.5);
    
    return result;
}

float3 GetFogRamp(float3 _worldPosition, float3 _cameraPosition, sampler2D _tex2D, float _scale, float _startDistance, float _startHeight)
{
    float distance = length(_worldPosition - _cameraPosition);

    float distanceEnd = max(_startDistance * _scale, 0.001);
    float heightEnd = max(_startHeight * _scale, 0.001);

    float distanceUV = saturate(distance / distanceEnd);
    float heightUV = saturate(_worldPosition.y / heightEnd);

    float2 uv = float2(distanceUV, heightUV);

    return tex2Dlod(_tex2D, float4(uv, 0, 0)).rgb;;
}

float GetFogNoise(float3 _worldPosition, sampler2D _tex2D, float _scale, float _speed)
{
    float2 uv1 = _worldPosition.xz * (_scale * 0.01) + float2(_Time.y * _speed * 0.25, 0);
    float2 uv2 = _worldPosition.xz * (_scale * 0.01) * 2.3 + float2(0, _Time.y * _speed * 0.15);

    float n1 = tex2D(_tex2D, uv1).r;
    float n2 = tex2D(_tex2D, uv2).r;

    return (n1 * 0.6 + n2 * 0.4);
}

FogDensity GetFogDensity(float3 _worldPosition, float3 _cameraPosition, FogSettings settings)
{
    float distance = length(_worldPosition - _cameraPosition);
    float height = _worldPosition.y;

    float distanceFog = smoothstep(0, settings.distanceStart, distance);
    distanceFog = pow(distanceFog, settings.distanceFalloff);

    float heightFog = 1 - smoothstep(0, settings.heightStart, height);
    heightFog = pow(heightFog, settings.heightFalloff);

    FogDensity result;
    result.distance = distance;
    result.value = distanceFog * heightFog * settings.density;
    
    return result;
}

float GetFogFactor(float3 _worldPosition, float3 _cameraPosition, FogSettings settings)
{    
    return saturate(GetFogDensity(_worldPosition, _cameraPosition, settings).value);
}

float GetFogFactor(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, sampler2D _noiseTex, float noiseOpacity, float _noiseScale, float _noiseSpeed, float _noiseModulation, float _noiseOffset, FogSettings settings)
{    
    FogDensity density = GetFogDensity(_worldPosition, _cameraPosition, settings);
    
    float factor = density.value;

    float noiseMask = smoothstep(0.2, 0.6, abs(_worldNormal.y));
    float noiseFade = (1 - saturate(density.distance / settings.distanceStart)) * noiseMask;
    float noiseFog = GetFogNoise(_worldPosition, _noiseTex, _noiseScale, _noiseSpeed);
    float noiseModulation = (noiseFog * 2.0 - 1.0) * _noiseModulation;
    
    factor *= max(0.0, 1.0 + noiseModulation * noiseFade * noiseOpacity);
    factor += noiseFog * noiseFade * _noiseOffset * noiseOpacity;
    
    return saturate(factor);
}

void GetFogDefault_float(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, out float3 color, out float factor)
{
    FogSettings settings;
    settings.density = _FOG_DENSITY;
    settings.distanceStart = _FOG_DISTANCE_START;
    settings.distanceFalloff = _FOG_DISTANCE_FALLOFF;
    settings.heightStart = _FOG_HEIGHT_START;
    settings.heightFalloff = _FOG_HEIGHT_FALLOFF;
        
    float fogFactor = GetFogFactor(_worldPosition, _worldNormal, _cameraPosition, _FOG_NOISE_TEX, _FOG_NOISE_OPACITY,  _FOG_NOISE_SCALE, _FOG_NOISE_SPEED, _FOG_NOISE_MODULATION, _FOG_NOISE_OFFSET, settings);
    float3 fogAmbient = GetFogAmbient(_worldPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_COLOR);
    float3 fogScatter = GetFogScatter(_worldPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_SCATTERING);
    float3 fogRamp = GetFogRamp(_worldPosition, _cameraPosition, _FOG_RAMP_TEX, _FOG_RAMP_SCALE, _FOG_DISTANCE_START, _FOG_HEIGHT_START);
    
    color = lerp(fogAmbient, fogRamp, saturate(_FOG_RAMP_OPACITY)) + fogScatter;
    factor = fogFactor;    
}

void GetFogDefault_half(half3 _worldPosition, half3 _worldNormal, half3 _cameraPosition, out half3 color, out half factor)
{
    GetFogDefault_half(_worldPosition, _worldNormal, _cameraPosition, color, factor);
}

void GetFogSimple_float(float3 _worldPosition, float3 _cameraPosition, out float3 color, out float factor)
{
    FogSettings settings;
    settings.density = _FOG_DENSITY;
    settings.distanceStart = _FOG_DISTANCE_START;
    settings.distanceFalloff = _FOG_DISTANCE_FALLOFF;
    settings.heightStart = _FOG_HEIGHT_START;
    settings.heightFalloff = _FOG_HEIGHT_FALLOFF;
    
    float fogFactor = GetFogFactor(_worldPosition, _cameraPosition, settings);
    float3 fogAmbient = GetFogAmbient(_worldPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_COLOR);
    float3 fogRamp = GetFogRamp(_worldPosition, _cameraPosition, _FOG_RAMP_TEX, _FOG_RAMP_SCALE, _FOG_DISTANCE_START, _FOG_HEIGHT_START);
    
    color = lerp(fogAmbient, fogRamp, saturate(_FOG_RAMP_OPACITY));
    factor = fogFactor;
}

void GetFogSimple_half(half3 _worldPosition, half3 _cameraPosition, out half3 color, out half factor)
{
    GetFogSimple_float(_worldPosition, _cameraPosition, color, factor);
}
#endif