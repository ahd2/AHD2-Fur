Shader "Unlit/Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half3 viewDirWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 posWS = TransformObjectToWorld(v.vertex);
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(posWS);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half col = tex2D(_MainTex, i.uv);
                return abs(length(normalize(i.viewDirWS)) -1) * 10;
                return half4(i.viewDirWS,1);
            }
            ENDHLSL
        }
    }
}
