Shader "Hidden/Lux Water/WaterMask" {
	Properties {
		[Header(Tessellation)]
		_LuxWater_EdgeLength 			("Edge Length", Range(4, 100)) = 50
		_LuxWaterMask_Extrusion 	    ("Extrusion", Float) = 0.1
		_LuxWater_Phong 				("Phong Strengh", Range(0,1)) = 0.5

		_LuxWater_MeshScale				("MeshScale", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

//	Pass 0: Box volume
		Pass
		{
			ZTest Less
		//	When inside we have to flip culling
			Cull Front
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile __ GERSTNERENABLED
				#define USINGWATERVOLUME
				
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity;
					float _GerstnerNormalIntensity;
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif
				
				v2f vert (appdata v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                	float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

					#if defined(GERSTNERENABLED)
						if(v.color.r > 0) {
							_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;

							half3 vtxForAni = (wpos).xzz;
							half3 offsets;
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,							// offsets
								_LuxWaterMask_GAmplitude,									// amplitude
								_LuxWaterMask_GFinalFrequency,								// frequency
								_LuxWaterMask_GSteepness,									// steepness
								_LuxWaterMask_GFinalSpeed,									// speed
								_LuxWaterMask_GDirectionAB,									// direction # 1, 2
								_LuxWaterMask_GDirectionCD									// direction # 3, 4
							);
							wpos.xyz += offsets * v.color.r;

							if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
								half3 toffsets = offsets;
								vtxForAni = wpos.xzz; 
								GerstnerOffsetOnly (
									offsets, v.vertex.xyz, vtxForAni,
									_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
									_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
									_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
									_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
									_LuxWaterMask_GDirectionAB.zwxy,
									_LuxWaterMask_GDirectionCD.zwxy
								);
								wpos.xyz += offsets * v.color.r;
							}
						}
					#endif
				//	From WS to CS
					o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
					fixed4 col = half4(0,1,0,1);
					return col;
				}
			ENDCG
		}

//	Pass 1: Water surface front and back side
//	Active water volume only!
		pass
		{
			ZTest LEqual
			Cull Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#pragma multi_compile __ GERSTNERENABLED
				#pragma multi_compile __ USINGWATERPROJECTORS
				#define USINGWATERVOLUME

				#include "UnityCG.cginc"

				#if defined(USINGWATERPROJECTORS)
					UNITY_DECLARE_SCREENSPACE_TEXTURE(_LuxWater_NormalOverlay);
					float _LuxWaterMask_Extrusion;
				#endif
				
				struct appdata
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					//float2 texcoord : TEXCOORD0;
                	UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float depth : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity;
					float _GerstnerNormalIntensity;
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif
				
				v2f vert (appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                	float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                	float4 origWpos = wpos;

					#if defined(GERSTNERENABLED)
						_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;

						half3 vtxForAni = wpos.xzz;
						half3 offsets;
						GerstnerOffsetOnly (
							offsets, v.vertex.xyz, vtxForAni,							// offsets
							_LuxWaterMask_GAmplitude,									// amplitude
							_LuxWaterMask_GFinalFrequency,								// frequency
							_LuxWaterMask_GSteepness,									// steepness
							_LuxWaterMask_GFinalSpeed,									// speed
							_LuxWaterMask_GDirectionAB,									// direction # 1, 2
							_LuxWaterMask_GDirectionCD									// direction # 3, 4
						);
						wpos.xyz += offsets * v.color.r;

						if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
							vtxForAni = wpos.xzz; 
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,
								_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
								_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
								_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
								_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
								_LuxWaterMask_GDirectionAB.zwxy,
								_LuxWaterMask_GDirectionCD.zwxy
							);
							wpos.xyz += offsets * v.color.r;
						}
					#endif

				//  Projector Displacment
			        #if defined(USINGWATERPROJECTORS)
			            if(_LuxWaterMask_Extrusion > 0) {

			            	float4 hposOrig = mul(UNITY_MATRIX_VP, origWpos);
			            	float4 projectorScreenPos = ComputeScreenPos(hposOrig);
			                float2 projectionUVs = projectorScreenPos.xy / projectorScreenPos.w;

							#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
							    fixed4 projectedNormal = UNITY_SAMPLE_TEX2DARRAY_LOD(_LuxWater_NormalOverlay, float3(projectionUVs, (float)unity_StereoEyeIndex), 0);
							#else
							    fixed4 projectedNormal = tex2Dlod(_LuxWater_NormalOverlay, float4(projectionUVs, 0, 0));
							#endif 
			                
			                // We do not have a normal here like in the regular shader. So we skip the normal.
			                // wpos.xyz += v.normal * (projectedNormal.b) * _LuxWaterMask_Extrusion;
			                wpos.y += projectedNormal.b * _LuxWaterMask_Extrusion;
			            }
			        #endif
				
				//	From WS to CS
					o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
					//o.depth = COMPUTE_DEPTH_01;
					o.depth = -(mul(UNITY_MATRIX_V, float4(wpos.xyz, 1.0f)).z * _ProjectionParams.w);
					return o;
				}
				
				fixed4 frag (v2f i, float facing : VFACE) : SV_Target {
					
					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					#if UNITY_VFACE_FLIPPED
						facing = -facing;
					#endif
					#if UNITY_VFACE_AFFECTED_BY_PROJECTION
						facing *= _ProjectionParams.x; // take possible upside down rendering into account
				  	#endif

				//  Metal has inversed facingSign which is not handled by Unity? if culling is set to Off
					#if defined(SHADER_API_METAL) && UNITY_VERSION < 201710
						facing *= -1;
					#endif

				  	fixed2 upsidedown = (facing > 0) ? fixed2(1, 0) : fixed2(0, 0.5);

					fixed2 depth = EncodeFloatRG(i.depth);
					fixed4 col = fixed4(upsidedown, depth.x, depth.y);
					return col;
				}
			ENDCG
		}

