using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class EditorAssetManager : MonoBehaviour
{
    [MenuItem("TanksMageddon/Generate")]
    static void GeneratePrefabs()
    {
        FlatternHierarchy();
        ClearNoMeshRenderers();
        NormalizeShaders();
        GenerateColliders();
        SavePrefabs();
    }

    [MenuItem("TanksMageddon/Normalize Shaders")]
    static void NormalizeShaders()
    {
        Shader standardShader = Shader.Find("Standard");
        List<Renderer> renderers = GameObject.FindObjectsOfType<Renderer>().ToList();
        renderers.ForEach(renderer =>
        {
            renderer.sharedMaterials.ToList().ForEach(material =>
            {
                if (material.shader != Shader.Find("Standard"))
                {
                    Debug.Log("Standardizing shader on material '" + material.name + "'.");
                    material.shader = standardShader;
                }
            });
        });
    }

    [MenuItem("TanksMageddon/Flattern Hierarchy")]
    private static void FlatternHierarchy()
    {
        object[] x = FindObjectsOfType(typeof(GameObject));
        DeparentChilder(x);

    }

    private static void DeparentChilder(object[] x)
    {
        GameObject oggetto;
        foreach (var item in x)
        {
            oggetto = item as GameObject;
            oggetto.transform.DetachChildren();

        }
    }
    [MenuItem("TanksMageddon/Clear No MeshRenderers")]
    private static void ClearNoMeshRenderers()
    {
        object[] x = FindObjectsOfType(typeof(GameObject));
        DestroyAsd(x);
    }

    private static void DestroyAsd(object[] x)
    {
        GameObject oggetto;
        foreach (var item in x)
        {
            oggetto = item as GameObject;

            if (oggetto.GetComponent<MeshRenderer>() != null || oggetto.GetComponent<Terrain>() != null || oggetto.GetComponent<Light>() != null || oggetto.GetComponent<Camera>() != null)
                continue;

            DestroyImmediate(oggetto);
        }
    }

    [MenuItem("TanksMageddon/AddCollider")]
    static void GenerateColliders()
    {
        GameObject[] ObjectInScene = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject Obj in ObjectInScene)
        {
            if (Obj.GetComponent<MeshRenderer>())
            {
                Obj.AddComponent<MeshCollider>();
            }
        }
    }

    [MenuItem("TanksMageddon/SavePrefabs")]
    public static void SavePrefabs()
    {
        //EditorWindow.GetWindow(typeof(CreatePrefab));
        GameObject[] obj = FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach (GameObject x in obj)
        {
            string FileLocation = "Assets/Resources/" + x.name + ".prefab";

            Object prefab = PrefabUtility.CreateEmptyPrefab(FileLocation);
            PrefabUtility.ReplacePrefab(x, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
    }
}
