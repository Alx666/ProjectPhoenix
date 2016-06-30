using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

//ToDo: MadMaxGameManager
public class GameManager : NetworkBehaviour
{
    public Text EndText;
    public Text ScoreText;
    public Text WoWText;
    public float RespawnTime;
    public Texture2D InGameMouseCursor;
    public int ScoreToWin;
    static public GameManager Instance { get; private set; }

    private Dictionary<Actor, int> scores;
    private List<NetworkStartPosition> m_hSpawnPoints;
    private IVictoryCondition m_hVictoryCondition;

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
        //GameObject.DontDestroyOnLoad(this.gameObject);
        m_hSpawnPoints = new List<NetworkStartPosition>(FindObjectsOfType<NetworkStartPosition>());

        if (isServer)
            StartCoroutine(WaitForInitialization(2f));

        //ToDo: Rendere victory condition generica
        m_hVictoryCondition = new DeathMatchWinCondition(ScoreToWin);

        Cursor.SetCursor(InGameMouseCursor, new Vector2(16, 16), CursorMode.Auto);
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
        return scores.OrderByDescending(hS => hS.Value).FirstOrDefault().Value;
    }

    void GameTerminated()
    {
        if (scores.OrderByDescending(hS => hS.Value).FirstOrDefault().Key.isLocalPlayer)
            EndText.text = "HAIL VICTORY";
        else
            EndText.text = "STINKY DEFEAT";

        StartCoroutine(WaitForEndGame(3f));
    }

    void AddScore(int value, Actor killer)
    {
        scores[killer] += value;
        if (m_hVictoryCondition.Check())
            GameTerminated();
    }

    public void WoW(Actor killer, Actor killed)
    {
        WoWText.text = killer.Name + " pwned " + killed.Name + "\n";

        if (killer == killed)
            AddScore(-1, killer);
        else
            AddScore(1, killer);
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
        return m_hSpawnPoints[UnityEngine.Random.Range(0, m_hSpawnPoints.Count - 1)].transform.position;
    }

    IEnumerator WaitForInitialization(float duration)
    {
        yield return new WaitForSeconds(duration);
        List<GameObject> m_hPlayerInstances = new List<GameObject>(LobbyManager.Instance.GetPlayerInstances());
        m_hPlayerInstances.ForEach(hA => RpcSyncPlayer(hA.GetComponent<Actor>().netId));
    }

    IEnumerator WaitForEndGame(float duration)
    {
        yield return new WaitForSeconds(duration);

        Application.Quit();

        Network.Disconnect();

        if (isServer)
            LobbyManager.Instance.lobbySlots.Where(hL => hL != null).ToList().ForEach(hP => Destroy(hP.gameObject));

        //Destroy(LobbyManager.Instance.gameObject);

        SceneManager.LoadScene("DRAIV_Splash");
    }
}
