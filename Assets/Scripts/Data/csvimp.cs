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

    [System.Serializable]

    public class Data
    {
        public Vector2[] dataPoints;
    }

    public Data dataList = new Data();
    private CultureInfo culture;
    public HPRoot HPRoot;

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
        IEnumerable<Crime> data;
        using (var reader = new StringReader(textAssetData.text))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            data = csv.GetRecords<Crime>().Take(400);
            foreach (var point in data)
            {
                yield return new WaitForEndOfFrame();
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(HPRoot.transform, false);

                float rdNum = Random.Range(0.8f, 1.05f);

                //TODO: set HP transforms
                cube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                cube.AddComponent<HPTransform>();
                cube.AddComponent<ArcGISLocationComponent>().Position = new Esri.GameEngine.Geometry.ArcGISPoint(
                    point.Longitude,
                    point.Latitude,
                    rdNum, 
                    new Esri.GameEngine.Geometry.ArcGISSpatialReference(4326));
                yield return new WaitForEndOfFrame();
                Debug.Log($"Long:{point.Longitude}:Lat:{point.Latitude} ({cube.transform.localPosition.ToString("F3")})");
                var worldPos = cube.transform.position;

                //cube.transform.localPosition = transform.InverseTransformDirection(cube.transform.localPosition);
                //hpTrans.UniversePosition = new Unity.Mathematics.double3(point.Latitude, rdNum, point.Longitude);
                //cube.transform.localPosition = new Vector3(point.Latitude, rdNum, point.Longitude);
            }
        }
    }

}
