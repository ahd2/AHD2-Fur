#ifndef FUR_PASS_INCLUDED
#define FUR_PASS_INCLUDED
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
    float3 normalWS : TEXCOORD1;
    float3 posWS : TEXCOORD2;
    half4 viewDirWS : TEXCOORD3;//a通道存NdotL
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f FurVertex (appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    v.vertex.xyz += UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Offset) * v.normalOS;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.posWS = TransformObjectToWorld(v.vertex);
    o.viewDirWS.xyz = GetWorldSpaceNormalizeViewDir(o.posWS);
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    o.uv = TRANSFORM_TEX(v.uv, _FurMask);
    Light mainlight = GetMainLight();
    o.viewDirWS.a = max(dot(o.normalWS, mainlight.direction), 0);
    return o;
}

half4 FurFragment (v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    half4 furMask = tex2D(_FurMask, i.uv);//rg通道为flowmap，b通道为遮罩
    half4 noiseTex = tex2D(_NoiseTex, i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Offset) * -1 * (furMask.xy * 2 - 1) * _FurUVOffsetScale);
    half NdotL = i.viewDirWS.a;
    float2 normalToUV = normal2uv(i.normalWS);
    half3 Irradiance = tex2D(_IrradianceMap, normalToUV);
    clip(noiseTex * furMask.b - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Clip));
    half3 finalColor = tex2D(_MainTex, i.uv).xyz * _FurColor;//基础毛发色
    finalColor *= Irradiance * 0.5;//乘以环境光
    finalColor *= noiseTex * 0.5 + 0.5;//模拟AO(0.5 - 1)
    //finalColor.xyz *= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Gradient) * 0.5 + 0.5;
    finalColor += _lightColor.xyz * _lightColor.a * NdotL;//加上方向光颜色
    half fresnel = pow(1 - saturate(dot(normalize(i.viewDirWS), i.normalWS)), 5);
    finalColor += fresnel * finalColor * 0.5;
    //return NdotL;
    return half4(finalColor,1);
}
#endif