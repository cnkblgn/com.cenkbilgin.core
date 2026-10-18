#ifndef HLSLFOG_INCLUDED
#define HLSLFOG_INCLUDED

// gölgeler için!
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"
#include "../../../_Shared/Shaders/Library/HLSL_Light.hlsl"
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

float3 GetFogScatter(float3 _worldPosition, float3 _cameraPosition, float3 _sunDirection, float3 _sunColor, float _scattering)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);
    float d = saturate(dot(direction, -_sunDirection));
    float s = pow(d, 8) * _scattering;
    
    return _sunColor * s;
}

float3 GetFogAmbient(float3 _worldPosition, float3 _cameraPosition, float3 _sunDirection, float3 _sunColor, float3 _ambientColor)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);

    float sunDot = saturate(dot(direction, -_sunDirection));
    sunDot = pow(sunDot, 16);

    float3 warmColor = _ambientColor + (_ambientColor * _sunColor);
    float3 coolColor = _ambientColor * 0.5;

    return lerp(coolColor, warmColor, sunDot);
}

float GetFogSample(float3 _worldPosition, float _fade, sampler2D _noiseTex, float _noiseScale, float _noiseSpeed)
{
    float2 uv1 = _worldPosition.xz * (_noiseScale * 0.01) + float2(_Time.y * _noiseSpeed * 0.1, 0);
    float2 uv2 = _worldPosition.xz * (_noiseScale * 0.01) * 2.3 + float2(0, _Time.y * _noiseSpeed * 0.07);

    float n1 = tex2D(_noiseTex, uv1).r;
    float n2 = tex2D(_noiseTex, uv2).r;

    return (n1 * 0.6 + n2 * 0.4) * _fade;
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

float GetFogFactor(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, sampler2D _noiseTex, float _noiseScale, float _noiseSpeed, float _noiseStrength, FogSettings settings)
{    
    FogDensity density = GetFogDensity(_worldPosition, _cameraPosition, settings);
    
    float factor = density.value;

    float noiseMask = smoothstep(0.2, 0.6, abs(_worldNormal.y));
    float noiseFade = (1 - saturate(density.distance / settings.distanceStart)) * noiseMask;
    float noiseFog = GetFogSample(_worldPosition, noiseFade, _noiseTex, _noiseScale, _noiseSpeed);

    factor = factor + (factor * noiseFog * _noiseStrength * 2);

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
        
    float fogFactor = GetFogFactor(_worldPosition, _worldNormal, _cameraPosition, _FOG_NOISE_TEX, _FOG_NOISE_SCALE, _FOG_NOISE_SPEED, _FOG_NOISE_STRENGTH, settings);
    float3 fogAmbient = GetFogAmbient(_worldPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_COLOR);
    float3 fogScatter = GetFogScatter(_worldPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_SCATTERING);

    color = fogAmbient + fogScatter;
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

    color = fogAmbient;
    factor = fogFactor;
}

void GetFogSimple_half(half3 _worldPosition, half3 _cameraPosition, out half3 color, out half factor)
{
    GetFogSimple_float(_worldPosition, _cameraPosition, color, factor);
}

void GetFogVolumetric_float(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, out float3 color, out float factor)
{
    FogSettings settings;
    settings.density = _FOG_DENSITY;
    settings.distanceStart = _FOG_DISTANCE_START;
    settings.distanceFalloff = _FOG_DISTANCE_FALLOFF;
    settings.heightStart = _FOG_HEIGHT_START;
    settings.heightFalloff = _FOG_HEIGHT_FALLOFF;

    float3 rayDir = normalize(_worldPosition - _cameraPosition);
    float maxDist = length(_worldPosition - _cameraPosition);

    int stepCount = _FOG_STEP_COUNT;
    float stepSize = maxDist / stepCount;

    float stepRandom = frac(sin(dot(_worldPosition.xz, float2(12.9898, 78.233))) * 43758.5453);
    float stepOffset = (stepRandom - 0.5) * stepSize;

    float3 accumulatedColor = float3(0, 0, 0);
    float accumulatedFactor = 0;

    [unroll(32)]
    for (int i = 0; i < stepCount; i++)
    {
        if (accumulatedFactor >= 0.99)
        {
            break;
        }

        float currentDistance = i * stepSize + stepOffset;
        float3 currentPosition = _cameraPosition + rayDir * currentDistance;

        FogDensity density = GetFogDensity(currentPosition, _cameraPosition, settings);      
        float stepFactor = density.value;

        if (stepFactor < 0.001)
        {
            continue;
        }

        float distanceFromCamera = density.distance;
        
        if (distanceFromCamera < settings.distanceStart)
        {
            float noiseMask = smoothstep(0.2, 0.6, abs(_worldNormal.y));
            float noiseFade = (1 - saturate(distanceFromCamera / settings.distanceStart)) * noiseMask;
            float noiseFog = GetFogSample(currentPosition, noiseFade, _FOG_NOISE_TEX, _FOG_NOISE_SCALE, _FOG_NOISE_SPEED);
            
            stepFactor = stepFactor + (stepFactor * noiseFog * _FOG_NOISE_STRENGTH * 4);
        }

        float3 lightDirection;
        float3 lightColor;
        float shadowAttenuation;
        float shadowInfluence;
        float shadowStrength = 1;
        GetMainLight_float(currentPosition, lightDirection, lightColor, shadowAttenuation);
        shadowInfluence = lerp(1.0, shadowAttenuation, shadowStrength);

        stepFactor *= shadowInfluence;

        float3 stepAmbient = GetFogAmbient(currentPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_COLOR);
        float3 stepScatter = GetFogScatter(currentPosition, _cameraPosition, _SUN_DIRECTION, _SUN_COLOR, _FOG_SCATTERING) * shadowInfluence;
        float3 stepColor = stepAmbient + stepScatter;

        accumulatedColor += stepColor * stepFactor * (1 - accumulatedFactor);
        accumulatedFactor += stepFactor * (1 - accumulatedFactor);
    }

    color = accumulatedColor;
    factor = saturate(accumulatedFactor);
}

void GetFogVolumetric_half(half3 _worldPosition, half3 _worldNormal, half3 _cameraPosition, out half3 color, out half factor)
{
    GetFogVolumetric_float(_worldPosition, _worldNormal, _cameraPosition, color, factor);
}
#endif