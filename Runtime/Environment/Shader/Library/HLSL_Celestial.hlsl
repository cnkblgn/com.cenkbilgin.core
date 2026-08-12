#ifndef HLSLCELESTIAL_INCLUDED
#define HLSLCELESTIAL_INCLUDED

struct CelestialSettings
{
    float3 color;
    float direction;
    float glow;
    float size;
};

float3 GetCelestialDisc(float3 _color, float _size, float _direction)
{
    float disc = smoothstep(1 - _size, 1, _direction);
    return disc * _color;
}

float3 GetCelestialGlow(float3 _color, float _glow, float _direction)
{
    float glow = pow(_direction, _glow);
    return glow * _color;
}

float3 DrawCelestial(CelestialSettings settings)
{
    float3 disc = GetCelestialDisc(settings.color, settings.size, settings.direction);
    float3 glow = GetCelestialGlow(settings.color, settings.glow, settings.direction);
    
    return disc + glow;
}
#endif