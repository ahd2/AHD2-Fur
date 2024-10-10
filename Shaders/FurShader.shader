Shader "Custom/Fur"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" { }
        _FurColor("FurColor", Color) = (1, 1, 1, 1)
        _FurMask("FurMask", 2D) = "white" { }
        _NoiseTex ("NoiseTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="AlphaTest"}
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex FurVertex
            #pragma fragment FurFragment
            #include "FurInput.hlsl"
            #include "FurPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FurMaskVertex
            #pragma fragment FurMaskFragment
            #include "FurInput.hlsl"
            #include "FurMaskPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FurFlowMapVertex
            #pragma fragment FurFlowMapFragment
            #include "FurInput.hlsl"
            #include "FurFlowMapPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            //暂时无用
            Name "FurShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            #pragma vertex FurShadowCasterVertex
            #pragma fragment FurShadowCasterFragment
            #include "FurInput.hlsl"
	        #include "FurShadowCasterPass.hlsl"
            ENDHLSL
        }
//        Pass
//        {
//            //Tags{ "LightMode" = "DepthNormals" }
//            ZWrite On
//            ColorMask 0
//            HLSLPROGRAM
//            // GPU Instancing
//            #pragma multi_compile_instancing
//            #pragma vertex FurDepthNormalsVertex
//            #pragma fragment FurDepthNormalsFragment
//            #include "FurInput.hlsl"
//	        #include "FurDepthNormalsPass.hlsl"
//            ENDHLSL
//        }
    }
}
