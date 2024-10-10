#ifndef FUR_INPUT_INCLUDED
#define FUR_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(half, _Clip)
UNITY_DEFINE_INSTANCED_PROP(half, _Offset)
UNITY_DEFINE_INSTANCED_PROP(half, _Gradient)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

sampler2D _MainTex;
sampler2D _NoiseTex;
sampler2D _FurMask;
sampler2D _IrradianceMap;
float4 _MainTex_ST;
float4 _NoiseTex_ST;
float4 _FurMask_ST;
half4 _lightColor;//a通道为强度
half3 _FurColor;
half _FurUVOffsetScale;


float2 normal2uv(half3 normal)
{
    float2 result;
    result.y = 1 - acos(normal.y) / PI;
    result.x = (atan2(normal.z , normal.x)) / PI * 0.5 + 0.5;
    return result;
}
#endif