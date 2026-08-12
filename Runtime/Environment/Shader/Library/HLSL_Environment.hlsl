#ifndef HLSLENVIRONMENT_INCLUDED
#define HLSLENVIRONMENT_INCLUDED

// FOG START
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

uniform float3 _MOON_COLOR = float3(1, 1, 1);
uniform float3 _MOON_DIRECTION = float3(0, 1, 0);
uniform float _MOON_SIZE = 0.02;
uniform float _MOON_GLOW = 32;
// SKY END

// CLOUDS START
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
// CLOUDS END

#endif