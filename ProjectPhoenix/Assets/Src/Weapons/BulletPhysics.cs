using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysics : NetworkBehaviour, IPoolable, IBullet
{
    public float Damage;
    [Range(0f, 100f)]
    public float Force;
    public float MaxRadius;
    public float ForceExolosion;

    private ParticlesController m_hParticlesController;
    private Rigidbody m_hRigidBody;
    private Vector3 m_vShootPosition;
    private GameObject target;

    /// /////////////////////////////////////////////
  
    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();

    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        if (m_hParticlesController != null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward = vDirection;

        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.VelocityChange);

        m_vShootPosition = vPosition;
    }

    void OnCollisionEnter(Collision collider)
    {

        Collider[] obj = Physics.OverlapSphere(collider.transform.position, MaxRadius);


        foreach (var Col in obj)
        {
            if (Col.GetComponent<IDamageable>() != null && !this)
            {

                IDamageable h_Hit = collider.gameObject.GetComponent<IDamageable>();

                this.Damage = AoeDamage(Col.transform.position, collider.transform.position);

                h_Hit.Damage(this.Damage);
            }
            if (Col.GetComponent<Rigidbody>() != null && !this)
            {

            Col.GetComponent<Rigidbody>().AddExplosionForce(ForceExolosion, collider.transform.position, MaxRadius, 1);
            }
        }

        Explosion();
    }

    private void Explosion()
    {
        this.m_hParticlesController.PlayHitVfx(this.transform.position, this.transform.up);

        if (this.Pool != null)
        {
            GlobalFactory.Recycle(this.gameObject);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    #region IPoolable
    public Pool Pool { get; set; }

    public void Disable()
    {
        this.m_hRigidBody.velocity = Vector3.zero;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
       
        this.gameObject.SetActive(false);
    }

    public void Enable()
    {
       
        this.gameObject.SetActive(true);
    }

    #endregion

    private float AoeDamage(Vector3 targetPosition, Vector3 CollisionPosition)
    {
        if (targetPosition != Vector3.zero)
        {

            float Raggio = MaxRadius;
            float distanceToImpact = targetPosition.z + Raggio - CollisionPosition.z;
            float m_fDamage = -(this.Damage * (Raggio - distanceToImpact)) / targetPosition.z;
            return Damage - m_fDamage;
        }

        Debug.Log(Damage);
        return Damage;

    }
}



