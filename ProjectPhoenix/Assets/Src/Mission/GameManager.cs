using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance { get; private set; }

    private IVictoryCondition m_hVictoryCondition;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new System.Exception("Multiple Gamemanager detected in scene!");
    }

    internal int GetHighestScore()
    {
        //ritorna il punteggio piu alto
        //serve implementazione del punteggio
        throw new NotImplementedException();
    }

    void Start()
    {
        //ToDo: Rendere victory condition generica
        m_hVictoryCondition = new DeathMatchWinCondition(20);
    }

    void Update()
    {
        if (m_hVictoryCondition.Check())
            GameTerminated();
    }

    void GameTerminated()
    {
        Debug.Log("Game finish");
    }
}
