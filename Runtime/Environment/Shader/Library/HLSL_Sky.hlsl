#ifndef HLSLSKY_INCLUDED
#define HLSLSKY_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"
#include "HLSL_Cloud.hlsl" 

uniform float3 _ZENITH_COLOR = float3(0.75, 0.86, 0.75);
uniform float3 _HORIZON_COLOR = float3(0.4, 0.4, 0.4);
uniform float _HORIZON_THICKNESS = 0.15;
uniform float _HORIZON_SOFTNESS = 0.25;

uniform float3 _SUN_COLOR = float3(1, 1, 1);
uniform float3 _SUN_DIRECTION = float3(0, -1, 0);
uniform float _SUN_SIZE = 0.02;
uniform float _SUN_GLOW = 32;

uniform float3 _MOON_COLOR = float3(1, 1, 1);
uniform float3 _MOON_DIRECTION = float3(0, 1, 0);
uniform float _MOON_SIZE = 0.02;
uniform float _MOON_GLOW = 32;

float3 DrawCelestial(float3 _color, float _direction, float _glow, float _size)
{
    float disc = smoothstep(1 - _size, 1, _direction);
    float3 discColor = disc * _color ;

    float glow = pow(_direction, _glow);
    float3 glowColor = glow * _color;

    return discColor + glowColor;
}

void GetSky_float(float3 _worldPosition, float3 _viewDirection, out float3 color)
{
    float height = _viewDirection.y;
    
    float zenithMask = smoothstep(0, 0.25, height);   
    float3 zenithColor = lerp(_ZENITH_COLOR, _HORIZON_COLOR, zenithMask);
        
    float horizonMask = 1 - smoothstep(_HORIZON_THICKNESS, _HORIZON_THICKNESS + _HORIZON_SOFTNESS, height * -1);
    float3 skyColor = lerp(zenithColor, _HORIZON_COLOR, horizonMask);
    
    float sunDirection = saturate(dot(_viewDirection, _SUN_DIRECTION));
    float3 sunColor = DrawCelestial(_SUN_COLOR, sunDirection, _SUN_GLOW, _SUN_SIZE);
    
    float moonDirection = saturate(dot(_viewDirection, _MOON_DIRECTION));
    float3 moonColor = DrawCelestial(_MOON_COLOR, moonDirection, _MOON_GLOW, _MOON_SIZE);   
  
    float isDay = saturate(sign(-_SUN_DIRECTION.y));
    float4 cloudSun = DrawCloud(_viewDirection, -_SUN_DIRECTION, _SUN_COLOR, skyColor);
    float4 cloudMoon = DrawCloud(_viewDirection, -_MOON_DIRECTION, _MOON_COLOR, skyColor);

    float3 cloudColor = cloudSun.rgb * isDay + cloudMoon.rgb * (1 - isDay);
    float cloudDensity = max(cloudSun.a, cloudMoon.a);

    float3 baseColor = skyColor + sunColor + moonColor;
    color = lerp(baseColor, cloudColor, cloudDensity);
}

void GetSky_half(half3 _worldPosition, half3 _viewDirection, out half3 color)
{
    GetSky_float(_worldPosition, _viewDirection, color);
}

#endif