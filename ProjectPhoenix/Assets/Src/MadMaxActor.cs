using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class MadMaxActor : Actor
{
	public Slider Slider;
	public Transform ToLookCamera;

	private float currentHealth;

	void Awake()
	{
		currentHealth = Hp;
		Slider = this.GetComponentInChildren<Slider>();
	}

	void LateUpdate()
	{
		Slider.transform.LookAt( ToLookCamera );
	}
    public override void Damage(IDamageSource hSource)
    {
        this.Hp -= hSource.GetDamage(this.Armor);

        if (this.Hp == 0f)
        {
            this.Die(hSource.Owner);
        }
    }

    public override void Die(Actor Killer)
    {
		GameManager.Instance.WoW( Killer, this );
    }
}
