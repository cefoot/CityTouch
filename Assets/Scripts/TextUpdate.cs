using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUpdate : MonoBehaviour
{

    public void UpdateText(float data)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = data.ToString("F3");
    }

    public void UpdateText(System.Object data)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = data.ToString();
    }
}
