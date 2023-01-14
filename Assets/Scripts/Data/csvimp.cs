using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Globalization;

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

    void readCSV()
    {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
        int tableSize = data.Length / 2;
        dataList.dataPoints = new Vector2[tableSize];

        for (int i = 0; i < tableSize; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);

            float rdNum = Random.Range(0.8f, 1.05f);
            var dataIdx = i * 2;
            dataList.dataPoints[i] = new Vector2(float.Parse(data[dataIdx], culture) / 125000,
            float.Parse(data[dataIdx + 1],culture) / 125000 );

            cube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            cube.transform.localPosition = new Vector3(dataList.dataPoints[i].x, rdNum, dataList.dataPoints[i].y);
        }


    }

}
