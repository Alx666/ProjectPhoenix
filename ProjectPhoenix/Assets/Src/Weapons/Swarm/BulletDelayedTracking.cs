using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ParticlesController))]
public class BulletDelayedTracking : MonoBehaviour, IBullet, IPoolable
{
    public Actor Owner { get; set; }
    public float IdleTime = 1f;
    public float LaunchVelocity = 3f;
    public float VelocityIncrement = 10f; 
    public GameObject FireEffect;
    public float Damage;
    private Dictionary<ArmorType, float> damageRates;
    [Range(0f, 1f)]
    public float LightArmorDamageRate;
    [Range(0f, 1f)]
    public float MediumArmorDamageRate;
    [Range(0f, 1f)]
    public float HeavyArmorDamageRate;

    private Rigidbody m_hRigidbody;
    private Collider  m_hCollider;
    private DelayState m_hDelay;
    private ParticlesController m_hParticlesController;

    private IState m_hCurrent;

    void Awake()
    {
        m_hRigidbody = this.GetComponent<Rigidbody>();
        m_hCollider  = this.GetComponent<Collider>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();

        m_hDelay = new DelayState(this);
        FlyState hFly = new FlyState(this);
        m_hDelay.Next = hFly;

        m_hCurrent = m_hDelay;

        damageRates = new Dictionary<ArmorType, float>();
        damageRates.Add(ArmorType.Light, LightArmorDamageRate);
        damageRates.Add(ArmorType.Medium, MediumArmorDamageRate);
        damageRates.Add(ArmorType.Heavy, HeavyArmorDamageRate);
    }

	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_hCurrent = m_hCurrent.Update();
    }

    void FixedUpdate()
    {
        m_hCurrent.OnFixedUpdate();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection, Actor hOwner)
    {
        Owner = hOwner;
        this.transform.position = vPosition;
        this.transform.forward = vWDirection;
        m_hRigidbody.AddForce(vDirection * LaunchVelocity, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collider)
    {
        if (collider.gameObject.GetComponent<BulletDelayedTracking>() != null)
            return;

        Actor hHit = collider.gameObject.GetComponent<Actor>();
        m_hParticlesController.PlayHitVfx(this.transform.position, this.transform.up);

        ArmorType armor = hHit.Armor;
        float rate = damageRates[armor];
        hHit.Damage(this);

        GlobalFactory.Recycle(this.gameObject);
    }

    public void Enable()
    {
        FireEffect.SetActive(false);
        m_hDelay.Reset();
        m_hCurrent = m_hDelay;
        this.gameObject.SetActive(true);
        m_hRigidbody.velocity = Vector3.zero;
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    public float GetDamage(ArmorType armor)
    {
        throw new NotImplementedException();
    }

    public Pool Pool { get; set; }


    private interface IState
    {
        void OnFixedUpdate();
        IState Update();
    }

    private class DelayState : IState
    {
        private BulletDelayedTracking m_hOwner;
        private float m_fTimer;

        public IState Next { get; set; }

        public DelayState(BulletDelayedTracking hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnFixedUpdate()
        {
        }

        public IState Update()
        {
            if (m_fTimer <= 0)
            {
                m_hOwner.FireEffect.SetActive(true);
                return Next;
            }
            else
            {
                m_fTimer -= Time.deltaTime;
                return this;
            }
        }

        public void Reset()
        {
            m_fTimer = m_hOwner.IdleTime;
        }
    }

    private class FlyState : IState
    {
        private BulletDelayedTracking m_hOwner;

        public IState Next { get; set; }

        public FlyState(BulletDelayedTracking hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnFixedUpdate()
        {
            m_hOwner.m_hRigidbody.AddForce(m_hOwner.transform.forward * m_hOwner.VelocityIncrement, ForceMode.Acceleration);
        }

        public IState Update()
        {
            return this;
        }
    }
}
