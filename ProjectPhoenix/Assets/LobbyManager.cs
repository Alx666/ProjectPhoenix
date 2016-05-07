using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyManager : NetworkLobbyManager
{
    static private Dictionary<NetworkConnection, int> currentPlayers = new Dictionary<NetworkConnection, int>();

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (!currentPlayers.ContainsKey(conn))
            currentPlayers.Add(conn, 0);

        return base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
    }

    static public void SetPlayerType(NetworkConnection conn, int typeId)
    {
        if (currentPlayers.ContainsKey(conn))
            currentPlayers[conn] = typeId;
    }

    public override void OnLobbyServerPlayersReady()
    {
        bool hBool = true;

        currentPlayers.ToList().ForEach(hP => 
        {
            if (!hP.Key.isReady)
                hBool = false;
        }
        );

        if(hBool)
            ServerChangeScene(playScene);
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        int index = currentPlayers[conn];

        GameObject _temp = (GameObject)GameObject.Instantiate(spawnPrefabs[index],
                            startPositions[conn.connectionId].position,
                            Quaternion.identity);

        return _temp;
    }
}
