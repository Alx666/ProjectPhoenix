using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class WoWManager : NetworkBehaviour {

	public Text SomeText;
	private int count = 0;
	private float time = 1f;

	void Awake()
	{
		SomeText = GetComponentInChildren<Text>();
	}

	void Start ()
	{
	
	}
	
	void Update ()
	{
		time -= Time.deltaTime;
		if ( time <=0f )
		{
			SomeText.text += ++count + "\n";
			time = 0.5f;
		}
	}
}
