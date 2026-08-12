#ifndef HLSLSKY_INCLUDED
#define HLSLSKY_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"
#include "HLSL_Environment.hlsl"
#include "HLSL_Cloud.hlsl"
#include "HLSL_Celestial.hlsl"

struct SkySettings
{
    float3 zenithColor;
    float3 horizonColor;
    float horizonThickness;
    float horizonSoftness;
};

float3 DrawSky(float3 _viewDirection, SkySettings settings)
{
    float height = _viewDirection.y;
    
    float zenithMask = smoothstep(0, 0.25, height);
    float3 zenithColor = lerp(settings.zenithColor, settings.horizonColor, zenithMask);
    
    float horizonMask = 1 - smoothstep(settings.horizonThickness, settings.horizonThickness + settings.horizonSoftness, height * -1);
    float3 skyColor = lerp(zenithColor, settings.horizonColor, horizonMask);
    
    return skyColor;
}

void GetSky_float(float3 _worldPosition, float3 _viewDirection, out float3 color)
{
    SkySettings skySettings;
    skySettings.zenithColor = _ZENITH_COLOR;
    skySettings.horizonColor = _HORIZON_COLOR;
    skySettings.horizonThickness = _HORIZON_THICKNESS;
    skySettings.horizonSoftness = _HORIZON_SOFTNESS;    
    float3 skyColor = DrawSky(_viewDirection, skySettings);

    CelestialSettings celestialSunSettings;
    celestialSunSettings.color = _SUN_COLOR;
    celestialSunSettings.direction = saturate(dot(_viewDirection, _SUN_DIRECTION));
    celestialSunSettings.glow = _SUN_GLOW;
    celestialSunSettings.size = _SUN_SIZE;
    float3 sunColor = DrawCelestial(celestialSunSettings);

    CelestialSettings celestialMoonSettings;
    celestialMoonSettings.color = _MOON_COLOR;
    celestialMoonSettings.direction = saturate(dot(_viewDirection, _MOON_DIRECTION));
    celestialMoonSettings.glow = _MOON_GLOW;
    celestialMoonSettings.size = _MOON_SIZE;
    float3 moonColor = DrawCelestial(celestialMoonSettings);

    CloudSettings cloudSettings;
    cloudSettings.tint = _CLOUD_TINT;
    cloudSettings.blend = _CLOUD_TEX_BLEND;
    cloudSettings.coverage = _CLOUD_COVERAGE;
    cloudSettings.opacity = _CLOUD_OPACITY;
    cloudSettings.fade = _CLOUD_FADE;
    cloudSettings.height = _CLOUD_HEIGHT;
    cloudSettings.curve = _CLOUD_CURVE;
    cloudSettings.scale = _CLOUD_SCALE;
    cloudSettings.speed = _CLOUD_SPEED;
    cloudSettings.turbulence = _CLOUD_TURBULENCE;
    cloudSettings.transmittance = _CLOUD_TRANSMITTANCE;
    cloudSettings.darkness = _CLOUD_DARKNESS;
    cloudSettings.rimWidth = _CLOUD_RIM_WIDTH;
    cloudSettings.rimStrength = _CLOUD_RIM_STRENGTH;

    CloudDensity cloudDensity = GetCloudDensity(_viewDirection, _CLOUD_TEX_A, _CLOUD_TEX_B, cloudSettings);
    
    float4 cloudSun = DrawCloud(_viewDirection, -_SUN_DIRECTION, _SUN_COLOR, skyColor, cloudDensity, cloudSettings);
    float4 cloudMoon = DrawCloud(_viewDirection, -_MOON_DIRECTION, _MOON_COLOR, skyColor, cloudDensity, cloudSettings);

    float3 cloudColor = cloudSun.rgb + cloudMoon.rgb;
    float cloudAlpha = cloudDensity.value;

    float3 baseColor = skyColor + sunColor + moonColor;
    color = lerp(baseColor, cloudColor, cloudAlpha);
}

void GetSky_half(half3 _worldPosition, half3 _viewDirection, out half3 color)
{
    GetSky_float(_worldPosition, _viewDirection, color);
}

#endif