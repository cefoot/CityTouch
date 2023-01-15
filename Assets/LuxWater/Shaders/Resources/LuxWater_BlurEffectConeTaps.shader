Shader "Hidden/Lux Water/BlurEffectConeTap" 
{
	Properties { _MainTex ("", any) = "" {} }
	
	CGINCLUDE
		#include "UnityCG.cginc"

		struct appdata
		{
		    float4 vertex : POSITION;
		    float2 uv : TEXCOORD0;
		    UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 taps[4] : TEXCOORD1;
			half2 origuv : TEXCOORD5;
			//UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};
		
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		//sampler2D _MainTex;
		half4 _MainTex_TexelSize;
		half4 _BlurOffsets;

		sampler2D _UnderWaterTex;

		v2f vert( appdata v ) {
			v2f o;

			UNITY_SETUP_INSTANCE_ID(v);
    		UNITY_INITIALIZE_OUTPUT(v2f, o);
    		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv - _BlurOffsets.xy * _MainTex_TexelSize.xy;
			o.origuv = v.uv;	
			
			// #ifdef UNITY_SINGLE_PASS_STEREO
			// 	// we need to keep texel size correct after the uv adjustment.
			// 	o.taps[0] = UnityStereoScreenSpaceUVAdjust(o.uv + _MainTex_TexelSize * _BlurOffsets.xy * (1.0f / _MainTex_ST.xy), _MainTex_ST);
			// 	o.taps[1] = UnityStereoScreenSpaceUVAdjust(o.uv - _MainTex_TexelSize * _BlurOffsets.xy * (1.0f / _MainTex_ST.xy), _MainTex_ST);
			// 	o.taps[2] = UnityStereoScreenSpaceUVAdjust(o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTex_ST.xy), _MainTex_ST);
			// 	o.taps[3] = UnityStereoScreenSpaceUVAdjust(o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTex_ST.xy), _MainTex_ST);
			// #else
				o.taps[0] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy;
				o.taps[1] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy;
				o.taps[2] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);
				o.taps[3] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);
			//#endif
			return o;
		}

		half4 frag(v2f i) : SV_Target {

			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			half4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.taps[0]);
			color += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.taps[1]);
			color += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.taps[2]);
			color += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.taps[3]);
			return color * 0.25;
		}
	ENDCG

	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.5
			ENDCG
		}
	}
	Fallback off
}