//	Pass 2: Water surface front side only.
		pass {
			Ztest LEqual
			Cull Back
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile __ GERSTNERENABLED
				#define USINGWATERVOLUME
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float depth : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity; // dummy
					float _GerstnerNormalIntensity;	 // dummy
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif
				
				v2f vert (appdata v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                	float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

					#if defined(GERSTNERENABLED)
						_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;
						half3 vtxForAni = wpos.xzz;
						half3 offsets;
						GerstnerOffsetOnly (
							offsets, v.vertex.xyz, vtxForAni,							// offsets
							_LuxWaterMask_GAmplitude,									// amplitude
							_LuxWaterMask_GFinalFrequency,								// frequency
							_LuxWaterMask_GSteepness,									// steepness
							_LuxWaterMask_GFinalSpeed,									// speed
							_LuxWaterMask_GDirectionAB,									// direction # 1, 2
							_LuxWaterMask_GDirectionCD									// direction # 3, 4
						);
						wpos.xyz += offsets * v.color.r;

						if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
							vtxForAni = wpos.xzz; 
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,
								_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
								_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
								_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
								_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
								_LuxWaterMask_GDirectionAB.zwxy,
								_LuxWaterMask_GDirectionCD.zwxy
							);
							wpos.xyz += offsets * v.color.r;
						}
					#endif

				//	From WS to CS
					o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
					//o.depth = COMPUTE_DEPTH_01;
					o.depth = -(mul(UNITY_MATRIX_V, float4(wpos.xyz, 1.0f)).z * _ProjectionParams.w);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target {
					
					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					fixed2 depth = EncodeFloatRG(i.depth);
					fixed4 col = fixed4(1, 0, depth.x, depth.y);
					return col;
				}
			ENDCG
		}


//	Tessellation

