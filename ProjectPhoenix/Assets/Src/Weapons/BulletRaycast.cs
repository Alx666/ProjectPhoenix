using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;


/// <summary>
/// Handles fast straight-line flying projectiles based on raycast logic
/// </summary>
public class BulletRaycast : NetworkBehaviour, IBullet, IPoolable
{
    
    public float MaxDistance = 50f;
    public float Speed = 1f;
    public float Damage = 10f;
    [Range(0f, 1f)]
    public float LightArmorDamageRate;
    [Range(0f, 1f)]
    public float MediumArmorDamageRate;
    [Range(0f, 1f)]
    public float HeavyArmorDamageRate;
    private float m_fTotalDistance;
    private ParticlesController m_hParticlesController;
    public Actor Owner { get; set; }

    void Awake()
    {
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection, Actor hOwner)
    {
        if (m_hParticlesController != null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward = vDirection;

        Owner = hOwner;
    }

    public float GetDamage(ArmorType armor)
    {
        float damage = 0; 

        switch (armor)
        {
            case ArmorType.Light:
                damage = Damage * LightArmorDamageRate;
                break;
            case ArmorType.Medium:
                damage = Damage * MediumArmorDamageRate;
                break;
            case ArmorType.Heavy:
                damage = Damage * HeavyArmorDamageRate;
                break;
            default:
                break;
        }
        return damage;
    }

    void Update()
    {
        RaycastHit vRaycast;

        float fSpace = Speed * Time.deltaTime;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out vRaycast, fSpace))
        {
            Actor hHit = vRaycast.collider.gameObject.GetComponent<Actor>();

            if (hHit != null)
            {
                if (m_hParticlesController != null)
                    m_hParticlesController.PlayHitVfx(vRaycast.point, vRaycast.normal);

                if (isServer)
                {
                    ArmorType armor = hHit.Armor;
                    float rate = damageRates[armor];
                    hHit.Damage(this);//non necessita di rpc perche agisce su Hp syncVar
                }
            }
            else
            {
                if (m_hParticlesController != null)
                    m_hParticlesController.PlayMissVfx(vRaycast.point, vRaycast.normal);
            }
            if(isServer)
                GlobalFactory.Recycle(this.gameObject);
        }
        else
        {
            this.transform.position += this.transform.forward * fSpace;

            m_fTotalDistance += fSpace;

            if (m_fTotalDistance > MaxDistance && isServer)
                GlobalFactory.Recycle(this.gameObject);
        }
    }

    #region IPoolable

    public Pool Pool { get; set; }

    

    public void Enable()
    {
        this.gameObject.SetActive(true);
        m_fTotalDistance = 0f;

        if (isServer)
            RpcEnable();
    }

    [ClientRpc]
    void RpcEnable()
    {
        Enable();
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);

        if (isServer)
            RpcDisable();
    }

    [ClientRpc]
    void RpcDisable()
    {
        Disable();
    }

    #endregion
}
