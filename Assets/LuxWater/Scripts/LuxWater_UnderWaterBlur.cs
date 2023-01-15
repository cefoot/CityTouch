using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace LuxWater {
	
	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderWaterBlur : MonoBehaviour {

		[Space(6)]
		[LuxWater_HelpBtn("h.3a2840a53u5j")]
		public float blurSpread = 0.6f;
		public int blurDownSample = 4;
		public int blurIterations = 4;

		private Vector2[] m_offsets	= new Vector2[4];

		private Material blurMaterial;
		private Material blitMaterial;

		private LuxWater_UnderWaterRendering waterrendermanager;
		private bool doBlur = false;

		private bool initBlur = true;

		private Camera cam;

		// Use this for initialization
		void OnEnable () {
			blurMaterial = new Material(Shader.Find("Hidden/Lux Water/BlurEffectConeTap"));
			blitMaterial = new Material(Shader.Find("Hidden/Lux Water/UnderWaterPost"));
			cam = GetComponent<Camera>();
			Invoke("GetWaterrendermanagerInstance", 0.0f); 
		}

		void OnDisable () {
			if (blurMaterial)
				DestroyImmediate(blurMaterial);
			if (blitMaterial)
				DestroyImmediate(blitMaterial);
		}

		void GetWaterrendermanagerInstance() {
			waterrendermanager = LuxWater_UnderWaterRendering.instance;
		}

		void OnRenderImage(RenderTexture src, RenderTexture dest) {

			if(waterrendermanager == null) {
				Graphics.Blit(src, dest);
				return;
			}

		//	XR support
			var xrSinglePassInstanced = false;
			var dim = TextureDimension.Tex2D;
			var depth = 1;
			var vrUsage = VRTextureUsage.None;
			if (cam.stereoEnabled) {
				vrUsage = VRTextureUsage.OneEye;
				if( XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced) {
					xrSinglePassInstanced = true;
					dim = TextureDimension.Tex2DArray;
					depth = 2;
					vrUsage = VRTextureUsage.TwoEyes;
				}
			}
			


		//	Only blur if any waterVolume is active
			doBlur = (waterrendermanager.activeWaterVolume > -1) ? true : false;
		//	We also call it once at start in order to reduce spikes
			if (doBlur || initBlur ) {
				initBlur = false;
			//	Downsample and blur UnderwaterTex
				int rtW = src.width / blurDownSample;
				int rtH = src.height / blurDownSample;

				RenderTextureDescriptor descr = new	RenderTextureDescriptor(rtW, rtH, RenderTextureFormat.DefaultHDR, 0);
				descr.dimension = dim;
				descr.volumeDepth = depth;
			//	Needed to make blit write into both slices of the array!
				descr.vrUsage = vrUsage;
				RenderTexture BlurBuffer = RenderTexture.GetTemporary(descr);

			// 	Copy screen to the smaller texture
				DownSample(src, BlurBuffer);
			//	Blur the small texture
				for (int i = 0; i < blurIterations; i++) {
					RenderTexture BlurBuffer2 = RenderTexture.GetTemporary(descr);
					FourTapCone(BlurBuffer, BlurBuffer2, i);
					RenderTexture.ReleaseTemporary(BlurBuffer);
					BlurBuffer = BlurBuffer2;
				}
				Shader.SetGlobalTexture("_UnderWaterTex", BlurBuffer);
			
			//	Combine Screen Buffer and BlurBuffer based on the Underwatermask
				Graphics.Blit(src, dest, blitMaterial, 1);
				RenderTexture.ReleaseTemporary(BlurBuffer);
			}
			else {
				Graphics.Blit(src, dest);
			}
		}


	//	////////////////////////////////////
	//	Helper functions
		
	//	Blur
		void FourTapCone (RenderTexture source, RenderTexture dest, int iteration) {
			float offset = 0.5f + iteration * blurSpread;
	        m_offsets[0].x = -offset;
	        m_offsets[0].y = -offset;
	        m_offsets[1].x = -offset;
	        m_offsets[1].y = offset;
	        m_offsets[2].x = offset;
	        m_offsets[2].y = offset;
	        m_offsets[3].x = offset;
	        m_offsets[3].y = -offset;
	        if (iteration == 0)
	        	Graphics.BlitMultiTap(source, dest, blurMaterial, m_offsets);
	        else
	        	Graphics.BlitMultiTap(source, dest, blurMaterial, m_offsets);
		}
		
	//	Downsampling
		void DownSample(RenderTexture source, RenderTexture dest) {
			float offset = 1.0f;
	        m_offsets[0].x = -offset;
	        m_offsets[0].y = -offset;
	        m_offsets[1].x = -offset;
	        m_offsets[1].y = offset;
	        m_offsets[2].x = offset;
	        m_offsets[2].y = offset;
	        m_offsets[3].x = offset;
	        m_offsets[3].y = -offset;
			Graphics.BlitMultiTap (source, dest, blurMaterial, m_offsets);
		}
	}
}