#ifndef HLSLDEPTH_INCLUDED
#define HLSLDEPTH_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#endif

float RawSceneDepth(float2 _uv)
{
#ifdef SHADERGRAPH_PREVIEW
    return 1.0;
#else
    return SampleSceneDepth(_uv);
#endif
}

float Linear01SceneDepth(float2 _uv)
{
#ifdef SHADERGRAPH_PREVIEW
    return 1.0;
#else
    return Linear01Depth(SampleSceneDepth(_uv), _ZBufferParams);
#endif
}

float EyeSceneDepth(float2 _uv)
{
#ifdef SHADERGRAPH_PREVIEW
    return 1.0;
#else
    return LinearEyeDepth(SampleSceneDepth(_uv), _ZBufferParams);
#endif
}

float3 WorldPositionFromRawDepth(float2 _uv)
{
#ifdef SHADERGRAPH_PREVIEW
    return float3(0.5, 0.5, 0.5);
#else   
    return ComputeWorldSpacePosition(_uv, SampleSceneDepth(_uv), UNITY_MATRIX_I_VP);
#endif
}

void GetWorldPositionFromRawDepth_float(float2 _uv, out float3 position)
{
    position = WorldPositionFromRawDepth(_uv);
}
void GetWorldPositionFromRawDepth_half(half2 _uv, out half3 position)
{
    GetWorldPositionFromRawDepth_float(_uv, position);
}

void GetRawSceneDepth_float(float2 _uv, out float depth)
{
    depth = RawSceneDepth(_uv);
}
void GetRawSceneDepth_half(half2 _uv, out half depth)
{
    GetRawSceneDepth_float(_uv, depth);
}

void GetLinear01SceneDepth_float(float2 _uv, out float depth)
{
    depth = Linear01SceneDepth(_uv);
}
void GetLinear01SceneDepth_half(half2 _uv, out half depth)
{
    GetLinear01SceneDepth_float(_uv, depth);
}

void GetEyeSceneDepth_float(float2 _uv, out float depth)
{
    depth = EyeSceneDepth(_uv);
}
void GetEyeSceneDepth_half(half2 _uv, out half depth)
{
    GetEyeSceneDepth_float(_uv, depth);
}

#endif