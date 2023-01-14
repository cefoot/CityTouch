using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands.Fileinfo;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tools : UnityEditor.Build.IPreprocessBuildWithReport
{
    public int callbackOrder => 1000;//last thing to do

    [MenuItem("Tools/esri/Apply arcgis fix")]
    public static void ApplyArcGisFix()
    {
        var dir = new System.IO.DirectoryInfo("Library/PackageCache/com.esri.arcgis-maps-sdk@d1c578d68416/SDK/Renderer/Renderables/");
        Debug.Log(dir.FullName);
        Debug.Log(string.Join(";", dir.GetFiles().Select(f => f.Name)));
        var f1 = Resources.LoadAll<TextAsset>("ArcGisFix");
        foreach (var item in f1)
        {
            var file = new System.IO.FileInfo(System.IO.Path.Combine(dir.FullName, item.name));
            using (var stream = file.OpenWrite())
            {
                stream.Position = 0;
                stream.Write(item.bytes, 0, item.bytes.Length);
            }
            Debug.Log($"File '{file.FullName}' updated");
        }
    }

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

    public void OnPreprocessBuild(BuildReport report)
    {
        ApplyArcGisFix();
    }
}
