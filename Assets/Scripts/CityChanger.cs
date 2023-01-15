using System;
using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Map;
using Esri.HPFramework;
using UnityEngine;

public class CityChanger : MonoBehaviour
{
    public ArcGISMapComponent map;
    public ArcGISLocationComponent camera;
    public LocationData[] Locations;

    [Serializable]
    public class LocationData
    {
        public string Name;
        public double Longitude;
        public double Latitude;
        public double Altitude;
        public double CamAltitude;
    }

    public void ChangeLocation(int index)
    {
        StartCoroutine(ChangeLocationAsync(index));
        LocationData data = Locations[index];
        var arcGISPoint = new Esri.GameEngine.Geometry.ArcGISPoint(
                    data.Longitude,
                    data.Latitude,
                    data.Altitude,
                    map.OriginPosition.SpatialReference
                    );
        map.OriginPosition = arcGISPoint;
        var extent = map.Extent;
        extent.GeographicCenter = arcGISPoint;
        map.Extent = extent;
        arcGISPoint = new Esri.GameEngine.Geometry.ArcGISPoint(
                    data.Longitude,
                    data.Latitude,
                    data.CamAltitude,
                    map.OriginPosition.SpatialReference
                    );
        camera.Position = arcGISPoint;
    }

    private IEnumerator ChangeLocationAsync(int index)
    {
        LocationData data = Locations[index];
        var arcGISPoint = new Esri.GameEngine.Geometry.ArcGISPoint(
                    data.Longitude,
                    data.Latitude,
                    data.Altitude,
                    map.OriginPosition.SpatialReference
                    );
        map.OriginPosition = arcGISPoint;
        var extent = map.Extent;
        extent.GeographicCenter = arcGISPoint;
        map.Extent = extent;
        yield return new WaitForEndOfFrame();
        arcGISPoint = new Esri.GameEngine.Geometry.ArcGISPoint(
                    data.Longitude,
                    data.Latitude,
                    data.CamAltitude,
                    map.OriginPosition.SpatialReference
                    );
        camera.Position = arcGISPoint;
        yield return new WaitForSecondsRealtime(0.5f);
        camera.PushChangesToHPTransform();
        yield return new WaitForSecondsRealtime(0.5f);
        camera.SyncPositionWithHPTransform();
        yield return new WaitForSecondsRealtime(0.5f);
        camera.PushChangesToHPTransform();
    }
}
