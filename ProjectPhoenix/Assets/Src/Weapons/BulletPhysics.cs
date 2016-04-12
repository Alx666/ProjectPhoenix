using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


[RequireComponent(typeof(Rigidbody))]

public class BulletPhysics : MonoBehaviour, IPoolable, IBullet
{
    //viene lanciata e all esplosione fa danni ad area
    //opsioni possibili:
    //1) 1 solo sphere collider che si allarga e si ristringe 
    //2) 2 sphere collider uno trigger e l'altro no il primo riguardante l'esposione mentre il secondo della proiettile fisico

    public float Damage;
    [Range(0f, 100f)]
    public float Force;
    public float MaxRadius;

    private ParticlesController m_hParticlesController;
    private Rigidbody m_hRigidBody;
    private Vector3 m_vShootPosition;
    private GameObject target;
    public bool IsCollided = false;
    public GameObject Target { get { return target; } set { target = value; } }

    /// /////////////////////////////////////////////
  
    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();

    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        if (m_hParticlesController != null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward = vDirection;

        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.Impulse);

        m_vShootPosition = vPosition;
    }


    void OnCollisionEnter(Collision collider)
    {

        Collider[] obj = Physics.OverlapSphere(collider.transform.position, MaxRadius);

        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i].gameObject.GetComponent<IDamageable>() != null && !this)
            {
                
                IDamageable h_Hit = collider.gameObject.GetComponent<IDamageable>();

             this.Damage= AoeDamage(obj[i].transform.position, collider.transform.position);

                h_Hit.Damage(this.Damage);
            }
        }

        Explosion();
    }

    private void Explosion()
    {
        this.m_hParticlesController.PlayHitVfx(this.transform.position, this.transform.up);
        GlobalFactory.Recycle(this.gameObject);
    }

    #region IPoolable
    public Pool Pool { get; set; }

    public void Disable()
    {
        this.m_hRigidBody.velocity = Vector3.zero;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        this.IsCollided = false;
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



