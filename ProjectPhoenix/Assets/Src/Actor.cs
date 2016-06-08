using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Actor : NetworkBehaviour, IDamageable
{
    [SyncVar]
    [SerializeField]
    private float Hp;
    
    public ArmorType Armor;
    //[SerializeField]private List<Skill> m_hSkills;

    
    void Update()
    {
        if (Hp <= 0)
            Die();
    }

    #region Damageable
    public void Damage(object damage)
    {
        throw new NotImplementedException();
    }

    public void Damage(float fDmg)
    {
        Hp -= fDmg;
    }
    #endregion

    public virtual void Die()
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
