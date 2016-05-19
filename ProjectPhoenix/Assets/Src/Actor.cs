using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Actor : NetworkBehaviour, IDamageable
{
    [SyncVar]
    [SerializeField]
    private float Hp;
    [SerializeField]
    private ArmorType Armor;
    //[SerializeField]private List<Skill> m_hSkills;

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
}

public enum ArmorType
{
    Light,
    Medium,
    Heavy
}
