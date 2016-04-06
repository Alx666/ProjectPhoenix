using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
/// <summary>
/// Handles physics based bullets (OnCollisionEnter)
/// artillery shoots, granades, etc...
/// </summary>
/// 
[RequireComponent(typeof(Rigidbody))]
public class BulletPhysics : MonoBehaviour, IBullet, IPoolable
{
    public float MaxDistance;
    public float Force;
    public float Damage;

    private Rigidbody           m_hRigidBody;
    private Vector3             m_vShootPosition;
    private ParticlesController m_hParticlesController;
    private SphereCollider[]    lIST_m_hAOE ;
    private SphereCollider      m_hAOE;
    private Explosion           Expl;


    void Awake()
    {
        m_hRigidBody           = this.GetComponent<Rigidbody>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();
        lIST_m_hAOE            = this.GetComponents<SphereCollider>().ToArray();
        m_hAOE                 = this.GetComponentInChildren<SphereCollider>();
        Expl                   = this.GetComponentInChildren<Explosion>();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        if(m_hParticlesController!= null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward  = vDirection;

        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.Impulse);

        m_vShootPosition = vPosition;
    }


    void OnTriggerEnter(Collider collider)
    {
       Debug.Log("Ho Colliso");


        float m_hDamage = this.Damage;
        //Attivo AOE
       // this.Expl.ExendExpolsion();

        m_hDamage = AoeDamage(Expl.target.transform.position, collider.transform.position);

        IDamageable m_hHit;
        if (collider.gameObject.GetComponent<IDamageable>() != null || Expl.target.GetComponent<IDamageable>() != null)
        {
            if (collider.gameObject.GetComponent<IDamageable>() != null)

                m_hHit = collider.gameObject.GetComponent<IDamageable>();
            else
                m_hHit = Expl.target.GetComponent<IDamageable>();
            
                m_hHit.Damage(m_hDamage);
        }

        Explosion();
    }

    private float AoeDamage(Vector3 targetPosition, Vector3 position)
    {
        if (targetPosition!= Vector3.zero)
        {

        float Raggio           =  m_hAOE.radius;
        float distanceToImpact = targetPosition.z + Raggio - position.z;
        float m_fDamage        = -(this.Damage * (Raggio-distanceToImpact)) / targetPosition.z;
        return Damage - m_fDamage;
        }

        Debug.Log(Damage);
        return Damage;
       
    }

    void Update()
    {        
        if (Vector3.Distance(m_vShootPosition, this.transform.position) > MaxDistance)
        {
            this.m_hRigidBody.velocity = Vector3.zero;
            GlobalFactory.Recycle(this.gameObject);
        }
    }

    #region IPoolable

    public Pool Pool { get; set; }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        this.m_hRigidBody.velocity  = Vector3.zero;
        this.transform.position     = Vector3.zero;
        this.transform.rotation     = Quaternion.identity;
        this.gameObject.SetActive(false);
    }

    #endregion

    protected void Explosion()
    {
        
        this.m_hParticlesController.PlayHitVfx(this.transform.position, this.transform.up);
        GlobalFactory.Recycle(this.gameObject);
    }
}
