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
    float opacity;
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
    
    float forwardScatter = pow(saturate(dot(direction, -_sunDirection)), 3.0);   
    float offsetScatter = 0.25;   
    float scatter = forwardScatter + offsetScatter;
    
    return _sunColor * scatter * _scattering;
}

float3 GetFogAmbient(float3 _worldPosition, float3 _cameraPosition, float3 _sunDirection, float3 _sunColor, float3 _ambientColor)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);

    float sunDot = saturate(dot(direction, -_sunDirection));
    sunDot = smoothstep(0.0, 1.0, sunDot); 
    
    float backDot = saturate(dot(direction, _sunDirection));
    backDot = pow(backDot, 4.0);
    
    float3 sunsetColor = _sunColor * 1.5;
    float3 baseColor = _ambientColor * 0.7;
    float3 backColor = _sunColor * 0.8;
    
    float3 result = lerp(baseColor, sunsetColor, sunDot);
    result = lerp(result, backColor, backDot * 0.5);
    
    return result;
}

float GetFogSample(float3 _worldPosition, sampler2D _noiseTex, float _noiseScale, float _noiseSpeed)
{
    float2 uv1 = _worldPosition.xz * (_noiseScale * 0.01) + float2(_Time.y * _noiseSpeed * 0.25, 0);
    float2 uv2 = _worldPosition.xz * (_noiseScale * 0.01) * 2.3 + float2(0, _Time.y * _noiseSpeed * 0.15);

    float n1 = tex2D(_noiseTex, uv1).r;
    float n2 = tex2D(_noiseTex, uv2).r;

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
    return saturate(GetFogDensity(_worldPosition, _cameraPosition, settings).value * settings.opacity);
}

float GetFogFactor(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, sampler2D _noiseTex, float noiseOpacity, float _noiseScale, float _noiseSpeed, float _noiseModulation, float _noiseOffset, FogSettings settings)
{    
    FogDensity density = GetFogDensity(_worldPosition, _cameraPosition, settings);
    
    float factor = density.value;

    float noiseMask = smoothstep(0.2, 0.6, abs(_worldNormal.y));
    float noiseFade = (1 - saturate(density.distance / settings.distanceStart)) * noiseMask;
    float noiseFog = GetFogSample(_worldPosition, _noiseTex, _noiseScale, _noiseSpeed);
    float noiseModulation = (noiseFog * 2.0 - 1.0) * _noiseModulation;
    factor *= max(0.0, 1.0 + noiseModulation * noiseFade * noiseOpacity);
    factor += noiseFog * noiseFade * _noiseOffset * noiseOpacity;
    
    return saturate(factor * settings.opacity);
}

void GetFogDefault_float(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, out float3 color, out float factor)
{
    FogSettings settings;
    settings.density = _FOG_DENSITY;
    settings.opacity = _FOG_OPACITY;
    settings.distanceStart = _FOG_DISTANCE_START;
    settings.distanceFalloff = _FOG_DISTANCE_FALLOFF;
    settings.heightStart = _FOG_HEIGHT_START;
    settings.heightFalloff = _FOG_HEIGHT_FALLOFF;
        
    float fogFactor = GetFogFactor(_worldPosition, _worldNormal, _cameraPosition, _FOG_NOISE_TEX, _FOG_NOISE_OPACITY,  _FOG_NOISE_SCALE, _FOG_NOISE_SPEED, _FOG_NOISE_MODULATION, _FOG_NOISE_OFFSET, settings);
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
    settings.opacity = _FOG_OPACITY;
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
    settings.opacity = _FOG_OPACITY;
    settings.distanceStart = _FOG_DISTANCE_START;
    settings.distanceFalloff = _FOG_DISTANCE_FALLOFF;
    settings.heightStart = _FOG_HEIGHT_START;
    settings.heightFalloff = _FOG_HEIGHT_FALLOFF;

    float3 rayDir = normalize(_worldPosition - _cameraPosition);
    float maxDist = length(_worldPosition - _cameraPosition);

    int stepCount = _FOG_STEP_COUNT;
    float stepSize = maxDist / stepCount;

    float stepRandom = frac(sin(dot(_worldPosition.xz, float2(12.9898, 78.233))) * 43758.5453);
    float stepOffset = stepRandom * stepSize;

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
        float stepDensity = density.value;

        if (stepDensity < 0.0001)
        {
            continue;
        }

        float distanceFromCamera = density.distance;

        if (distanceFromCamera < settings.distanceStart)
        {
            float noiseFade = (1.0 - saturate(distanceFromCamera / settings.distanceStart));
            float noiseFog = GetFogSample(currentPosition, _FOG_NOISE_TEX, _FOG_NOISE_SCALE, _FOG_NOISE_SPEED);
            float noiseModulation = (noiseFog * 2.0 - 1.0) * _FOG_NOISE_MODULATION;
            stepDensity *= max(0.0, 1.0 + noiseModulation * noiseFade *_FOG_NOISE_OPACITY);         
            stepDensity += noiseFog * noiseFade * _FOG_NOISE_OFFSET * _FOG_NOISE_OPACITY;
        }
        
        float stepFactor = 1.0 - exp(-stepDensity * stepSize);
        
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

        float transmittance = 1.0 - accumulatedFactor;
        accumulatedColor += stepColor * stepFactor * transmittance;
        accumulatedFactor += stepFactor * transmittance;
    }

    color = accumulatedColor * settings.opacity;
    factor = saturate(accumulatedFactor * settings.opacity);
}

void GetFogVolumetric_half(half3 _worldPosition, half3 _worldNormal, half3 _cameraPosition, out half3 color, out half factor)
{
    GetFogVolumetric_float(_worldPosition, _worldNormal, _cameraPosition, color, factor);
}
#endif