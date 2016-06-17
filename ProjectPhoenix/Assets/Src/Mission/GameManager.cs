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

    static public GameManager Instance { get; private set; }

	private Dictionary<Actor, int> scores;

    //private IVictoryCondition m_hVictoryCondition;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new System.Exception("Multiple Gamemanager detected in scene!");

		scores = new Dictionary<Actor, int>(); //TODO: popolare lista con i player [Compito del lobby manager]
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
        //m_hVictoryCondition = new DeathMatchWinCondition(20);
    }

    void Update()
    {
		ScoreText.text = scores.ToList().Where( hP => hP.Key.isLocalPlayer ).First().Value.ToString();
	}

    void GameTerminated()
    {
        //Debug.Log("Game finish");
    }

	void AddScore(int value, Actor killer )
	{
		scores[killer] += value;
	}

	public void WoW( Actor killer, Actor killed )
	{
		WoWText.text = killer.Name + " pwned " + killed.Name + "\n";
		AddScore( 100, killer );
	}

	public void ShowScores()
	{
		if ( Input.GetKey(KeyCode.Tab) )
		{
			//TODO x SAMUELE: Implementare.
		}
	}
}
