using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DataMenuInfo : MonoBehaviour
{
    public GameObject SelectedPrefab{ get; set; }


    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
