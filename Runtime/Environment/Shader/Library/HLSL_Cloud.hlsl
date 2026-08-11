#ifndef HLSLCLOUD_INCLUDED
#define HLSLCLOUD_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"

uniform sampler2D _CLOUD_TEX_A; // R -> Mask, G -> +Distortion, B -> -Distortion
uniform sampler2D _CLOUD_TEX_B; // R -> Mask, G -> +Distortion, B -> -Distortion
uniform float _CLOUD_TEX_BLEND = 0;

uniform float3 _CLOUD_TINT = float3(1, 1, 1);
uniform float _CLOUD_COVERAGE = 1;
uniform float _CLOUD_OPACITY = 1;
uniform float _CLOUD_FADE = 0;
uniform float _CLOUD_HEIGHT = 8;
uniform float _CLOUD_CURVE = 128;
uniform float _CLOUD_SCALE = 2;
uniform float _CLOUD_SPEED = 0.05;
uniform float _CLOUD_TURBULENCE = 0.4;
uniform float _CLOUD_TRANSMITTANCE = 0.5;
uniform float _CLOUD_DARKNESS = 0.5;
uniform float _CLOUD_RIM_WIDTH = 0.05;
uniform float _CLOUD_RIM_STRENGTH = 5;

float3 GetPoint(float3 _viewDirection, float _curve, float _height)
{
    float r = _curve;
    float h = _height;

    float t = sqrt(r * r * _viewDirection.y * _viewDirection.y + 2.0 * r * h + h * h) - r * _viewDirection.y;

    return _viewDirection * t;
}

float2 GetUV(float3 _viewDirection)
{
    return GetPoint(_viewDirection, _CLOUD_CURVE, _CLOUD_HEIGHT).xz * _CLOUD_SCALE * 0.1;
}

float4 DrawCloud(float3 _viewDirection, float3 _lightDirection, float3 _lightColor, float3 _skyColor)
{
    float time = _Time.y;
    
    float3 viewDirection = normalize(-_viewDirection);
    
    float2 baseUV = GetUV(viewDirection);

    float2 distortionUV = float2(frac(baseUV.x + time * _CLOUD_SPEED * 0.01), frac(baseUV.y + time * -_CLOUD_SPEED * 0.01));
    float2 distortionSample = tex2D(_CLOUD_TEX_A, distortionUV).gb;
    [branch]
    if (_CLOUD_TEX_BLEND > 0)
    {
        float2 distortionSampleB = tex2D(_CLOUD_TEX_B, distortionUV).gb;
        distortionSample = lerp(distortionSample, distortionSampleB, _CLOUD_TEX_BLEND);
    }

    float2 warpUV = (float2(distortionSample.r, distortionSample.g) * 2.0 - 1.0) * _CLOUD_TURBULENCE;

    float baseSample = tex2D(_CLOUD_TEX_A, baseUV + warpUV).r;
    [branch]
    if (_CLOUD_TEX_BLEND > 0)
    {
        float baseSampleB = tex2D(_CLOUD_TEX_B, baseUV + warpUV).r;
        baseSample = lerp(baseSample, baseSampleB, _CLOUD_TEX_BLEND);
    }
    
    float cloudMask = smoothstep(0, _CLOUD_FADE, viewDirection.y);
    float cloudShape = saturate(baseSample - (1.0 - _CLOUD_COVERAGE));
    float cloudDensity = cloudShape * cloudMask * _CLOUD_OPACITY;
    cloudDensity = saturate(cloudDensity * 4);
    
    float NdotL = saturate(dot(viewDirection, _lightDirection));
    float sunHeight = saturate(_lightDirection.y);

    float absorbsition = exp(-cloudDensity * cloudDensity);
    float transmittance = lerp(absorbsition, 1, NdotL * 0.6 + sunHeight * 0.4);
    transmittance = saturate(transmittance * cloudShape * NdotL * _CLOUD_TRANSMITTANCE * 2);

    float3 shadowColor = _skyColor * (_skyColor + (1.0 - _CLOUD_DARKNESS));
    float3 sunsetColor = _lightColor * 0.25;
    float3 litColor = lerp(_lightColor, sunsetColor, sunHeight * 0.75f);
    float3 cloudColor = lerp(shadowColor, litColor, transmittance);

    float rimAmount = 1.0 - cloudDensity;
    float rimMask = smoothstep(0.0, _CLOUD_RIM_WIDTH, rimAmount) * (1.0 - smoothstep(_CLOUD_RIM_WIDTH, _CLOUD_RIM_WIDTH * 3.0, rimAmount));
    float3 rimColor = _lightColor * pow(NdotL, 16.0) * rimMask * _CLOUD_RIM_STRENGTH;

    cloudColor += rimColor;

    return float4(cloudColor * _CLOUD_TINT, saturate(cloudDensity));
}

#endif