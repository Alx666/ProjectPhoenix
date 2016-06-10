using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ScoreManager : NetworkBehaviour {

	public Text Text;
	public static int Score;

	void Awake()
	{
		Text = GetComponent<Text>();
		Score = 0;
	}
	void Start ()
	{
	
	}
	
	void Update ()
	{
		Text.text = "Score: " + Score;
		AddPoints();
		RemovePoints();
	}

	void AddPoints()
	{
		if ( Input.GetKeyDown(KeyCode.Space) )
		{
			Score = Score + 100;
		}
	}

	void RemovePoints()
	{
		if ( Input.GetKeyDown(KeyCode.A ) && Score >= 100 )
		{
			Score = Score - 100;
		}
	}
}
