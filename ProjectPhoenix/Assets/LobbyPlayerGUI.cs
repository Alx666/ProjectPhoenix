using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LobbyPlayerGUI : NetworkLobbyPlayer
{
    public void SetTypeLocal(int id)
    {
        LobbyManager.SetPlayerType(this.connectionToServer, id);
    }

    public void SetReadyToBegin()
    {
        if (this.isLocalPlayer)
            this.SendReadyToBeginMessage();
    }

    public override void OnClientReady(bool readyState)
    {
        if (readyState)
        {
            GetComponentInChildren<Canvas>().enabled = false;
        }
    }
}
