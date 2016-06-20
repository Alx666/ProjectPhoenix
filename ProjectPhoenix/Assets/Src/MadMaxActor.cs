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

    void Update()
    {
        Slider.value = Mathf.Clamp(Hp, 0f, 100f);
    }

	void LateUpdate()
	{
		Slider.transform.LookAt( Camera.main.transform );
	}

    public override void Damage(IDamageSource hSource)
    {
        this.Hp -= hSource.GetDamage(this.Armor);

        if (this.Hp <= 0f)
        {
            RpcDie(hSource.Owner.netId);
        }
    }

    public override void Die(Actor Killer)
    {
		GameManager.Instance.WoW( Killer, this );
    }
    
    [ClientRpc]
    public void RpcDie(NetworkInstanceId hID)
    {
        Die(ClientScene.FindLocalObject(hID).GetComponent<Actor>());
    }
}
