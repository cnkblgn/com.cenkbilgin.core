#ifndef HLSLSTARS_INCLUDED
#define HLSLSTARS_INCLUDED

#include "../../../_Shared/Shaders/Library/HLSL_Helper.hlsl"

struct StarSettings
{
    float3 color;
    float amount;
    float density;
    float size;
    float brightness;
    float twinkleAmount;
    float twinkleSpeed;
};

float3 DrawStars(float3 _viewDirection, StarSettings settings)
{
    float t = _Time.y;
    float3 p = _viewDirection * settings.density;
    float3 c = floor(p);
    float3 f = frac(p);

    float3 position = lerp(0.3, 0.7, Hash33(c));
    float distance = length(f - position);
    float fade = smoothstep(0.0, 0.2, -_viewDirection.y);

    float random = Hash31(c + 17.31);
    float visible = step(1.0 - settings.amount, random);

    float star = smoothstep(settings.size, 0.0, distance);
    star *= star;

    float twinkle = 1.0 - settings.twinkleAmount * (0.5 + 0.5 * sin(t * settings.twinkleSpeed + random * 6.2831));

    float magnitude = lerp(0.3, 1.0, Hash31(c + 5.7));

    return settings.color * settings.brightness * star * visible * twinkle * magnitude * fade;
}
#endif