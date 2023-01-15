using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Unity.VisualScripting;
using Esri.HPFramework;
using System.Linq;
using System;

[System.Serializable]
class NYCFeatureCollectionData
{
    public string type;
    public NYCFeature[] features;
}

[System.Serializable]
class NYCFeature
{
    public string type;
    public Geometry geometry;
    public NYCHexaProperties properties;
}

[System.Serializable]
class NYCHexaProperties
{
    public string radius;
    public string count_;
    public string ObjectId;
}

public class NYCHexaFeatureLayerQuery : MonoBehaviour
{
    public string FeatureLayerURL = "https://services6.arcgis.com/wuONiWa1WYQCnLzh/arcgis/rest/services/nyc_hex_cut_smaller/FeatureServer/0";

    // This prefab will be instatiated for each feature we parse
    public GameObject TouchablePrefab;

    // The height where we spawn the prefab before finding the ground height
    // TODO: this should be randomized
    private int PrefabSpawnHeight = 10000;

    // This will hold a reference to each feature we created
    public List<GameObject> Points = new List<GameObject>();

    // In the query request we can denote the Spatial Reference we want the return geometries in.
    // It is important that we create the GameObjects with the same Spatial Reference
    private int FeatureSRWKID = 4326;

    // This camera reference will be passed to the points to calculate the distance from the camera to each point
    public ArcGISCameraComponent ArcGISCamera;

    private Dictionary<HxWaveSpatialEffect, float> _keyValuePairs = new Dictionary<HxWaveSpatialEffect, float>();

