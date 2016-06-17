using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class LobbyPlayer : NetworkLobbyPlayer
{
    public string Username { get; set; }

    public GameObject PrefabToSpawn { get; set; }

    public override void OnClientEnterLobby()
    {                
        base.OnClientEnterLobby();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Username = LobbyManager.Instance.NameField.text;
        LobbyManager.Instance.LocalPlayer = this;
        
    }

    public override void OnClientReady(bool readyState)
    {
        base.OnClientReady(readyState);
    }


  
    private void CmdSendUsername(string sName)
    {
        Username = sName;
    }

    [Command]
    public void CmdSendGamePrefab(NetworkHash128 vHash)
    {
        //Debug.Log(vHash.ToString());
        //LobbyManager.Instance.spawnPrefabs.ForEach(x => Debug.Log(x.GetComponent<NetworkIdentity>().assetId.ToString()));

        GameObject hGamePrefab = LobbyManager.Instance.spawnPrefabs.Where(x => x.GetComponent<NetworkIdentity>().assetId.ToString() == vHash.ToString()).FirstOrDefault();
        if (hGamePrefab == null)
            throw new Exception("Requested Prefab Not Found!");

        PrefabToSpawn = hGamePrefab;
    }

}
