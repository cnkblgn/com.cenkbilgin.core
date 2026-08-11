#ifndef HLSLLIGHT_INCLUDED
#define HLSLLIGHT_INCLUDED

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

void GetMainLight_float(half3 _worldPosition, out half3 lightDirection, out half3 lightColor)
{
#ifdef SHADERGRAPH_PREVIEW
    lightDirection = normalize(half3(-0.7, 0.7, -0.7));
    lightColor = half3(1, 1, 1);
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
#endif
}
void GetMainLight_half(half3 _worldPosition, out half3 lightDirection, out half3 lightColor)
{
    GetMainLight_float(_worldPosition, lightDirection, lightColor);
}

void GetCustomLighting_float(in half3 _worldPosition, in half3 _worldNormal, in half2 _screenPosition, in half3 _ambientColor, in half3 _baseColor, out half3 color)
{
    half mainLightDiffuse;
    half3 mainLightDirection;
    half3 mainLightColor;
    
    GetMainLight_float(_worldPosition, mainLightDirection, mainLightColor);
    GetLambertDiffuse_float(_worldNormal, mainLightDirection, mainLightDiffuse);
      
    half diffuse = mainLightDiffuse;
    color = mainLightColor * diffuse;

#ifndef SHADERGRAPH_PREVIEW 
    uint pixelLightCount = GetAdditionalLightsCount();

#if USE_CLUSTER_LIGHT_LOOP
    InputData inputData = (InputData)0;
    inputData.normalizedScreenSpaceUV = _screenPosition;
    inputData.positionWS = _worldPosition;
#endif
    
    LIGHT_LOOP_BEGIN(pixelLightCount)
#if !USE_CLUSTER_LIGHT_LOOP
    lightIndex = GetPerObjectLightIndex(lightIndex);
#endif		
    Light light = GetAdditionalPerObjectLight(lightIndex, _worldPosition);
    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, _worldPosition, light.direction);

    half thisDiffuse;
    GetLambertDiffuse_float(_worldNormal, light.direction, thisDiffuse);
    thisDiffuse *= light.distanceAttenuation * light.shadowAttenuation;

#if defined(_LIGHT_COOKIES)
            float3 cookieColor = SampleAdditionalLightCookie(lightIndex, _worldPosition);
            light.color *= cookieColor;
#endif		
    
    diffuse += thisDiffuse;
    color += light.color * thisDiffuse;
    
    LIGHT_LOOP_END
    color = diffuse <= 0 ? mainLightColor : color / diffuse;
#endif
       
    color *= diffuse;
    color += _ambientColor;
    color *= _baseColor;
}
void GetCustomLighting_half(in half3 _worldPosition, in half3 _worldNormal, in half2 _screenPosition, in half3 _ambientColor, in half3 _baseColor, out half3 color)
{
    GetCustomLighting_float(_worldPosition, _worldNormal, _screenPosition, _ambientColor, _baseColor, color);
}
#endif