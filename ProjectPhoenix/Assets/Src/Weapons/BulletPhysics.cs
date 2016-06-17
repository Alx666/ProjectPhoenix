using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


[RequireComponent(typeof(Rigidbody))]

public class BulletPhysics : NetworkBehaviour, IPoolable, IBullet
{
    //viene lanciata e all esplosione fa danni ad area
    //opsioni possibili:
    //1) 1 solo sphere collider che si allarga e si ristringe 
    //2) 2 sphere collider uno trigger e l'altro no il primo riguardante l'esposione mentre il secondo della proiettile fisico
    public Actor Owner { get; set; }
    public float Damage;
    [Range(0f, 100f)]
    public float Force;
    public float MaxRadius;
    public float ForceExolosion;
    private Dictionary<ArmorType, float> damageRates;
    [Range(0f, 1f)]
    public float LightArmorDamageRate;
    [Range(0f, 1f)]
    public float MediumArmorDamageRate;
    [Range(0f, 1f)]
    public float HeavyArmorDamageRate;
    private float m_fTotalDistance;

    private ParticlesController m_hParticlesController;
    private Rigidbody m_hRigidBody;
    private Vector3 m_vShootPosition;
    private GameObject target;
    
    public GameObject Target { get { return target; } set { target = value; } }

    /// /////////////////////////////////////////////
  
    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();
        damageRates = new Dictionary<ArmorType, float>();
        damageRates.Add(ArmorType.Light, LightArmorDamageRate);
        damageRates.Add(ArmorType.Medium, MediumArmorDamageRate);
        damageRates.Add(ArmorType.Heavy, HeavyArmorDamageRate);

    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection, Actor hOwner)
    {
        if (m_hParticlesController != null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward = vDirection;

        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.VelocityChange);

        m_vShootPosition = vPosition;
        Owner = hOwner;
    }


    void OnCollisionEnter(Collision collider)
    {

        Collider[] obj = Physics.OverlapSphere(collider.transform.position, MaxRadius);


        foreach (var Col in obj)
        {
            if (Col.GetComponent<Actor>() != null && !this)
            {

                Actor h_Hit = collider.gameObject.GetComponent<Actor>();

                this.Damage = AoeDamage(Col.transform.position, collider.transform.position);
                ArmorType armor = h_Hit.Armor;
                float rate = damageRates[armor];
                h_Hit.Damage(this);
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
        GlobalFactory.Recycle(this.gameObject);
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

    public float GetDamage(ArmorType armor)
    {
        throw new NotImplementedException();
    }
}



