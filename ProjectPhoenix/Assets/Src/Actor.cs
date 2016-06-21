using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Actor : NetworkBehaviour, IDamageable
{        
    [SerializeField]
    protected float Hp;
    
    public ArmorType Armor;

    [SyncVar]
    public string Name;

    //[SerializeField]private List<Skill> m_hSkills;


    void Update()
    {
    }
    
    public virtual void Damage(IDamageSource hSource)
    {

    }

    public virtual void Die(Actor Killer)
    {
        Destroy(this.gameObject); //temp
    }
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy
}
