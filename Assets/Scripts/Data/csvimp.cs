using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Globalization;
// TODO: add library imports


public class Foo
{
    public int Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}

public sealed class FooMap : ClassMap<Foo>
{
    public FooMap()
    {
        Map(m => m.Id).Name("INCIDENT_KEY");
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


    // Start is called before the first frame update
    void Start()
    {
        readCSV();
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

    void readCSV()
    {
        using (var writer = GenerateStreamFromString(textAssetData.text))
        using (var reader = new StreamReader(writer))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var data = csv.GetRecords<Foo>();
        }

        int tableSize = data.Length;

        foreach (let point in data)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);

            float rdNum = Random.Range(0.8f, 1.05f);
            //dataList.dataPoints[i] = new Vector2(float.Parse(data[dataIdx], culture) / 125000,
            // / 125000 );

            //TODO: set HP transforms
            cube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            cube.transform.localPosition = new Vector3(dataList.dataPoints[i].x, rdNum, dataList.dataPoints[i].y);
        }
    }

}
