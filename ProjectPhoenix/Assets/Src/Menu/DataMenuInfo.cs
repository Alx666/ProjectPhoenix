using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DataMenuInfo : MonoBehaviour
{
    public GameObject SelectedPrefab{ get; set; }


    public void Start()
    {
        this.transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }
}
