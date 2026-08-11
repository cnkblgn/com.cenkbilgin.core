#ifndef HLSLFOG_INCLUDED
#define HLSLFOG_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"

uniform float3 _FOG_COLOR = float3(0.4, 0.4, 0.4);
uniform float _FOG_DENSITY = 1;
uniform float _FOG_DISTANCE_START = 64;
uniform float _FOG_DISTANCE_FALLOFF = 1;
uniform float _FOG_HEIGHT_START = 64;
uniform float _FOG_HEIGHT_FALLOFF = 1;
uniform float _FOG_SCATTERING = 5;

uniform sampler2D _FOG_NOISE_TEX;
uniform float _FOG_NOISE_SCALE = 0.05;
uniform float _FOG_NOISE_SPEED = 0.5;
uniform float _FOG_NOISE_STRENGTH = 0.5;

float3 Scatter(float3 _worldPosition, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, float _scattering)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);
    float d = saturate(dot(direction, _lightDirection));
    float s = pow(d, 8) * _scattering;
    return _lightColor * s;
}

float3 Ambient(float3 _worldPosition, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, float3 _ambientColor)
{
    float3 direction = normalize(_worldPosition - _cameraPosition);
    
    float sunDot = saturate(dot(direction, _lightDirection));
    sunDot = pow(sunDot, 2);
    
    float3 warmColor = _ambientColor + (_ambientColor * _lightColor * 15);
    float3 coolColor = _ambientColor * 0.5;

    return lerp(coolColor, warmColor, sunDot);
}

float SampleNoise(float3 _worldPosition, float _fade)
{
    float2 uv1 = _worldPosition.xz * _FOG_NOISE_SCALE + float2(_Time.y * _FOG_NOISE_SPEED * 0.1, 0);
    float2 uv2 = _worldPosition.xz * _FOG_NOISE_SCALE * 2.3 + float2(0, _Time.y * _FOG_NOISE_SPEED * 0.07);
    
    float n1 = tex2D(_FOG_NOISE_TEX, uv1).r;
    float n2 = tex2D(_FOG_NOISE_TEX, uv2).r;
    
    return (n1 * 0.6 + n2 * 0.4) * _fade;
}

float Fog(float3 _worldPosition, float3 _cameraPosition, float _distanceStart, float _distanceFalloff, float _heightStart, float _heightFalloff, float _density)
{
    float distance = length(_worldPosition - _cameraPosition);
    float height = _worldPosition.y;

    float distanceFog = smoothstep(0, _distanceStart, distance);
    distanceFog = pow(distanceFog, _distanceFalloff);

    float heightFog = 1 - smoothstep(0, _heightStart, height);
    heightFog = pow(heightFog, _heightFalloff);
    
    float baseFog = distanceFog * heightFog * _density;
    
    return saturate(baseFog);
}

float Fog(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, float _distanceStart, float _distanceFalloff, float _heightStart, float _heightFalloff, float _density)
{
    float distance = length(_worldPosition - _cameraPosition);
    float height = _worldPosition.y;

    float distanceFog = smoothstep(0, _distanceStart, distance);
    distanceFog = pow(distanceFog, _distanceFalloff);
            
    float heightFog = 1 - smoothstep(0, _heightStart, height);
    heightFog = pow(heightFog, _heightFalloff);
          
    float noiseMask = smoothstep(0.2, 0.6, abs(_worldNormal.y));
    float noiseFade = (1 - saturate(distance / (_FOG_DISTANCE_START))) * noiseMask;
    float noiseFog = SampleNoise(_worldPosition, noiseFade);
    
    float baseFog = distanceFog * heightFog * _density;

    baseFog = baseFog + (baseFog * noiseFog * _FOG_NOISE_STRENGTH * 2);

    return saturate(baseFog);
}

void GetFog_float(float3 _worldPosition, float3 _worldNormal, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, float2 _uv, out float3 color, out float factor)
{    
    float fogFactor = Fog(_worldPosition, _worldNormal, _cameraPosition, _FOG_DISTANCE_START, _FOG_DISTANCE_FALLOFF, _FOG_HEIGHT_START, _FOG_HEIGHT_FALLOFF, _FOG_DENSITY);
    float3 fogColor = Ambient(_worldPosition, _cameraPosition, _lightDirection, _lightColor, _FOG_COLOR);
    
    float3 scatterColor = Scatter(_worldPosition, _cameraPosition, _lightDirection, _lightColor, _FOG_SCATTERING);
    
    color = fogColor + scatterColor;
    factor = fogFactor;
}

void GetFog_half(half3 _worldPosition, half3 _worldNormal, half3 _cameraPosition, half3 _lightDirection, half3 _lightColor, float2 _uv, out half3 color, out half factor)
{
    GetFog_float(_worldPosition, _worldNormal, _cameraPosition, _lightDirection, _lightColor, _uv, color, factor);
}

void GetFogSimple_float(float3 _worldPosition, float3 _cameraPosition, float3 _lightDirection, float3 _lightColor, out float3 color, out float factor)
{
    float fogFactor = Fog(_worldPosition, _cameraPosition, _FOG_DISTANCE_START, _FOG_DISTANCE_FALLOFF, _FOG_HEIGHT_START, _FOG_HEIGHT_FALLOFF, _FOG_DENSITY);
    float3 fogColor = Ambient(_worldPosition, _cameraPosition, _lightDirection, _lightColor, _FOG_COLOR);
    
    color = fogColor;
    factor = fogFactor;
}

void GetFogSimple_half(half3 _worldPosition, half3 _cameraPosition, half3 _lightDirection, half3 _lightColor, out half3 color, out half factor)
{
    GetFogSimple_float(_worldPosition, _cameraPosition, _lightDirection, _lightColor, color, factor);
}
#endif