    public HPRoot HPRoot;
    public Material material;
    public bool ColorBasedOnData = false;
    public float AmplitudeOffset = 100f;
    public float MaxAddedAltitude = 10f;
    private Dictionary<HxWaveSpatialEffect, float> _hxWaveSpatialEffects = new Dictionary<HxWaveSpatialEffect, float>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetFeatures());
    }

    // Sends the Request to get features from the service
    private IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm


        string QueryRequestURL = FeatureLayerURL + "/Query?" + MakeRequestHeaders();
        Debug.Log(QueryRequestURL);
        UnityWebRequest Request = UnityWebRequest.Get(QueryRequestURL);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
            PopulatePointDropdown();
        }
    }

    // Creates the Request Headers to be used in our HTTP Request
    // f=geojson is the output format
    // where=1=1 gets every feature. geometry based or more intelligent where clauses should be used
    //     with larger datasets
    // outSR=4326 gets the return geometries in the SR 4326
    // outFields=LEAGUE,TEAM,NAME specifies the fields we want in the response
    private string MakeRequestHeaders()
    {
        // string[] OutFields =
        // {
        //     "LEAGUE",
        //     "TEAM",
        //     "NAME"
        // };
        string[] OutFields =
        {
            "ObjectId",
            "count_",
            "radius"
        };

        string OutFieldHeader = "outFields=";
        for (int i = 0; i < OutFields.Length; i++)
        {
            OutFieldHeader += OutFields[i];

            if (i < OutFields.Length - 1)
            {
                OutFieldHeader += ",";
            }
        }

        string[] RequestHeaders =
        {
            "f=geojson",
            "where=1=1",
            "outSR=" + FeatureSRWKID.ToString(),
            OutFieldHeader
        };

        string ReturnValue = "";
        for (int i = 0; i < RequestHeaders.Length; i++)
        {
            ReturnValue += RequestHeaders[i];

            if (i < RequestHeaders.Length - 1)
            {
                ReturnValue += "&";
            }
        }

        return ReturnValue;
    }


    // Given a valid response from our query request to the feature layer, this method will parse the response text
    // into geometries and properties which it will use to create new GameObjects and locate them correctly in the world.
    // This logic will differ based on the properties you are trying to parse out of the response.
    private void CreateGameObjectsFromResponse(string Response)
    {
        SettingsHelper.OnChangeCallback = OnWaveValuesChanged;
        // Deserialize the JSON response from the query.
        var deserialized = JsonUtility.FromJson<NYCFeatureCollectionData>(Response);
        Debug.Log("deserialized" + deserialized.features);
        float max = deserialized.features.Select(f => int.Parse(f.properties.count_)).Max();
        float min = deserialized.features.Select(f => int.Parse(f.properties.count_)).Min();
        foreach (NYCFeature feature in deserialized.features)
        {
            if (feature.geometry.coordinates == null)
            {
                continue;
            }
            double Longitude = feature.geometry.coordinates[0];
            double Latitude = feature.geometry.coordinates[1];

            ArcGISPoint Position = new ArcGISPoint(Longitude, Latitude, 0, new ArcGISSpatialReference(FeatureSRWKID));

            var NewPrefab = Instantiate(TouchablePrefab, transform);
            NewPrefab.transform.localPosition = Vector3.zero;

            // change the Y value according to the count number

            NewPrefab.name = feature.properties.ObjectId;
            Points.Add(NewPrefab);
            NewPrefab.SetActive(true);
            NewPrefab.AddComponent<HPTransform>();
            var LocationComponent = NewPrefab.AddComponent<ArcGISLocationComponent>();
            LocationComponent.Position = Position;
            LocationComponent.Rotation = new ArcGISRotation(157d, 90d, 0d);
            float datVal = int.Parse(feature.properties.count_);
            datVal -= min;
            datVal /= (max - min);
            NewPrefab.transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.24f, datVal, 0.24f);
            var effect = NewPrefab.GetComponentInChildren<HxWaveSpatialEffect>();
            effect.transform.localScale = new Vector3(0.24f, .3f, 0.24f);

            _keyValuePairs[effect] = datVal;
            effect.amplitudeN = datVal * SettingsHelper.AmplitudeModifier;
            effect.frequencyHz = datVal * SettingsHelper.FrequencyModifier;


            var PointInfo = NewPrefab.GetComponent<PointInfo>();

            PointInfo.SetInfo(feature.properties.ObjectId);

            // PointInfo.SetInfo(feature.properties.NAME);
            // PointInfo.SetInfo(feature.properties.TEAM);
            // PointInfo.SetInfo(feature.properties.LEAGUE);

            PointInfo.ArcGISCamera = ArcGISCamera;
            PointInfo.SetSpawnHeight(PrefabSpawnHeight);

            int count = int.Parse(feature.properties.count_);
            for (var i = 0; i < count * 2; i++)
            {
                Debug.Log("count" + count);
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

                dataPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                dataPoint.AddComponent<HPTransform>();
                var arcGisLocation = dataPoint.AddComponent<ArcGISLocationComponent>();
                var LatOffset = Random.Range(-0.0010f, 0.0010f);
                var LngOffset = Random.Range(-0.0010f, 0.0010f);
                arcGisLocation.Position = new Esri.GameEngine.Geometry.ArcGISPoint(
                    Longitude + LngOffset,
                    Latitude + LatOffset,
                    AmplitudeOffset + Random.Range(0f, MaxAddedAltitude * datVal),
                    new Esri.GameEngine.Geometry.ArcGISSpatialReference(4326));
                arcGisLocation.Rotation = new Esri.ArcGISMapsSDK.Utils.GeoCoord.ArcGISRotation(0d, 90d, 0d);
                var hxWaveDirectEffect = dataPoint.AddComponent<HxWaveSpatialEffect>();
                _hxWaveSpatialEffects[hxWaveDirectEffect] = rdNum;
                hxWaveDirectEffect.amplitudeN = SettingsHelper.AmplitudeModifier;
                hxWaveDirectEffect.frequencyHz = SettingsHelper.FrequencyModifier;
                var hxSphereBoundingVolume = dataPoint.AddComponent<HxSphereBoundingVolume>();
                hxWaveDirectEffect.BoundingVolume = hxSphereBoundingVolume;
                // hxSphereBoundingVolume.RadiusM = .5f;
            }
        }
    }

    private void OnWaveValuesChanged()
    {
        foreach (var item in _keyValuePairs)
        {
            item.Key.amplitudeN = SettingsHelper.AmplitudeModifier * item.Value;
            item.Key.frequencyHz = SettingsHelper.FrequencyModifier * item.Value;
        }
    }

    // Populates the prefab drown down with all the Points names from the Points list
    private void PopulatePointDropdown()
    {
        //Populate Point name drop down
        List<string> PointNames = new List<string>();
        foreach (GameObject Point in Points)
        {
            PointNames.Add(Point.name);
        }
        PointNames.Sort();
    }
}
