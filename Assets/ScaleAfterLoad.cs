using System;
using System.Collections;
using System.Collections.Generic;
using Esri.GameEngine;
using Esri.GameEngine.Map;
using UnityEngine;

public class ScaleAfterLoad : MonoBehaviour
{
    public float TargetScale = 0.0002f;

    private void OnEnable()
    {
        GetComponent<ArcGISMap>().DoneLoading = new ArcGISLoadableDoneLoadingEvent(DoneLoading);
    }

    private void DoneLoading(Exception ex)
    {
        transform.localScale = transform.localScale * TargetScale;
    }

}
