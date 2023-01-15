using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater {

	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderwaterRenderingSlave : MonoBehaviour {


		private LuxWater_UnderWaterRendering waterrendermanager;
		private bool readyToGo = false;

		private static CommandBuffer cb_MaskSlave;
		private CameraEvent cameraEvent = CameraEvent.BeforeSkybox; // This works for both deferred an forward

		public Camera cam;

		void OnEnable () {
			cam = GetComponent<Camera>();

			cb_MaskSlave = new CommandBuffer();
			cb_MaskSlave.name = "Lux Water: Underwater Mask Slave";
			cam.AddCommandBuffer(cameraEvent, cb_MaskSlave);

		//	Get with LuxWater_UnderWaterRendering singleton – using invoke just in order to get around script execution order problems
			Invoke("GetWaterrendermanager", 0.0f);
		}

		void OnDisable () {
			if(cb_MaskSlave != null && cam != null) {
				cam.RemoveCommandBuffer(cameraEvent, cb_MaskSlave);
			}
		}

		void GetWaterrendermanager() {
			var manager = LuxWater_UnderWaterRendering.instance;
			if (manager != null) {
				waterrendermanager = manager;
				readyToGo = true;
			}
		}

		void OnPreCull () {
			if (readyToGo) {
				waterrendermanager.RenderWaterMask( cam, true, cb_MaskSlave );
			}
		}

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture src, RenderTexture dest) {
			if (readyToGo) {
				waterrendermanager.RenderUnderWater(src, dest, cam, true);
			}
		//	We have to blit in any case - otherwise the screen will be black.
			else {
				Graphics.Blit(src, dest);
			}
		}

	}

}