//	Pass 3: Water surface front and back side
//	Active water volume only!
		pass
		{
			ZTest LEqual
			Cull Off
			CGPROGRAM
				#pragma target 4.6
				
				#pragma hull hs_surf
				#pragma domain ds_surf
				#pragma vertex tessvert
				#pragma fragment frag

				#pragma multi_compile __ GERSTNERENABLED
				#pragma multi_compile __ USINGWATERPROJECTORS
				#define USINGWATERVOLUME

				#include "UnityCG.cginc"

				#if defined(USINGWATERPROJECTORS)
					UNITY_DECLARE_SCREENSPACE_TEXTURE(_LuxWater_NormalOverlay);
					float _LuxWaterMask_Extrusion;
				#endif
				
				struct appdata_water
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float depth : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
//float3 normal  : TEXCOORD1;
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity;
					float _GerstnerNormalIntensity;
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif

				v2f vert (appdata_water v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                	//float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                //	As the tessellator returns v.vertex in WS
                	float4 wpos = v.vertex;
                	float4 origWpos = wpos;

					#if defined(GERSTNERENABLED)
					//  Reduce cracks
						wpos = floor(wpos * 1000.0f) * 0.001f;
						_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;

						half3 vtxForAni = (wpos).xzz;
						half3 offsets;
						GerstnerOffsetOnly (
							offsets, v.vertex.xyz, vtxForAni,							// offsets
							_LuxWaterMask_GAmplitude,									// amplitude
							_LuxWaterMask_GFinalFrequency,								// frequency
							_LuxWaterMask_GSteepness,									// steepness
							_LuxWaterMask_GFinalSpeed,									// speed
							_LuxWaterMask_GDirectionAB,									// direction # 1, 2
							_LuxWaterMask_GDirectionCD									// direction # 3, 4
						);
						wpos.xyz += offsets * v.color.r;
						if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
							vtxForAni = wpos.xzz; 
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,
								_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
								_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
								_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
								_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
								_LuxWaterMask_GDirectionAB.zwxy,
								_LuxWaterMask_GDirectionCD.zwxy
							);
							wpos.xyz += offsets * v.color.r;
						}
					#endif

			    //  Projector Displacment
			        #if defined(USINGWATERPROJECTORS)
			            if(_LuxWaterMask_Extrusion > 0) {

			            	float4 hposOrig = mul(UNITY_MATRIX_VP, origWpos);
			            	float4 projectorScreenPos = ComputeScreenPos(hposOrig);
			                float2 projectionUVs = projectorScreenPos.xy / projectorScreenPos.w;

							#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
							    fixed4 projectedNormal = UNITY_SAMPLE_TEX2DARRAY_LOD(_LuxWater_NormalOverlay, float3(projectionUVs, (float)unity_StereoEyeIndex), 0);
							#else
							    fixed4 projectedNormal = tex2Dlod(_LuxWater_NormalOverlay, float4(projectionUVs, 0, 0));
							#endif 
			                
			                // We do not have a normal here like in the regular shader. So we skip the normal.
			                // wpos.xyz += v.normal * (projectedNormal.b) * _LuxWaterMask_Extrusion;
			                wpos.y += projectedNormal.b * _LuxWaterMask_Extrusion;
			            }
			        #endif

			    //	From WS to CS
			        o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
			    //	This is object to viewpos!
					//o.depth = COMPUTE_DEPTH_01;
					o.depth = -(mul(UNITY_MATRIX_V, float4(wpos.xyz, 1.0f)).z * _ProjectionParams.w);

					return o;
				}

			//	As vert is needed by LuxWater_Tess.cginc we include it after the definition
				#define ISWATERVOLUME
				#include "../Includes/LuxWater_Tess.cginc"
				
				fixed4 frag (v2f i, float facing : VFACE) : SV_Target {

					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
					
					#if UNITY_VFACE_FLIPPED
						facing = -facing;
					#endif
					#if UNITY_VFACE_AFFECTED_BY_PROJECTION
						facing *= _ProjectionParams.x; // take possible upside down rendering into account
				  	#endif

				//  Metal has inversed facingSign which is not handled by Unity? if culling is set to Off
					#if defined(SHADER_API_METAL) && UNITY_VERSION < 201710
						facing *= -1;
					#endif

				  	fixed2 upsidedown = (facing > 0) ? fixed2(1, 0) : fixed2(0, 0.5);

					fixed2 depth = EncodeFloatRG(i.depth);
					fixed4 col = fixed4(upsidedown, depth.x, depth.y);
					return col;
				}
			ENDCG
		}

//	Pass 4: Water surface front side only
		pass
		{
			ZTest LEqual
			Cull Back
//ZWrite Off

			CGPROGRAM
				#pragma target 4.6
				
				#pragma hull hs_surf
				#pragma domain ds_surf
				#pragma vertex tessvert
				#pragma fragment frag

				#pragma multi_compile __ GERSTNERENABLED
				#define USINGWATERVOLUME

				#include "UnityCG.cginc"
				
				struct appdata_water
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float depth : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity;
					float _GerstnerNormalIntensity;
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif

				v2f vert (appdata_water v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                //	wpos comes in v.vertex from the tessellator
                	float4 wpos = v.vertex;

					#if defined(GERSTNERENABLED)
						//float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
					//  Reduce cracks
						wpos = floor(wpos * 1000.0f) * 0.001f;
						_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;

						half3 vtxForAni = (wpos).xzz;
						half3 offsets;
						GerstnerOffsetOnly (
							offsets, v.vertex.xyz, vtxForAni,							// offsets
							_LuxWaterMask_GAmplitude,									// amplitude
							_LuxWaterMask_GFinalFrequency,								// frequency
							_LuxWaterMask_GSteepness,									// steepness
							_LuxWaterMask_GFinalSpeed,									// speed
							_LuxWaterMask_GDirectionAB,									// direction # 1, 2
							_LuxWaterMask_GDirectionCD									// direction # 3, 4
						);
						wpos.xyz += offsets * v.color.r;
						if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
							vtxForAni = wpos.xzz; 
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,
								_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
								_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
								_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
								_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
								_LuxWaterMask_GDirectionAB.zwxy,
								_LuxWaterMask_GDirectionCD.zwxy
							);
							wpos.xyz += offsets * v.color.r;
						}
					#endif

					o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
					//o.depth = COMPUTE_DEPTH_01;
					o.depth = -(mul(UNITY_MATRIX_V, float4(wpos.xyz, 1.0f)).z * _ProjectionParams.w);
					return o;
				}

			//	As vert is needed by LuxWater_Tess.cginc we include it after the definition
				#define ISWATERVOLUME
				#include "../Includes/LuxWater_Tess.cginc"

				fixed4 frag (v2f i) : SV_Target {

					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					fixed2 depth = EncodeFloatRG(i.depth);
					fixed4 col = fixed4(1, 0, depth.x, depth.y);
					return col;
				}
			ENDCG
		}

