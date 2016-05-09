using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public static List<GameObject> Players;
    public GameObject player;
    void Awake()
    {
        Players = new List<GameObject>();
        Players.Add(player);
    }

   
}
