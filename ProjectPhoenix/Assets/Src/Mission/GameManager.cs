using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : NetworkBehaviour
{
    public Text ScoreText;
    public Text WoWText;

    public float RespawnTime;

    static public GameManager Instance { get; private set; }

    private Dictionary<Actor, int> scores;
    private List<NetworkStartPosition> m_hSpawnPoints;

    //private IVictoryCondition m_hVictoryCondition;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new System.Exception("Multiple Gamemanager detected in scene!");

        scores = new Dictionary<Actor, int>(); //TODO: popolare lista con i player [Compito del lobby manager]
    }

    void Start()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
        m_hSpawnPoints = new List<NetworkStartPosition>(FindObjectsOfType<NetworkStartPosition>());

        if (isServer)
            StartCoroutine(WaitForInitialization(2f));

        //ToDo: Rendere victory condition generica
        //m_hVictoryCondition = new DeathMatchWinCondition(20);
    }

    [ClientRpc]
    public void RpcSyncPlayer(NetworkInstanceId hId)
    {
        Actor hActor = ClientScene.FindLocalObject(hId).GetComponent<Actor>();
        if (!scores.ContainsKey(hActor))
            scores.Add(hActor, 0);
    }

    void Update()
    {
        ScoreText.text = scores.ToList().Where(hP => hP.Key.isLocalPlayer).FirstOrDefault().Value.ToString();
    }

    internal int GetHighestScore()
    {
        //ritorna il punteggio piu alto
        //serve implementazione del punteggio
        throw new NotImplementedException();
    }

    void GameTerminated()
    {
        //Debug.Log("Game finish");
    }

    void AddScore(int value, Actor killer)
    {
        scores[killer] += value;
    }

    public void WoW(Actor killer, Actor killed)
    {
        WoWText.text = killer.Name + " pwned " + killed.Name + "\n";
        AddScore(100, killer);
    }

    public void ShowScores()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            //TODO x SAMUELE: Implementare.
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return m_hSpawnPoints[UnityEngine.Random.Range(0, m_hSpawnPoints.Count-1)].transform.position;
    }

    IEnumerator WaitForInitialization(float duration)
    {
        yield return new WaitForSeconds(duration);
        List<GameObject> m_hPlayerInstances = new List<GameObject>(LobbyManager.Instance.GetPlayerInstances());
        m_hPlayerInstances.ForEach(hA => RpcSyncPlayer(hA.GetComponent<Actor>().netId));
    }
}
