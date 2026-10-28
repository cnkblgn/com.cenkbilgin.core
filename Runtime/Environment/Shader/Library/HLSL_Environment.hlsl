#ifndef HLSLENVIRONMENT_INCLUDED
#define HLSLENVIRONMENT_INCLUDED

// ENVIRONMENT START
#define _ENVIRONMENT_MAX_LIGHT 16
uniform float _ENVIRONMENT_LIGHT_COUNT;
uniform float4 _ENVIRONMENT_LIGHT_POSITION[_ENVIRONMENT_MAX_LIGHT]; // xyz = position, w = range
uniform float4 _ENVIRONMENT_LIGHT_COLOR[_ENVIRONMENT_MAX_LIGHT]; // rgb = color * intensity, w = spot offset
uniform float4 _ENVIRONMENT_TIME;

float3 GetEnvironmentLight(float3 _worldPosition, float3 _cameraPosition)
{
    int count = (int) _ENVIRONMENT_LIGHT_COUNT;

    if (count <= 0)
    {
        return 0;
    }

    float3 toPixel = _worldPosition - _cameraPosition;
    
    float rayLength = length(toPixel);
    float3 rayDirection = toPixel / max(rayLength, 0.0001);
    
    float3 result = 0;

    [loop]
    for (int i = 0; i < count; i++)
    {
        float4 lightPosition = _ENVIRONMENT_LIGHT_POSITION[i];
        float3 toLight = lightPosition.xyz - _cameraPosition;
        
        float t = clamp(dot(toLight, rayDirection), 0.0, rayLength);
        float3 offset = _cameraPosition + rayDirection * t - lightPosition.xyz;
        
        float distanceSqr = dot(offset, offset);
        float rangeSqr = lightPosition.w * lightPosition.w;

        float attenuation = saturate(1.0 - distanceSqr / rangeSqr);
        attenuation *= attenuation;

        result += _ENVIRONMENT_LIGHT_COLOR[i].rgb * attenuation;
    }

    return result;
}
// ENVIRONMENT END

// FOG START
#define _ENVIRONMENT_TIME_FOG _ENVIRONMENT_TIME.x
uniform float3 _FOG_COLOR = float3(0.4, 0.4, 0.4);
uniform float _FOG_DENSITY = 1;
uniform float _FOG_OPACITY = 1;
uniform float _FOG_DISTANCE_START = 64;
uniform float _FOG_DISTANCE_FALLOFF = 1;
uniform float _FOG_HEIGHT_START = 64;
uniform float _FOG_HEIGHT_FALLOFF = 1;
uniform float _FOG_SUN_INFLUENCE = 1;
uniform float _FOG_SUN_SCATTERING = 1;
uniform sampler2D _FOG_NOISE_TEX;
uniform float _FOG_NOISE_OPACITY = 1;
uniform float _FOG_NOISE_FADE = 1;
uniform float _FOG_NOISE_SCALE = 0.05;
uniform sampler2D _FOG_RAMP_TEX;
uniform float _FOG_RAMP_OPACITY;
uniform float _FOG_RAMP_SCALE;
// FOG END

// SKY START
uniform float3 _ZENITH_COLOR = float3(0.75, 0.86, 0.75);
uniform float3 _HORIZON_COLOR = float3(0.4, 0.4, 0.4);
uniform float _HORIZON_THICKNESS = 0.15;
uniform float _HORIZON_SOFTNESS = 0.25;
uniform float3 _SUN_COLOR = float3(1, 1, 1);
uniform float3 _SUN_DIRECTION = float3(0, -1, 0);
uniform float _SUN_SIZE = 0.02;
uniform float _SUN_GLOW = 32;
uniform float _SUN_POWER = 1;
uniform float3 _MOON_COLOR = float3(1, 1, 1);
uniform float3 _MOON_DIRECTION = float3(0, 1, 0);
uniform float _MOON_SIZE = 0.02;
uniform float _MOON_GLOW = 32;
uniform float _MOON_POWER = 1;
// SKY END

// CLOUDS START
#define _ENVIRONMENT_TIME_CLOUD _ENVIRONMENT_TIME.y
uniform sampler2D _CLOUD_BASE_TEX;          // R -> Mask, G -> +Distortion, B -> -Distortion
uniform sampler2D _CLOUD_TARGET_TEX;        // R -> Mask, G -> +Distortion, B -> -Distortion
uniform float _CLOUD_BLEND = 0;
uniform float3 _CLOUD_COLOR = float3(1, 1, 1);
uniform float _CLOUD_COVERAGE = 1;
uniform float _CLOUD_OPACITY = 1;
uniform float _CLOUD_FADE = 0;
uniform float _CLOUD_HEIGHT = 8;
uniform float _CLOUD_CURVE = 128;
uniform float _CLOUD_SCALE = 2;
uniform float _CLOUD_TURBULENCE = 0.4;
uniform float _CLOUD_TRANSMITTANCE = 0.5;
uniform float _CLOUD_DARKNESS = 0.5;
uniform float _CLOUD_RIM_WIDTH = 0.05;
uniform float _CLOUD_RIM_STRENGTH = 5;
// CLOUDS END

// STARS START
#define _ENVIRONMENT_TIME_STAR _ENVIRONMENT_TIME.z
uniform float3 _STAR_COLOR = float3(1, 1, 1);
uniform float _STAR_AMOUNT = 1;
uniform float _STAR_DENSITY = 256;
uniform float _STAR_SIZE = 0.05;
uniform float _STAR_BRIGHTNESS = 0.05;
uniform float _STAR_ANIMATION = 0.25;
// STARS END

#endif