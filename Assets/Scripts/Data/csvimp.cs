using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using CsvHelper;
using Unity.VisualScripting;
using Esri.HPFramework;
using Esri.ArcGISMapsSDK.Components;
using System.Linq;
using Esri.GameEngine.Map;
// TODO: add library imports


public class Crime
{
    public int INCIDENT_KEY { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public sealed class CrimeData : ClassMap<Crime>
{
    public CrimeData()
    {
        Map(m => m.INCIDENT_KEY).Name("INCIDENT_KEY");
        Map(m => m.Latitude).Name("Latitude");
        Map(m => m.Longitude).Name("Longitude");
    }
}

public class csvimp : MonoBehaviour
{
    public TextAsset textAssetData;

    public Material material;

    public bool AmplitudeIsModified = false;
    public float DefaultAmplitude = 20F;
    public bool FrequencyIsModified = false;
    public float DefaultFrequency = 40F;
    public float AmplitudeOffset = 170f;
    public bool ColorBasedOnData = false;
    public float MaxAddedAltitude = 340f;

    [System.Serializable]
    public class Data
    {
        public Vector2[] dataPoints;
    }

    public Data dataList = new Data();
    private CultureInfo culture;
    public HPRoot HPRoot;
    private Dictionary<HxWaveSpatialEffect, float> _hxWaveSpatialEffects = new Dictionary<HxWaveSpatialEffect, float>();

    public int OnlyTakeFristXFromData = 100;

    public void UpdateTactiles()
    {
        foreach (var item in _hxWaveSpatialEffects)
        {
            item.Key.amplitudeN = item.Value * SettingsHelper.AmplitudeModifier;
            item.Key.frequencyHz = item.Value * SettingsHelper.FrequencyModifier;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(readCSV());
        culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
    }

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    IEnumerator readCSV()
    {
        SettingsHelper.OnChangeCallback = UpdateTactiles;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(5f);
        IEnumerable<Crime> data;
        using (var reader = new StringReader(textAssetData.text))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            data = csv.GetRecords<Crime>().Take(OnlyTakeFristXFromData);
            foreach (var point in data)
            {
                float rdNum = 0.5f;// Random.Range(0.2f, 1f);
                var dataPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dataPoint.layer = gameObject.layer;
                dataPoint.transform.SetParent(HPRoot.transform, false);
                dataPoint.GetComponent<Renderer>().material = material;
                var color = ColorBasedOnData ? Color.HSVToRGB(.3f - (rdNum / 1f) * .3f, 1f, 1f) : dataPoint.GetComponent<Renderer>().material.color;
                color.a = 0.45f;
                dataPoint.GetComponent<Renderer>().material.color = color;
                Destroy(dataPoint.GetComponent<Collider>());
                dataPoint.AddComponent<MeshCollider>();

                dataPoint.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                dataPoint.AddComponent<HPTransform>();
                var arcGisLocation = dataPoint.AddComponent<ArcGISLocationComponent>();
                arcGisLocation.Position = new Esri.GameEngine.Geometry.ArcGISPoint(
                    point.Longitude,
                    point.Latitude,
                    AmplitudeOffset + Random.Range(0f, MaxAddedAltitude),
                    new Esri.GameEngine.Geometry.ArcGISSpatialReference(4326));
                arcGisLocation.Rotation = new Esri.ArcGISMapsSDK.Utils.GeoCoord.ArcGISRotation(0d, 90d, 0d);
                var hxWaveDirectEffect = dataPoint.AddComponent<HxWaveSpatialEffect>();
                _hxWaveSpatialEffects[hxWaveDirectEffect] = rdNum;
                hxWaveDirectEffect.amplitudeN = AmplitudeIsModified ? rdNum * SettingsHelper.AmplitudeModifier : DefaultAmplitude;
                hxWaveDirectEffect.frequencyHz = FrequencyIsModified ? rdNum * SettingsHelper.FrequencyModifier : DefaultFrequency;
                var hxSphereBoundingVolume = dataPoint.AddComponent<HxSphereBoundingVolume>();
                hxWaveDirectEffect.BoundingVolume = hxSphereBoundingVolume;
                hxSphereBoundingVolume.RadiusM = .5f;
            }
        }
    }

}
