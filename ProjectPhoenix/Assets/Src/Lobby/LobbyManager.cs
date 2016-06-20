using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class LobbyManager : NetworkLobbyManager
{
    public float            PrematchCountdown = 5.0f;

    public bool IsServer { get; private set; }

    public List<GameObject> ObjectsToEnable;
    public Text             IPField;
    public Text             NameField;
    public LobbyPlayer      LocalPlayer { get; set; }

    private Dictionary<NetworkConnection, LobbyPlayer> m_hCurrentLobbyPlayers;
    private List<GameObject> m_hCurrentPlayersIntances;

    public static LobbyManager Instance { get; private set; }

    void Awake()
    {
        m_hCurrentLobbyPlayers = new Dictionary<NetworkConnection, LobbyPlayer>();
        m_hCurrentPlayersIntances = new List<GameObject>();
        GameObject.DontDestroyOnLoad(this.gameObject);

        if (Instance != null)
            throw new Exception("Duplicate Lobby Manager Detected!");

        Instance = this;
    }

    #region Control Methods

    public void OnStartHostButton()
    {
        this.StartHost();
        IsServer = true;
    }

    public void OnJoinHostButton()
    {
        networkAddress = IPField.text;
        this.StartClient();                                     //Host Joined Callback
    }

    public GameObject GamePrefab
    {
        set
        {
            LocalPlayer.PrefabToSpawn = value;       
            
            if(!this.IsServer)     
                LocalPlayer.CmdSendGamePrefab(value.GetComponent<NetworkIdentity>().assetId);

            LocalPlayer.SendReadyToBeginMessage();
        }
        get
        {
            return LocalPlayer.PrefabToSpawn;
        }
    }
    

    #endregion

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        LobbyPlayer hLobbyPlayer = (GameObject.Instantiate(lobbyPlayerPrefab.gameObject) as GameObject).GetComponent<LobbyPlayer>();
        hLobbyPlayer.Username = NameField.text;
        m_hCurrentLobbyPlayers.Add(conn, hLobbyPlayer);
        return hLobbyPlayer.gameObject;
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log(m_hCurrentLobbyPlayers[conn].Username);
        GameObject hNew = (GameObject)GameObject.Instantiate(m_hCurrentLobbyPlayers[conn].PrefabToSpawn, startPositions[conn.connectionId].position, Quaternion.identity);
        m_hCurrentPlayersIntances.Add(hNew);
        return hNew;
    } 

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ObjectsToEnable.ForEach(x => x.SetActive(true));
        this.GetComponentInChildren<RectTransform>().gameObject.SetActive(false);

        if (!NetworkServer.active)
        {
            //Pure Client
        }
        else
        {
            //Client on Server Machine
        }

        //conn.RegisterHandler(MsgKicked, KickedMessageHandler);
    }


    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        //conn.playerControllers[0].unetView.isServer
        //conn.playerControllers[0].unetView.isClient
    }

    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        if (m_hCurrentLobbyPlayers.ContainsKey(conn))
            m_hCurrentLobbyPlayers.Remove(conn);
    }

    public override void OnLobbyServerPlayersReady()
    {        
        bool allready = true;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
                allready &= lobbySlots[i].readyToBegin;
        }

        if (allready)
            ServerChangeScene(playScene);
    }

    public List<GameObject> GetPlayerInstances()
    {
        return m_hCurrentPlayersIntances;
    }

    //public IEnumerator ServerCountdownCoroutine()
    //{
    //    float remainingTime = PrematchCountdown;
    //    int floorTime = Mathf.FloorToInt(remainingTime);

    //    while (remainingTime > 0)
    //    {
    //        yield return null;

    //        remainingTime -= Time.deltaTime;
    //        int newFloorTime = Mathf.FloorToInt(remainingTime);

    //        if (newFloorTime != floorTime)
    //        {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
    //            floorTime = newFloorTime;

    //            for (int i = 0; i < lobbySlots.Length; ++i)
    //            {
    //                if (lobbySlots[i] != null)
    //                {
    //                    //there is maxPlayer slots, so some could be == null, need to test it before accessing!
    //                    (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
    //                }
    //            }
    //        }
    //    }

    //    for (int i = 0; i < lobbySlots.Length; ++i)
    //    {
    //        if (lobbySlots[i] != null)
    //        {
    //            (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
    //        }
    //    }

    //    ServerChangeScene(playScene);
    //}

    //public override void OnStartHost()
    //{
    //    base.OnStartHost();
    //}

    //public override void OnLobbyServerDisconnect(NetworkConnection conn)
    //{
    //}

    //public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    //{
    //    return true;
    //}

    //public override void OnClientDisconnect(NetworkConnection conn)
    //{
    //    base.OnClientDisconnect(conn);
    //}

    //public override void OnClientError(NetworkConnection conn, int errorCode)
    //{
    //}


}

