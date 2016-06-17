using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour {

	public int StartingHealth;
	public Slider HealthSlider;
	public Transform ToLookCamera;

	private int currentHealth;

	void Awake()
	{
		currentHealth = StartingHealth;
	}
	void Start ()
	{
	
	}
	
	void Update ()
	{
		if ( Input.GetKeyDown(KeyCode.Space ) )
		{
			TakeDamage( 10 );
		}
	}

	void LateUpdate()
	{
		transform.LookAt(ToLookCamera );
	}

	public void TakeDamage(int value )
	{
		currentHealth -= value;
		HealthSlider.value = currentHealth;
	}
}
