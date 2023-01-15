using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;

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
    public string FeatureLayerURL = "https://services6.arcgis.com/wuONiWa1WYQCnLzh/arcgis/rest/services/nyc_hex_cut/FeatureServer/0";
    
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
            
            if(i < OutFields.Length - 1)
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
        // Deserialize the JSON response from the query.
        var deserialized = JsonUtility.FromJson<NYCFeatureCollectionData>(Response);
        Debug.Log("deserialized" + deserialized.features);
        foreach (NYCFeature feature in deserialized.features)
        {
            Debug.Log("feature:" + feature.properties.ObjectId);
            if(feature.geometry.coordinates == null) {
                continue;
            }
            double Longitude = feature.geometry.coordinates[0];
            double Latitude = feature.geometry.coordinates[1];

            ArcGISPoint Position = new ArcGISPoint(Longitude, Latitude, 0, new ArcGISSpatialReference(FeatureSRWKID));
            
            var NewPrefab = Instantiate(TouchablePrefab, this.transform);
            // change the Y value according to the count number
            
            NewPrefab.name = feature.properties.ObjectId;
            Points.Add(NewPrefab);
            NewPrefab.SetActive(true);
            var LocationComponent = NewPrefab.GetComponent<ArcGISLocationComponent>();
            LocationComponent.enabled = true;
            LocationComponent.Position = Position;
            LocationComponent.Rotation = new ArcGISRotation(157d, 90d, 0d);
            NewPrefab.transform.GetChild(0).gameObject.transform.localScale = new Vector3(this.transform.localScale.x, float.Parse(feature.properties.count_)*0.1, this.transform.localScale.z);

            var PointInfo = NewPrefab.GetComponent<PointInfo>();

            PointInfo.SetInfo(feature.properties.ObjectId);

            // PointInfo.SetInfo(feature.properties.NAME);
            // PointInfo.SetInfo(feature.properties.TEAM);
            // PointInfo.SetInfo(feature.properties.LEAGUE);

            PointInfo.ArcGISCamera = ArcGISCamera;
            PointInfo.SetSpawnHeight(PrefabSpawnHeight);
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
