#ifndef HLSLLIGHT_INCLUDED
#define HLSLLIGHT_INCLUDED

// Eðer hedef shader, shadow sample edecek ve kullanacak ise bu keywordleri kullanmasý gerekiyor!
//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

void GetLambertDiffuse_float(half3 _worldNormal, half3 _lightDirection, out half diffuse)
{
    diffuse = saturate(dot(_worldNormal, _lightDirection));
}
void GetLambertDiffuse_half(half3 _worldNormal, half3 _lightDirection, out half diffuse)
{
    GetLambertDiffuse_float(_worldNormal, _lightDirection, diffuse);
}

void GetMainLight_float(out float3 lightDirection, out float3 lightColor)
{
#ifdef SHADERGRAPH_PREVIEW
    lightDirection = normalize(half3(-0.7, 0.7, -0.7));
    lightColor = half3(1, 1, 1);
#else
    Light light = GetMainLight();
    lightDirection = light.direction;
    lightColor = light.color;
#endif
}
void GetMainLight_half(out half3 lightDirection, out half3 lightColor)
{
    GetMainLight_float(lightDirection, lightColor);
}

void GetMainLight_float(half3 _worldPosition, out half3 lightDirection, out half3 lightColor, out half shadowAttenuation)
{
#ifdef SHADERGRAPH_PREVIEW
    lightDirection = normalize(half3(-0.7, 0.7, -0.7));
    lightColor = half3(1, 1, 1);
    shadowAttenuation = 1.0;
#else
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(_worldPosition);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(_worldPosition);
#endif
    Light light = GetMainLight(shadowCoord, _worldPosition, unity_ProbesOcclusion);
    lightDirection = light.direction;
    lightColor = light.color;
    shadowAttenuation = light.shadowAttenuation;
#endif
}
void GetMainLight_half(half3 _worldPosition, out half3 lightDirection, out half3 lightColor, out half shadowAttenuation)
{
    GetMainLight_float(_worldPosition, lightDirection, lightColor, shadowAttenuation);
}
#endif