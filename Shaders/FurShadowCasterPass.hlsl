#ifndef FUR_SHADOW_CASTER_PASS_INCLUDED
#define FUR_SHADOW_CASTER_PASS_INCLUDED
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f FurShadowCasterVertex (appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    v.vertex.xyz += UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Offset) * v.normalOS;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _FurMask);
    return o;
}

half4 FurShadowCasterFragment (v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    half4 furMask = tex2D(_FurMask, i.uv);//rg通道为flowmap，b通道为遮罩
    half4 noiseTex = tex2D(_NoiseTex, i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Offset) * -1 * (furMask.xy * 2 - 1) * _FurUVOffsetScale);
    clip(noiseTex * furMask.b - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Clip));
    return 1;
}
#endif