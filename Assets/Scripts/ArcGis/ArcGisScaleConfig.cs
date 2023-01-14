using System;
using UnityEngine;
using UnityEngine.Events;

public class ArcGisScaleConfig : MonoBehaviour
{
    [SerializeField]
    public Vector3 TargetScale = new Vector3(0.0002f, 0.0002f, 0.0002f);

    [SerializeField]
    public int ObjectLayer = 16;

    public UnityEvent ZoomApplied;

    private void OnValidate()
    {
        Debug.Log($"Layer set({ObjectLayer})");
        Debug.Log($"Layer set:'{LayerMask.LayerToName(ObjectLayer)}'");
    }

    private void OnEnable()
    {
        //Debug.Log(typeof(RenderZoomLevel).AssemblyQualifiedName);
        Type type = Type.GetType("Esri.ArcGISMapsSDK.Renderer.Renderables.RenderZoomLevel, ArcGISMapsSDK, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
        if (type == null)
        {
            Debug.LogError("ArcGis Fix not applied! Use 'Unity>Tools>esri>Apply arcgis fix' to fix");
            return;
        }
        var field = type.GetField("Zoom");
        field.SetValue(type, TargetScale);
        //RenderLayer.Layer
        type = Type.GetType("Esri.ArcGISMapsSDK.Renderer.Renderables.RenderLayer, ArcGISMapsSDK, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
        field = type.GetField("Layer");
        Debug.Log($"Layer set:'{LayerMask.LayerToName(ObjectLayer)}'");
        field.SetValue(type, ObjectLayer);

        ZoomApplied?.Invoke();
        
    }

}
