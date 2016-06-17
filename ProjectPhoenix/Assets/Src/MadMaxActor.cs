using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MadMaxActor : Actor
{

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
