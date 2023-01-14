using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tools : MonoBehaviour
{
    [MenuItem("Tools/Hirachy/ShowAllElements", false, 0)]
    public static void UnHideObjects()
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var obj in scene.GetRootGameObjects())
        {
            UnHideObjects(obj);
        }
    }

    private static void UnHideObjects(GameObject obj)
    {
        obj.hideFlags = HideFlags.None;
        foreach (Transform item in obj.transform)
        {
            UnHideObjects(item.gameObject);
        }
    }
}
