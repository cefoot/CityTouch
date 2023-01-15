
Shader "Lux Water/Projectors/Foam Projector" {
Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
    
    [Header(Blending)]
    [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("    SrcFactor", Float) = 5
    [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("    DstFactor", Float) = 1

    [Space(5)]
    _Opacity("Opacity", Range(0,1)) = 1
    _MainTex ("Mask (R)", 2D) = "white" {}

    [Space(5)]
    [KeywordEnum(Simple, Foam)] _Overlay ("Overlay mode", Float) = 0
}

Category {
    Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    ZTest [_ZTest]
    ZWrite Off
    Cull [_Culling]
    Blend [_BlendSrc] [_BlendDst]
    ColorMask RGB
    Lighting Off 

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        //  Because of Instancing
            #pragma target 3.5

            #pragma multi_compile _OVERLAY_SIMPLE _OVERLAY_FOAM

            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed _Opacity;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target {

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed col = i.color.a * tex2D(_MainTex, i.texcoord).r * _Opacity;
                #if defined(_OVERLAY_FOAM)
                    return fixed4(col.r, 0, 0, col.r );
                #else
                    return fixed4(0, col.r, 0, col.r );
                #endif
            }
            ENDCG
        }
    }
}
}
