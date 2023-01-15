Shader "HaptX/TexturedEmission" {
	Properties {
		_AlbedoTex ("Albedo (RGB)", 2D) = "white" {}
    _EmissionTex ("Emission (RGB)", 2D) = "black" {}
    _EmissionIntensity ("Emission Intensity", Float) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _AlbedoTex;
    
    sampler2D _EmissionTex;

		struct Input {
			float2 uv_AlbedoTex;
      float2 uv_EmissionTex;
		};

		half _Glossiness;
		half _Metallic;
    half _EmissionIntensity;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 ac = tex2D (_AlbedoTex, IN.uv_AlbedoTex);
      fixed4 ec = tex2D (_EmissionTex, IN.uv_EmissionTex);
			o.Albedo = ac.rgb;
      o.Emission = ec * _EmissionIntensity;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = ac.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
