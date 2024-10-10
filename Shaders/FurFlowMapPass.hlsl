#ifndef FUR_FLOWMAP_PASS_INCLUDED
#define FUR_FLOWMAP_PASS_INCLUDED
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

v2f FurFlowMapVertex (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _FurMask);
    return o;
}

half4 FurFlowMapFragment (v2f i) : SV_Target
{
    half4 furMask = tex2D(_FurMask, i.uv);
    return half4(furMask.xy,0,1);
}
#endif