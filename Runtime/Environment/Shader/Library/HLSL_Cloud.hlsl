#ifndef HLSLCLOUD_INCLUDED
#define HLSLCLOUD_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"

struct CloudSettings
{
    float3 color;
    float blend;
    float coverage;
    float opacity;
    float fade;
    float height;
    float curve;
    float scale;
    float turbulence;
    float transmittance;
    float darkness;
    float rimWidth;
    float rimStrength;
};

struct CloudDensity
{
    float shape;
    float value;
};

float GetCloudHash(float3 _viewDirection, float _speed, float _scale)
{
    float time = _Time.y * _speed;
    float3 direction = normalize(_viewDirection) * _scale;
    float3 position = float3(direction.x + direction.y * 0.37 + direction.z * 0.19, direction.y + direction.z * 0.43 + direction.x * 0.23, direction.z + direction.x * 0.31 + direction.y * 0.17);

    position += float3(time * 0.45, -time * 0.31, time * 0.17);

    return Hash31(floor(position));
}

float3 GetCloudUVPoint(float3 _viewDirection, float _curve, float _height)
{
    float r = _curve;
    float h = _height;
    float t = sqrt(r * r * _viewDirection.y * _viewDirection.y + 2.0 * r * h + h * h) - r * _viewDirection.y;
    return _viewDirection * t;
}

float2 GetCloudUV(float3 _viewDirection, float _curve, float _height, float _scale)
{
    return GetCloudUVPoint(_viewDirection, _curve, _height).xz * _scale * 0.1;
}

float4 GetCloudSample(sampler2D _texA, sampler2D _texB, float _blend, float2 _uv)
{
    float4 sampleA = tex2D(_texA, _uv);

    [branch]
    if (_blend > 0)
    {
        float4 sampleB = tex2D(_texB, _uv);
        return lerp(sampleA, sampleB, _blend);
    }

    return sampleA;
}

CloudDensity GetCloudDensity(float3 _viewDirection, sampler2D _texA, sampler2D _texB, float _time, CloudSettings _settings)
{
    float3 viewDirection = normalize(-_viewDirection);

    float2 baseUV = GetCloudUV(viewDirection, _settings.curve, _settings.height, _settings.scale);
    float2 flowUV = baseUV + float2(_time * 0.01, _time * 0.004);
    float2 distortionUV = flowUV * 2.0 + float2(-_time * 0.02, _time * 0.015);
    float2 distortionSample = GetCloudSample(_texA, _texB, _settings.blend, distortionUV).gb;

    float2 warpUV = (distortionSample * 2.0 - 1.0) * _settings.turbulence;
    float baseSample = GetCloudSample(_texA, _texB, _settings.blend, flowUV + warpUV).r;
    
    float mask = smoothstep(0, _settings.fade, viewDirection.y);
    float shape = saturate(baseSample - (1.0 - _settings.coverage));
    float density = shape * mask * _settings.opacity;
    density = saturate(density * 4);

    CloudDensity result;
    result.shape = shape;
    result.value = density;
    return result;
}

float4 DrawCloud(float3 _viewDirection, float3 _lightDirection, float3 _lightColor, float3 _skyColor, CloudDensity _density, CloudSettings _settings)
{
    float3 viewDirection = normalize(-_viewDirection);

    float sunDot = pow(saturate(dot(viewDirection, _lightDirection)), 4);
    float sunHeight = saturate(_lightDirection.y);

    float absorption = exp(-_density.value * _density.value);
    float transmittance = lerp(absorption, 1, sunDot * 0.6 + sunHeight * 0.4);
    
    transmittance = saturate(transmittance * _density.shape * sunDot * _settings.transmittance * 2);

    float3 shadowColor = _skyColor * (_skyColor + (1.0 - _settings.darkness));
    float3 sunsetColor = _lightColor * 0.25;
    float3 litColor = lerp(_lightColor, sunsetColor, sunHeight * 0.75f);
    float3 cloudColor = lerp(shadowColor, litColor, transmittance);

    float rimAmount = 1.0 - _density.value;
    float rimMask = smoothstep(0.0, _settings.rimWidth, rimAmount) * (1.0 - smoothstep(_settings.rimWidth, _settings.rimWidth * 3.0, rimAmount));
    float3 rimColor = _lightColor * pow(sunDot, 8.0) * rimMask * _settings.rimStrength;

    cloudColor += rimColor;
    cloudColor *= lerp(0.75, 1.05, GetCloudHash(_viewDirection, 12, 512));

    return float4(cloudColor * _settings.color, saturate(_density.value));
}
#endif