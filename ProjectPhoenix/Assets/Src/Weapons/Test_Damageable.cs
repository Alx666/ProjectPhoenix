using UnityEngine;
using System;
using System.Collections;


public class Test_Damageable:MonoBehaviour, IDamageable
{
	public float fHp;

	public Test_Damageable()
	{
		
	}

    public void Damage(object damage)
    {
        throw new NotImplementedException();
    }

    public void Damage( float fDmg )
	{
		this.fHp -= fDmg;
		if ( this.fHp <= 0f )
		{
			GameObject.Destroy( this.gameObject );
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
