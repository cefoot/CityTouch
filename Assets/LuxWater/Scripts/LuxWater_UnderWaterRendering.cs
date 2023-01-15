using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace LuxWater {

	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderWaterRendering : MonoBehaviour {

		public static LuxWater_UnderWaterRendering instance;

		[Space(6)]
		[LuxWater_HelpBtn("h.d0q6uguuxpy")]
		public Transform Sun;
		[Space(4)]
		public bool FindSunOnEnable = false;
		public string SunGoName = "";
		public string SunTagName = "";
		private Light SunLight;

		[Space(2)]
		[Header("Deep Water Lighting")]
		[Space(4)]
		public bool EnableDeepwaterLighting = false;
		public float DefaultWaterSurfacePosition = 0.0f;
		public float DirectionalLightingFadeRange = 64.0f;
		public float FogLightingFadeRange = 64.0f;

		[Space(2)]
		[Header("Advanced Deferred Fog")]
		[Space(4)]
		public bool EnableAdvancedDeferredFog = false;
		public float FogDepthShift = 1.0f;
		public float FogEdgeBlending = 0.125f;


		[Space(8)]
        [System.NonSerialized]
        public int activeWaterVolume = -1;

		[System.NonSerialized]
		public List<Camera> activeWaterVolumeCameras = new List<Camera>();

		[System.NonSerialized]
        public float activeGridSize = 0.0f;

		[System.NonSerialized]
		public float WaterSurfacePos = 0.0f;

		[Space(8)]
		[System.NonSerialized]
		public List<int> RegisteredWaterVolumesIDs = new List<int>();
		[System.NonSerialized]
		public List<LuxWater_WaterVolume> RegisteredWaterVolumes = new List<LuxWater_WaterVolume>();
		private List<Mesh> WaterMeshes = new List<Mesh>();
		private List<Transform> WaterTransforms = new List<Transform>();
		private List<Material> WaterMaterials = new List<Material>();
		private List<bool> WaterIsOnScreen = new List<bool>();
		private List<bool> WaterUsesSlidingVolume = new List<bool>();

		private RenderTexture UnderWaterMask;

		[Space(2)]
		[Header("Managed transparent Materials")]
		[Space(4)]
		public List<Material> m_aboveWatersurface = new List<Material>();
		public List<Material> m_belowWatersurface = new List<Material>();

		[Space(2)]
		[Header("Optimize")]
		[Space(4)]
		public ShaderVariantCollection PrewarmedShaders;
		public int ListCapacity = 10;
		
		[Space(2)]
		[Header("Debug")]
		[Space(4)]
		public bool enableDebug = false;
		[Space(8)]

		private Material mat;
		private Material blurMaterial;
		private Material blitMaterial;

		//private RenderTexture UnderwaterTex;
		
		private Camera cam;
		private bool UnderwaterIsSetUp = false;

		private static CommandBuffer cb_Mask;
		private CameraEvent cameraEvent = CameraEvent.AfterSkybox; //.BeforeSkybox; // This works for both deferred an forward

		private Transform camTransform;
		private Matrix4x4 frustumCornersArray = Matrix4x4.identity;
		private Matrix4x4 frustumCornersArray2nd = Matrix4x4.identity;

		private SphericalHarmonicsL2 ambientProbe;
		private Vector3[] directions = new Vector3[] { new Vector3(0.0f, 1.0f, 0.0f) };
		private Color[] AmbientLightingSamples = new Color[1];


	// Metal Support
	// We have to manually grab the depth texture
		//private CommandBuffer cb_DepthGrab;
		//private CommandBuffer cb_AfterFinalPass;
		
		private bool DoUnderWaterRendering = false;
		private Matrix4x4 camProj;
		private Vector3[] frustumCorners = new Vector3[4];

		private float Projection;
		private bool islinear = false;

		private Matrix4x4 WatervolumeMatrix;


		private static readonly int UnderWaterMaskPID = Shader.PropertyToID("_UnderWaterMask");
		private static readonly int Lux_FrustumCornersWSPID = Shader.PropertyToID("_Lux_FrustumCornersWS");
		private static readonly int Lux_FrustumCornersWS2ndPID = Shader.PropertyToID("_Lux_FrustumCornersWS2ndEye");
		private static readonly int Lux_CameraWSPID = Shader.PropertyToID("_Lux_CameraWS");

		private static readonly int GerstnerEnabledPID = Shader.PropertyToID("_GerstnerEnabled");

		private static readonly int LuxWaterMask_GerstnerVertexIntensityPID = Shader.PropertyToID("_LuxWaterMask_GerstnerVertexIntensity");
		private static readonly int GerstnerVertexIntensityPID = Shader.PropertyToID("_GerstnerVertexIntensity");
		private static readonly int LuxWaterMask_GAmplitudePID = Shader.PropertyToID("_LuxWaterMask_GAmplitude");
		private static readonly int GAmplitudePID = Shader.PropertyToID("_GAmplitude");
		private static readonly int LuxWaterMask_GFinalFrequencyPID = Shader.PropertyToID("_LuxWaterMask_GFinalFrequency");
		private static readonly int GFinalFrequencyPID = Shader.PropertyToID("_GFinalFrequency");
		private static readonly int LuxWaterMask_GSteepnessPID = Shader.PropertyToID("_LuxWaterMask_GSteepness");
		private static readonly int GSteepnessPID = Shader.PropertyToID("_GSteepness");
		private static readonly int LuxWaterMask_GFinalSpeedPID = Shader.PropertyToID("_LuxWaterMask_GFinalSpeed");
		private static readonly int GFinalSpeedPID = Shader.PropertyToID("_GFinalSpeed");
		private static readonly int LuxWaterMask_GDirectionABPID = Shader.PropertyToID("_LuxWaterMask_GDirectionAB");
		private static readonly int GDirectionABPID = Shader.PropertyToID("_GDirectionAB");
		private static readonly int LuxWaterMask_GDirectionCDPID = Shader.PropertyToID("_LuxWaterMask_GDirectionCD");
		private static readonly int GDirectionCDPID = Shader.PropertyToID("_GDirectionCD");
		private static readonly int LuxWaterMask_GerstnerSecondaryWaves = Shader.PropertyToID("_LuxWaterMask_GerstnerSecondaryWaves");
		private static readonly int GerstnerSecondaryWaves = Shader.PropertyToID("_GerstnerSecondaryWaves");

		private static readonly int Lux_UnderWaterAmbientSkyLightPID = Shader.PropertyToID("_Lux_UnderWaterAmbientSkyLight");

		private static readonly int Lux_UnderWaterSunColorPID = Shader.PropertyToID("_Lux_UnderWaterSunColor");
		private static readonly int Lux_UnderWaterSunDirPID = Shader.PropertyToID("_Lux_UnderWaterSunDir");
		private static readonly int Lux_UnderWaterSunDirViewSpacePID = Shader.PropertyToID("_Lux_UnderWaterSunDirViewSpace");

		private static readonly int Lux_EdgeLengthPID = Shader.PropertyToID("_LuxWater_EdgeLength");
		//private static readonly int Lux_WaterMeshScalePID = Shader.PropertyToID("_LuxWater_MeshScale");
		private static readonly int Lux_ExtrusionPID = Shader.PropertyToID("_LuxWater_Extrusion");
		private static readonly int LuxMask_ExtrusionPID = Shader.PropertyToID("_LuxWaterMask_Extrusion");

		private static readonly int Lux_MaxDirLightDepthPID = Shader.PropertyToID("_MaxDirLightDepth");
		private static readonly int Lux_MaxFogLightDepthPID = Shader.PropertyToID("_MaxFogLightDepth");

		private static readonly int Lux_CausticsEnabledPID = Shader.PropertyToID("_CausticsEnabled");
		private static readonly int Lux_CausticModePID = Shader.PropertyToID("_CausticMode");
		private static readonly int Lux_UnderWaterFogColorPID = Shader.PropertyToID("_Lux_UnderWaterFogColor");
		private static readonly int Lux_UnderWaterFogDensityPID = Shader.PropertyToID("_Lux_UnderWaterFogDensity");
		private static readonly int Lux_UnderWaterFogAbsorptionCancellationPID = Shader.PropertyToID("_Lux_UnderWaterFogAbsorptionCancellation");
		private static readonly int Lux_UnderWaterAbsorptionHeightPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionHeight");
		private static readonly int Lux_UnderWaterAbsorptionMaxHeightPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionMaxHeight");
		private static readonly int Lux_UnderWaterAbsorptionDepthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionDepth");
		private static readonly int Lux_UnderWaterAbsorptionColorStrengthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionColorStrength");
		private static readonly int Lux_UnderWaterAbsorptionStrengthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionStrength");
		private static readonly int Lux_UnderWaterUnderwaterScatteringPowerPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringPower");
		private static readonly int Lux_UnderWaterUnderwaterScatteringIntensityPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringIntensity");
		private static readonly int Lux_UnderWaterUnderwaterScatteringColorPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringColor");
		private static readonly int Lux_UnderWaterUnderwaterScatteringOcclusionPID = Shader.PropertyToID("_Lux_UnderwaterScatteringOcclusion");
		private static readonly int Lux_UnderWaterCausticsPID = Shader.PropertyToID("_Lux_UnderWaterCaustics");

		private static readonly int Lux_UnderWaterDeferredFogParams = Shader.PropertyToID("_LuxUnderWaterDeferredFogParams");
		private static readonly int CausticTexPID = Shader.PropertyToID("_CausticTex");
		private static readonly int Lux_UnderWaterCausticsTilingPID = Shader.PropertyToID("_Lux_UnderWaterCausticsTiling");
		private static readonly int Lux_UnderWaterCausticsScalePID = Shader.PropertyToID("_Lux_UnderWaterCausticsScale");
		private static readonly int Lux_UnderWaterCausticsSpeedPID = Shader.PropertyToID("_Lux_UnderWaterCausticsSpeed");
		private static readonly int Lux_UnderWaterCausticsSelfDistortionPID = Shader.PropertyToID("_Lux_UnderWaterCausticsSelfDistortion");
		private static readonly int Lux_UnderWaterFinalBumpSpeed01PID = Shader.PropertyToID("_Lux_UnderWaterFinalBumpSpeed01");
		private static readonly int Lux_UnderWaterFogDepthAttenPID = Shader.PropertyToID("_Lux_UnderWaterFogDepthAtten");

		void OnEnable () {
			if(instance != null) {
				Destroy(this);
			}
			else {
				instance = this;
			}
		//	Set up materials	
			mat = new Material(Shader.Find("Hidden/Lux Water/WaterMask"));
			blurMaterial = new Material(Shader.Find("Hidden/Lux Water/BlurEffectConeTap"));
			blitMaterial = new Material(Shader.Find("Hidden/Lux Water/UnderWaterPost"));
		//	Get camera
			if(cam == null) {
				cam = GetComponent<Camera>();
			}
		//	Metal support – make sure the camera actually renders a depth texture
			cam.depthTextureMode |= DepthTextureMode.Depth;
			camTransform = cam.transform;
		//	Set up Commandbufer
			cb_Mask = new CommandBuffer();
			cb_Mask.name = "Lux Water: Underwater Mask";
			cam.AddCommandBuffer(cameraEvent, cb_Mask);
		//	Find the sun dynmically
			if (FindSunOnEnable) {
				if (SunGoName != "")
					Sun = GameObject.Find(SunGoName).transform;
				else if (SunTagName != "")
					Sun = GameObject.FindWithTag(SunTagName).transform;
			}
		//	Set Pojection (if flipped or not)
			if(SystemInfo.usesReversedZBuffer) {
				Projection = -1.0f; 
			}
			else {
				Projection = 1.0f;
			}
            islinear = (QualitySettings.desiredColorSpace == ColorSpace.Linear) ? true : false;
        //	Warm up needed shaders
            if (PrewarmedShaders != null) {
            	if(!PrewarmedShaders.isWarmedUp) {
            		PrewarmedShaders.WarmUp();
            	}
            }
        //	Get SunLight
            if(Sun != null) {
            	SunLight = Sun.GetComponent<Light>();
            }
        //	Set up Lists
            RegisteredWaterVolumesIDs.Capacity = ListCapacity;
            RegisteredWaterVolumes.Capacity = ListCapacity;
            WaterMeshes.Capacity = ListCapacity;
            WaterTransforms.Capacity = ListCapacity;
            WaterMaterials.Capacity = ListCapacity;
            WaterIsOnScreen.Capacity = ListCapacity;
            WaterUsesSlidingVolume.Capacity = ListCapacity;
            activeWaterVolumeCameras.Capacity = 2; // we consider 2 splitscreen cameras max
        //	Deep Water Lighting and advanced deferred Fog
            SetDeepwaterLighting ();
           	SetDeferredFogParams ();
		}

		void CleanUp () {
			instance = null;
			if(UnderWaterMask != null) {
				UnderWaterMask.Release();
				Destroy(UnderWaterMask);
			}
			if(mat)
				Destroy(mat);
			if(blurMaterial)
				Destroy(blurMaterial);
			if(blitMaterial)
				Destroy(blitMaterial);

			Shader.DisableKeyword("LUXWATER_DEEPWATERLIGHTING");
			Shader.DisableKeyword("LUXWATER_DEFERREDFOG");
			cam.RemoveCommandBuffer(cameraEvent, cb_Mask);
		}

		void OnDisable () {
			CleanUp ();
		}

	//	Is also called when the scene gets unloaded.
		void OnDestroy() {
			CleanUp ();
		}

		void OnValidate () {
			SetDeepwaterLighting ();
			SetDeferredFogParams ();
		}

		public void SetDeferredFogParams() {
			if(EnableAdvancedDeferredFog) {
            	Shader.EnableKeyword("LUXWATER_DEFERREDFOG");
				Vector4 fogParams = new Vector4( (DoUnderWaterRendering) ? 1 : 0, FogDepthShift, FogEdgeBlending, 0);
				Shader.SetGlobalVector(Lux_UnderWaterDeferredFogParams, fogParams);
			}
			else {
            	Shader.DisableKeyword("LUXWATER_DEFERREDFOG");
			}
		}

		public void SetDeepwaterLighting () {
			if(EnableDeepwaterLighting) {
				Shader.EnableKeyword("LUXWATER_DEEPWATERLIGHTING");
				if(activeWaterVolume != -1) {
					Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", WaterSurfacePos);
				}
				else {
					Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", DefaultWaterSurfacePosition);
				}
				Shader.SetGlobalFloat("_Lux_UnderWaterDirLightingDepth", DirectionalLightingFadeRange);
				Shader.SetGlobalFloat("_Lux_UnderWaterFogLightingDepth", FogLightingFadeRange);
			}
			else {
				Shader.DisableKeyword("LUXWATER_DEEPWATERLIGHTING");
			}
		}

		public void RegisterWaterVolume(LuxWater_WaterVolume item, int ID, bool visible, bool SlidingVolume) {
			RegisteredWaterVolumesIDs.Add(ID);
			RegisteredWaterVolumes.Add(item);
			WaterMeshes.Add(item.WaterVolumeMesh);
			WaterMaterials.Add(item.transform.GetComponent<Renderer>().sharedMaterial);
			WaterTransforms.Add(item.transform);
			WaterIsOnScreen.Add(visible);
			WaterUsesSlidingVolume.Add(SlidingVolume);
		//	Dummy: Trigger GetTexture() and SetGerstnerWaves() on register to prevent garbage in the OnRenderImage function.
			var i = WaterMaterials.Count - 1;
			Shader.SetGlobalTexture(Lux_UnderWaterCausticsPID, WaterMaterials[i].GetTexture(CausticTexPID) );
			SetGerstnerWaves(i);
			//mat.SetPass(0);
			//Graphics.DrawMeshNow( WaterMeshes[i], WaterTransforms[i].localToWorldMatrix, 0); // submesh 0 = Water box volume
		}

		public void DeRegisterWaterVolume(LuxWater_WaterVolume item, int ID) {
			int index = RegisteredWaterVolumesIDs.IndexOf(ID);
			if (activeWaterVolume == index) {
				activeWaterVolume = -1;
			}
			RegisteredWaterVolumesIDs.RemoveAt(index);
			RegisteredWaterVolumes.RemoveAt(index);
			WaterMeshes.RemoveAt(index);
			WaterMaterials.RemoveAt(index);
			WaterTransforms.RemoveAt(index);
			WaterIsOnScreen.RemoveAt(index);
			WaterUsesSlidingVolume.RemoveAt(index);
		}

		public void SetWaterVisible(int ID) {
			int index = RegisteredWaterVolumesIDs.IndexOf(ID);
			WaterIsOnScreen[index] = true;
		}
		public void SetWaterInvisible(int ID) {
			int index = RegisteredWaterVolumesIDs.IndexOf(ID);
			WaterIsOnScreen[index] = false;
		}

		public void EnteredWaterVolume (LuxWater_WaterVolume item, int ID, Camera triggerCam, float GridSize) {
			DoUnderWaterRendering = true;
			int index = RegisteredWaterVolumesIDs.IndexOf(ID);
			if(index != activeWaterVolume) {
				activeWaterVolume = index;
				activeGridSize = GridSize;
				
				WaterSurfacePos = WaterTransforms[activeWaterVolume].position.y;

			//	Update Transparents
				for (int i = 0; i < m_aboveWatersurface.Count; i++) {
					m_aboveWatersurface[i].renderQueue = 2997; // 2998
				}
				for (int i = 0; i < m_belowWatersurface.Count; i++) {
					m_belowWatersurface[i].renderQueue = 3001;
				}
			}

			if (!activeWaterVolumeCameras.Contains(triggerCam)) {
				activeWaterVolumeCameras.Add(triggerCam);
			}
		}

		public void LeftWaterVolume (LuxWater_WaterVolume item, int ID, Camera triggerCam) {
			DoUnderWaterRendering = false;
			int index = RegisteredWaterVolumesIDs.IndexOf(ID);
			if (activeWaterVolume == index) {
				activeWaterVolume = -1;
			//	Update Transparents
				for (int i = 0; i < m_aboveWatersurface.Count; i++) {
					m_aboveWatersurface[i].renderQueue = 3000;
				}
				for (int i = 0; i < m_belowWatersurface.Count; i++) {
					m_belowWatersurface[i].renderQueue = 2997; // 2998
				}
			}

			if (activeWaterVolumeCameras.Contains(triggerCam)) {
				activeWaterVolumeCameras.Remove(triggerCam);
			}
		}


	//	void OnPreCull () {
	//	Here we have to use OnPreRender to make XR Multipass work	
		void OnPreRender () {
			SetDeferredFogParams();
			RenderWaterMask( cam, false, cb_Mask );
		}

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture src, RenderTexture dest) {
			RenderUnderWater( src, dest, cam, false);
		}


	//	---------------------------------------------------------


		public void SetGerstnerWaves (int index) {
			if (WaterMaterials[index].GetFloat(GerstnerEnabledPID) == 1.0f) {
				mat.EnableKeyword("GERSTNERENABLED");
				mat.SetVector(LuxWaterMask_GerstnerVertexIntensityPID, WaterMaterials[index].GetVector(GerstnerVertexIntensityPID) );
				mat.SetVector(LuxWaterMask_GAmplitudePID, WaterMaterials[index].GetVector(GAmplitudePID) );
				mat.SetVector(LuxWaterMask_GFinalFrequencyPID, WaterMaterials[index].GetVector(GFinalFrequencyPID) );
				mat.SetVector(LuxWaterMask_GSteepnessPID, WaterMaterials[index].GetVector(GSteepnessPID) );
				mat.SetVector(LuxWaterMask_GFinalSpeedPID, WaterMaterials[index].GetVector(GFinalSpeedPID) );
				mat.SetVector(LuxWaterMask_GDirectionABPID, WaterMaterials[index].GetVector(GDirectionABPID) );
				mat.SetVector(LuxWaterMask_GDirectionCDPID, WaterMaterials[index].GetVector(GDirectionCDPID) );
				mat.SetVector(LuxWaterMask_GerstnerSecondaryWaves, WaterMaterials[index].GetVector(GerstnerSecondaryWaves) );
			}
			else {
				mat.DisableKeyword("GERSTNERENABLED");
			}
		}


		public void RenderWaterMask(Camera currentCamera, bool SecondaryCameraRendering, CommandBuffer cmb) {

		//	As we need _Time when drawing the Underwatermask (Gerstner Waves) we have to use a custom _Lux_Time as _Time will not be updated OnPreCull
			Shader.SetGlobalFloat("_Lux_Time", Time.timeSinceLevelLoad);

			currentCamera.depthTextureMode |= DepthTextureMode.Depth;
			camTransform = currentCamera.transform;

		//	XR support
			var xrSinglePassInstanced = false;
			var dim = TextureDimension.Tex2D;
			if (currentCamera.stereoEnabled) {
				if( XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced) {
					xrSinglePassInstanced = true;
					dim = TextureDimension.Tex2DArray;
				}
			}

/*		//	Check if the CommandBuffer already exists
			if (cb_DepthGrab == null) {
				//var commandBuffers = cam.GetCommandBuffers(CameraEvent.BeforeForwardAlpha);
				var commandBuffers = cam.GetCommandBuffers(CameraEvent.AfterSkybox); //AfterImageEffectsOpaque); // this is where deferred fog gets rendered
				for(int i = 0; i < commandBuffers.Length; i++) {
					if(commandBuffers[i].name == "Lux Water Grab Depth") {
						cb_DepthGrab = commandBuffers[i];
						break;
					}
				}
			}
			if (cb_DepthGrab == null) {
				cb_DepthGrab = new CommandBuffer();
				cb_DepthGrab.name = "Lux Water Grab Depth";
				cam.AddCommandBuffer(CameraEvent.AfterSkybox, cb_DepthGrab);
			}
			cb_DepthGrab.Clear();
			int depthGrabID = Shader.PropertyToID("_Lux_GrabbedDepth");
			cb_DepthGrab.GetTemporaryRT(depthGrabID, -1, -1, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
			cb_DepthGrab.Blit(BuiltinRenderTextureType.CameraTarget, depthGrabID);
*/

		//	Setup UnderWaterMask and SetRenderTarget upfront to prevent spikes.
			UnityEngine.Profiling.Profiler.BeginSample("Create UnderWaterMask Tex");
				if (!UnderWaterMask) {
					UnderWaterMask = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 24, RenderTextureFormat.ARGB32,  RenderTextureReadWrite.Linear);
					UnderWaterMask.dimension = dim;
					if(xrSinglePassInstanced) {
						UnderWaterMask.volumeDepth = 2;
					}
				}
			//	if SecondaryCameraRendering = true (Splitscreen) do not resize UnderWaterMask
			//	as the secondary camera might have a smaller frame which would be quite expensive...
				else if (UnderWaterMask.width != currentCamera.pixelWidth && !SecondaryCameraRendering) {
					DestroyImmediate(UnderWaterMask);
					UnderWaterMask = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 24, RenderTextureFormat.ARGB32,  RenderTextureReadWrite.Linear);
					UnderWaterMask.dimension = dim;
					if(xrSinglePassInstanced) {
						UnderWaterMask.volumeDepth = 2;
					}
				}

				Shader.SetGlobalTexture(UnderWaterMaskPID, UnderWaterMask);
				//Graphics.SetRenderTarget(UnderWaterMask);
			UnityEngine.Profiling.Profiler.EndSample();
			
		//	Set frustum corners – which are needed to reconstruct the world position in the underwater post shader.
		//	As this spikes as hell when a volume gets active we simply do it all the time.
	        UnityEngine.Profiling.Profiler.BeginSample("Set up Frustum Corners");
		        
				// https://forum.unity.com/threads/stereo-frustum-corners-seem-incorrect.588268/
		        currentCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), currentCamera.farClipPlane, currentCamera.stereoActiveEye, frustumCorners);
		        var bottomLeft = camTransform.TransformVector(frustumCorners[0]);
		        var topLeft = camTransform.TransformVector(frustumCorners[1]);
		        var topRight = camTransform.TransformVector(frustumCorners[2]);
		        var bottomRight = camTransform.TransformVector(frustumCorners[3]);

		        frustumCornersArray.SetRow(0, bottomLeft);
		        frustumCornersArray.SetRow(1, bottomRight);
		        frustumCornersArray.SetRow(2, topLeft);
		        frustumCornersArray.SetRow(3, topRight);

		        if (xrSinglePassInstanced) {
					currentCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), currentCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Right, frustumCorners);
					bottomLeft = camTransform.TransformVector(frustumCorners[0]);
		         	topLeft = camTransform.TransformVector(frustumCorners[1]);
		         	topRight = camTransform.TransformVector(frustumCorners[2]);
		         	bottomRight = camTransform.TransformVector(frustumCorners[3]);

		        	frustumCornersArray2nd.SetRow(0, bottomLeft);
		        	frustumCornersArray2nd.SetRow(1, bottomRight);
		        	frustumCornersArray2nd.SetRow(2, topLeft);
		        	frustumCornersArray2nd.SetRow(3, topRight);    	
		        }
	        UnityEngine.Profiling.Profiler.EndSample();

	    //	Set ambient lighting. Spikes so it is taken out of the if.
			UnityEngine.Profiling.Profiler.BeginSample("Set up ambient lighting");
		        ambientProbe = RenderSettings.ambientProbe;
		        ambientProbe.Evaluate(directions, AmbientLightingSamples);
	            if (islinear)
		            Shader.SetGlobalColor(Lux_UnderWaterAmbientSkyLightPID, (AmbientLightingSamples[0] * RenderSettings.ambientIntensity).linear);
	            else
	                Shader.SetGlobalColor(Lux_UnderWaterAmbientSkyLightPID, AmbientLightingSamples[0] * RenderSettings.ambientIntensity);
	    	UnityEngine.Profiling.Profiler.EndSample();

	    //	Do all heavy spiking stuff before this return!
	    //	Check if currentCamera is actually within the watervolume
			if (!activeWaterVolumeCameras.Contains(currentCamera) && !EnableAdvancedDeferredFog) {
				// This return breaks split screen cameras, tess, gerstners and the "sample mask to correct vface"
				// But actually we do not really need it. The script will never draw anything but just create a black mask: GL.Clear(true, true, Color.black, 1);
				// return;
			}

		//	Render UnderWaterMask
			if(activeWaterVolume > -1) {
			//	Tell all shaders that underwater rendering is active (like e.g. fog shaders)
				Shader.EnableKeyword("LUXWATERENABLED");
			//	In case deep water lighting is disabled we pick DirectionalLightingFadeRange from the water material
	            if(!EnableDeepwaterLighting) {
	            	Shader.SetGlobalFloat("_Lux_UnderWaterDirLightingDepth", WaterMaterials[activeWaterVolume].GetFloat(Lux_MaxDirLightDepthPID));
	            	Shader.SetGlobalFloat("_Lux_UnderWaterFogLightingDepth", WaterMaterials[activeWaterVolume].GetFloat(Lux_MaxFogLightDepthPID));
	            }
	            Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", WaterSurfacePos);
	        }
	        else {
				Shader.DisableKeyword("LUXWATERENABLED");
			}

			RenderTargetIdentifier target;
			if(xrSinglePassInstanced) {
				target = new RenderTargetIdentifier(UnderWaterMask, 0, CubemapFace.Unknown, -1); // slice = -1 makes it draw into both layers!
			}
			else {
				target = new RenderTargetIdentifier(UnderWaterMask);
			}

			cmb.Clear();
			cmb.SetRenderTarget(target);
			cmb.ClearRenderTarget(true, true, new Color(0,0,0,0), 1.0f);
			cmb.SetGlobalMatrix(Lux_FrustumCornersWSPID, frustumCornersArray);
			cmb.SetGlobalMatrix("_Lux_FrustumCornersWS2ndEye", frustumCornersArray2nd);
			cmb.SetGlobalVector(Lux_CameraWSPID, camTransform.position);

			//GL.PushMatrix();
			//GL.Clear(true, true, Color.black, 1);
			//camProj = currentCamera.projectionMatrix;
			//GL.LoadProjectionMatrix(camProj);

		//	These params might be odd (because of the scene camera or another camera rendering before). Needed by tessellation shader!
			Shader.SetGlobalVector("_WorldSpaceCameraPos", camTransform.position);
			// x is 1.0 (or –1.0 if currently rendering with a flipped projection matrix), y is the camera’s near plane, z is the camera’s far plane and w is 1/FarPlane.
			Shader.SetGlobalVector("_ProjectionParams", new Vector4( Projection, currentCamera.nearClipPlane, currentCamera.farClipPlane, 1.0f / currentCamera.farClipPlane ) );
			// x is the width of the camera’s target texture in pixels, y is the height of the camera’s target texture in pixels, z is 1.0 + 1.0/width and w is 1.0 + 1.0/height.
			Shader.SetGlobalVector("_ScreenParams", new Vector4(currentCamera.pixelWidth, currentCamera.pixelHeight, 1.0f + 1.0f / currentCamera.pixelWidth, 1.0f + 1.0f / currentCamera.pixelHeight ) );

			for (int i = 0; i < RegisteredWaterVolumes.Count; i++ ) {

				if( !WaterIsOnScreen[i] && i != activeWaterVolume) {
					continue;
				}
				if( !EnableAdvancedDeferredFog && i != activeWaterVolume) {
					continue;
				}

				WatervolumeMatrix = WaterTransforms[i].localToWorldMatrix;

			//	Handle sliding volumes
				if( WaterUsesSlidingVolume[i]) {
				//	Here we have to change the WatervolumeMatrix and center the volume on the camera
					var CamPos = camTransform.position;
				//	Extract local position. NOTE: We must explicitly define a float4 here! (no var)
					Vector4 position = WatervolumeMatrix.GetColumn(3);

					var scale = WaterTransforms[i].lossyScale;
					var gridSteps = new Vector2(activeGridSize * scale.x, activeGridSize * scale.z);

					var factor = (float)Math.Round(CamPos.x / gridSteps.x);
					var stepLeft = gridSteps.x * factor;
					factor = (float)Math.Round(CamPos.z / gridSteps.y );
					var stepUp = gridSteps.y * factor;

				//	We have to factor in the original position
					position.x = stepLeft + (position.x % gridSteps.x);
					position.z = stepUp + (position.z % gridSteps.y);

					WatervolumeMatrix.SetColumn(3, position);

				}

				var waterMaterial = WaterMaterials[i];

				UnityEngine.Profiling.Profiler.BeginSample("Set up Gerstner");
				//	Gerstner Waves
					
				//	SetGerstnerWaves(i); // this produces 32bytes of garbage when entering a volume? So we do it manually.
					if (waterMaterial.GetFloat(GerstnerEnabledPID) == 1.0f) {
						mat.EnableKeyword("GERSTNERENABLED");
						mat.SetVector(LuxWaterMask_GerstnerVertexIntensityPID, waterMaterial.GetVector(GerstnerVertexIntensityPID) );
						mat.SetVector(LuxWaterMask_GAmplitudePID, waterMaterial.GetVector(GAmplitudePID) );
						mat.SetVector(LuxWaterMask_GFinalFrequencyPID, waterMaterial.GetVector(GFinalFrequencyPID) );
						mat.SetVector(LuxWaterMask_GSteepnessPID, waterMaterial.GetVector(GSteepnessPID) );
						mat.SetVector(LuxWaterMask_GFinalSpeedPID, waterMaterial.GetVector(GFinalSpeedPID) );
						mat.SetVector(LuxWaterMask_GDirectionABPID, waterMaterial.GetVector(GDirectionABPID) );
						mat.SetVector(LuxWaterMask_GDirectionCDPID, waterMaterial.GetVector(GDirectionCDPID) );
						mat.SetVector(LuxWaterMask_GerstnerSecondaryWaves, waterMaterial.GetVector(GerstnerSecondaryWaves) );
					}
					else {
						mat.DisableKeyword("GERSTNERENABLED");
					}

				//	Projectors
					mat.SetFloat(LuxMask_ExtrusionPID, waterMaterial.GetFloat(Lux_ExtrusionPID) );


				UnityEngine.Profiling.Profiler.EndSample();

			//	Draw UnderWaterMask
				UnityEngine.Profiling.Profiler.BeginSample("Draw Water Mask");
					bool useTessellation = waterMaterial.HasProperty(Lux_EdgeLengthPID) && SystemInfo.graphicsShaderLevel >= 46;
					if (useTessellation) {
						mat.SetFloat(Lux_EdgeLengthPID, waterMaterial.GetFloat(Lux_EdgeLengthPID));
					}
					UnityEngine.Profiling.Profiler.BeginSample("Draw Water Volume");				
					if ( i == activeWaterVolume && activeWaterVolumeCameras.Contains(currentCamera) ) {
						if( WaterUsesSlidingVolume[i] && useTessellation) {
							//mat.SetPass(5);
							cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 0, 5); // submesh 0 = Water box volume
						}
						else {
							//mat.SetPass(0);
							cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 0, 0); // submesh 0 = Water box volume	
						}
						//Graphics.DrawMeshNow( WaterMeshes[i], WatervolumeMatrix, 0); // submesh 0 = Water box volume

					}
					UnityEngine.Profiling.Profiler.EndSample();
				//	Draw water surface only if volume is active or AdvancedDeferredFog is enabled
					if(  i == activeWaterVolume && activeWaterVolumeCameras.Contains(currentCamera) || EnableAdvancedDeferredFog ) {
						if (useTessellation) {
							if ( i == activeWaterVolume) {
								//mat.SetPass(3);
								cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 1, 3); // submesh 1 = Water surface	
							}
							else {
								//mat.SetPass(4);
								cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 1, 4); // submesh 1 = Water surface
							}
						}
						else {
							if ( i == activeWaterVolume) {
								//mat.SetPass(1);
								cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 1, 1); // submesh 1 = Water surface
							}
							else {
								//mat.SetPass(2);
								cmb.DrawMesh( WaterMeshes[i], WatervolumeMatrix, mat, 1, 2); // submesh 1 = Water surface	
							}
						}
						//Graphics.DrawMeshNow( WaterMeshes[i], WatervolumeMatrix, 1); // submesh 1 = Water surface
				
					}
				UnityEngine.Profiling.Profiler.EndSample();
			}
			//GL.PopMatrix();
        }

		public void RenderUnderWater(RenderTexture src, RenderTexture dest, Camera currentCamera, bool SecondaryCameraRendering) {

		//	Check if currentCamera is actually within the watervolume
		//	TODO: This prevents fps camera from rendering underwater
			if (activeWaterVolumeCameras.Contains(currentCamera)) {
				if(DoUnderWaterRendering && activeWaterVolume > -1) {
				//	We currently only support one active water volume. So we setup the materials only once.
					if (!UnderwaterIsSetUp) {
						if(Sun) {
							Vector3 SunDir = -Sun.forward;
			                Color SunColor = SunLight.color * SunLight.intensity;
			                if (islinear) {
			                    SunColor = SunColor.linear;
			                }
							Shader.SetGlobalColor(Lux_UnderWaterSunColorPID, (SunColor) * Mathf.Clamp01( Vector3.Dot(SunDir, Vector3.up) )  );
							Shader.SetGlobalVector(Lux_UnderWaterSunDirPID, -SunDir );
							Shader.SetGlobalVector(Lux_UnderWaterSunDirViewSpacePID, currentCamera.WorldToViewportPoint(
								currentCamera.transform.position - SunDir * 1000.0f 
								) );
						}
						if (WaterMaterials[activeWaterVolume].GetFloat(Lux_CausticsEnabledPID) == 1) {
							blitMaterial.EnableKeyword("GEOM_TYPE_FROND");
							if (WaterMaterials[activeWaterVolume].GetFloat(Lux_CausticModePID) == 1) {
								blitMaterial.EnableKeyword("GEOM_TYPE_LEAF");
							}
							else {
								blitMaterial.DisableKeyword("GEOM_TYPE_LEAF");
							}
						}
						else {
							blitMaterial.DisableKeyword("GEOM_TYPE_FROND");
						}
		                if (islinear) {
						    Shader.SetGlobalColor(Lux_UnderWaterFogColorPID, WaterMaterials[activeWaterVolume].GetColor("_Color").linear );
		                }
		                else {
		                    Shader.SetGlobalColor(Lux_UnderWaterFogColorPID, WaterMaterials[activeWaterVolume].GetColor("_Color"));
		                }

		                Shader.SetGlobalFloat(Lux_UnderWaterFogDensityPID, WaterMaterials[activeWaterVolume].GetFloat("_Density") );
						Shader.SetGlobalFloat(Lux_UnderWaterFogAbsorptionCancellationPID, WaterMaterials[activeWaterVolume].GetFloat("_FogAbsorptionCancellation") );

						Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionHeightPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionHeight") );
						Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionMaxHeightPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionMaxHeight") );

						Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionDepthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionDepth") );
						Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionColorStrengthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionColorStrength") );
						Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionStrengthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionStrength") );

						Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringPowerPID, WaterMaterials[activeWaterVolume].GetFloat("_ScatteringPower")); //"_UnderwaterScatteringPower"));
						Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringIntensityPID, WaterMaterials[activeWaterVolume].GetFloat("_UnderwaterScatteringIntensity"));
						if (islinear) {
							Shader.SetGlobalColor(Lux_UnderWaterUnderwaterScatteringColorPID, WaterMaterials[activeWaterVolume].GetColor("_TranslucencyColor").linear);
						}
						else {
							Shader.SetGlobalColor(Lux_UnderWaterUnderwaterScatteringColorPID, WaterMaterials[activeWaterVolume].GetColor("_TranslucencyColor"));
						}
						Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringOcclusionPID, WaterMaterials[activeWaterVolume].GetFloat("_ScatterOcclusion")); //"_UnderwaterScatteringPower"));

					//	This spikes and produces garbage. So we do it in RegisterWaterVolume as well.
						Shader.SetGlobalTexture(Lux_UnderWaterCausticsPID, WaterMaterials[activeWaterVolume].GetTexture(CausticTexPID) );
		                Shader.SetGlobalFloat(Lux_UnderWaterCausticsTilingPID, WaterMaterials[activeWaterVolume].GetFloat("_CausticsTiling"));
		                Shader.SetGlobalFloat(Lux_UnderWaterCausticsScalePID, WaterMaterials[activeWaterVolume].GetFloat("_CausticsScale") );
						Shader.SetGlobalFloat(Lux_UnderWaterCausticsSpeedPID, WaterMaterials[activeWaterVolume].GetFloat("_CausticsSpeed") );
						Shader.SetGlobalFloat(Lux_UnderWaterCausticsTilingPID, WaterMaterials[activeWaterVolume].GetFloat("_CausticsTiling") );
						Shader.SetGlobalFloat(Lux_UnderWaterCausticsSelfDistortionPID, WaterMaterials[activeWaterVolume].GetFloat("_CausticsSelfDistortion") );
						Shader.SetGlobalVector(Lux_UnderWaterFinalBumpSpeed01PID, WaterMaterials[activeWaterVolume].GetVector("_FinalBumpSpeed01") );
						Shader.SetGlobalVector(Lux_UnderWaterFogDepthAttenPID, WaterMaterials[activeWaterVolume].GetVector("_DepthAtten"));
					}
				//	Calculate fog and color attenuation and copy to Screen
					Graphics.Blit(src, dest, blitMaterial, 0);
				}
			//	We have to blit in any case - otherwise the screen will be black.
				else {
					Graphics.Blit(src, dest);
				}
			}
		//	We have to blit in any case - otherwise the screen will be black.
			else {
				Graphics.Blit(src, dest);
			}
		}


		#if UNITY_EDITOR
			void OnDrawGizmos() {
				if (enableDebug) {
					if(cam == null || UnderWaterMask == null) { // || activeWaterVolume == -1)
						return;
					}
			      	int textureDrawWidth = (int)(cam.aspect * 128.0f);
			        GL.PushMatrix();
			        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
			        Graphics.DrawTexture(new Rect(0, 0, textureDrawWidth, 128), UnderWaterMask);
			        GL.PopMatrix();
			    }
			}

            void OnGUI() {
                if(enableDebug) {
                    var Alignement = GUI.skin.label.alignment;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    if (activeWaterVolume == -1) {
                    	GUI.Label(new Rect(10, 0, 160, 40), "Active water volume: none\nRegistered volumes: " + RegisteredWaterVolumes.Count);	
                    }
                    else {
                    	GUI.Label(new Rect(10, 0, 400, 40), "Active water volume: " + RegisteredWaterVolumes[activeWaterVolume].transform.gameObject.name + "\nRegistered volumes: " + RegisteredWaterVolumes.Count ); //+ activeWaterVolume.ToString() );
                    }
                    GUI.skin.label.alignment = Alignement;
                }
            }
		#endif
	}
}