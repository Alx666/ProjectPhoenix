using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightFlicker : MonoBehaviour
{

    public List<Animator> LightAnimators;

    public float RandomTime;

    private float Targettime;
    private float CurrentTime;

	// Use this for initialization
	void Start ()
    {
        CurrentTime = 0.0f;
        Targettime = Random.Range(2.0f, RandomTime);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(CurrentTime >= Targettime)
        {
            LightAnimators.ForEach(hA => hA.SetTrigger("Flicker"));
            Targettime = Random.Range(0.0f, RandomTime);
            CurrentTime = 0.0f;
        }
        else
        {
            CurrentTime += Time.deltaTime;
        }
	}
}
