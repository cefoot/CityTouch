using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DummyCreator : MonoBehaviour
{
    public Material material;

    public int CountX = 20;
    public int CountZ = 20;
    public float MaxHeight = .3f;
    public Vector3 Offset = new Vector3(.5f, 0f, .5f);
    // Start is called before the first frame update
    void Start()
    {
        var sizeX = 1f / CountX;
        var sizeZ = 1f / CountZ;
        for (var x = 0; x < CountX; x++)
        {
            for (var z = 0; z < CountZ; z++)
            {
                var height = Random.Range(.05f, MaxHeight);
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.layer = gameObject.layer;
                obj.transform.parent = transform;
                obj.transform.localPosition = new Vector3(sizeX * x, height / 2f, sizeZ * z) - Offset;
                obj.transform.localScale = new Vector3(sizeX, height, sizeZ);
                obj.GetComponent<Renderer>().material = material;
                var color = Color.HSVToRGB(.3f - (height / MaxHeight) * .3f, 1f, 1f);
                color.a = 0.45f;
                obj.GetComponent<Renderer>().material.color = color;
                Destroy(obj.GetComponent<Collider>());
                obj.AddComponent<MeshCollider>();
                var hxWaveDirectEffect = obj.AddComponent<HxWaveSpatialEffect>();
                hxWaveDirectEffect.amplitudeN = height * 200;
                hxWaveDirectEffect.frequencyHz = 10 + x * 10;
                var hxSphereBoundingVolume = obj.AddComponent<HxSphereBoundingVolume>();
                hxWaveDirectEffect.BoundingVolume = hxSphereBoundingVolume;
                hxSphereBoundingVolume.RadiusM = .5f;

            }
        }
    }

}