//	Pass 5: Box volume tessellated as needed by sliding volumes
		Pass
		{
			ZTest Less
		//	When inside we have to flip culling
			Cull Front

//Zwrite Off
			
			CGPROGRAM
				#pragma target 4.6

				#pragma hull hs_surf
				#pragma domain ds_surf
				#pragma vertex tessvert
				#pragma fragment frag

				#pragma multi_compile __ GERSTNERENABLED
				#define USINGWATERVOLUME
				
				#include "UnityCG.cginc"

				struct appdata_water
				{
					float4 vertex : POSITION;
					//float3 normal : NORMAL;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
                	UNITY_VERTEX_OUTPUT_STEREO
				};

			//	Gerstner Waves
				#if defined(GERSTNERENABLED)
					float3 _GerstnerVertexIntensity;
					float _GerstnerNormalIntensity;
					uniform float3 _LuxWaterMask_GerstnerVertexIntensity;
				 	uniform float4 _LuxWaterMask_GAmplitude;
					uniform float4 _LuxWaterMask_GFinalFrequency;
					uniform float4 _LuxWaterMask_GSteepness;
					uniform float4 _LuxWaterMask_GFinalSpeed;
					uniform float4 _LuxWaterMask_GDirectionAB;
					uniform float4 _LuxWaterMask_GDirectionCD;
					uniform float4 _LuxWaterMask_GerstnerSecondaryWaves;
					#include "../Includes/LuxWater_GerstnerWaves.cginc"
				#endif
				
				v2f vert (appdata_water v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_INITIALIZE_OUTPUT(v2f, o);
                	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                	float4 wpos = v.vertex;

					#if defined(GERSTNERENABLED)
						//float4 wpos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
					//	Reduce cracks
						wpos = floor(wpos * 1000.0f) * 0.001f;
	
						_GerstnerVertexIntensity = _LuxWaterMask_GerstnerVertexIntensity;

						half3 vtxForAni = (wpos).xzz;
						half3 offsets;
						GerstnerOffsetOnly (
							offsets, v.vertex.xyz, vtxForAni,							// offsets
							_LuxWaterMask_GAmplitude,									// amplitude
							_LuxWaterMask_GFinalFrequency,								// frequency
							_LuxWaterMask_GSteepness,									// steepness
							_LuxWaterMask_GFinalSpeed,									// speed
							_LuxWaterMask_GDirectionAB,									// direction # 1, 2
							_LuxWaterMask_GDirectionCD									// direction # 3, 4
						);
						wpos.xyz += offsets * v.color.r;

						if(_LuxWaterMask_GerstnerSecondaryWaves.x > 0) {
							half3 toffsets = offsets;
							vtxForAni = wpos.xzz; 
							GerstnerOffsetOnly (
								offsets, v.vertex.xyz, vtxForAni,
								_LuxWaterMask_GAmplitude * _LuxWaterMask_GerstnerSecondaryWaves.x,
								_LuxWaterMask_GFinalFrequency * _LuxWaterMask_GerstnerSecondaryWaves.y,
								_LuxWaterMask_GSteepness * _LuxWaterMask_GerstnerSecondaryWaves.z,
								_LuxWaterMask_GFinalSpeed * _LuxWaterMask_GerstnerSecondaryWaves.w,
								_LuxWaterMask_GDirectionAB.zwxy,
								_LuxWaterMask_GDirectionCD.zwxy
							);
							wpos.xyz += offsets * v.color.r;
						}
					#endif

					o.vertex = mul(UNITY_MATRIX_VP, float4(wpos.xyz, 1.0f));
					return o;
				}

			//	As vert is needed by LuxWater_Tess.cginc we include it after the definition
				#define ISWATERVOLUME
				#include "../Includes/LuxWater_Tess.cginc"
				
				fixed4 frag (v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
                	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                	
					fixed4 col = half4(0,1,0,1);
					return col;
				}
			ENDCG
		}

	}
	Fallback "Hidden/Lux Water/WaterMaskNoTess"
}
