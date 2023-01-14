using System;
using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Renderer.Renderables;
using Esri.GameEngine;
using Esri.GameEngine.Map;
using UnityEngine;

public class ArcGisScaleConfig : MonoBehaviour
{
    public Vector3 TargetScale = new Vector3(0.0002f, 0.0002f, 0.0002f);

    private void OnEnable()
    {
        RenderZoomLevel.Zoom = TargetScale;
    }

}
