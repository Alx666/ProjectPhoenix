using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BulletTracking : NetworkBehaviour, IBullet, IPoolable
{
    protected GameObject target;
    protected float ElapsedTime;

    public float Damage;
    public Actor Owner { get; set; }

    ParticlesController controller;
    FollowTarget follow;
    NoTarget noTarget;
    ITrackingMode current;
    [Range(3f, 5f)]
    public float Duration = 0f;
    [Range(1f, 5f)]
    public float Force = 2f;

    private Dictionary<ArmorType, float> damageRates;
    [Range(0f, 1f)]
    public float LightArmorDamageRate;
    [Range(0f, 1f)]
    public float MediumArmorDamageRate;
    [Range(0f, 1f)]
    public float HeavyArmorDamageRate;

    void Awake()
    {
        this.controller = GetComponent<ParticlesController>();
        this.follow = new FollowTarget(this);
        this.noTarget = new NoTarget(this);
        this.gameObject.SetActive(false);

        damageRates = new Dictionary<ArmorType, float>();
        damageRates.Add(ArmorType.Light, LightArmorDamageRate);
        damageRates.Add(ArmorType.Medium, MediumArmorDamageRate);
        damageRates.Add(ArmorType.Heavy, HeavyArmorDamageRate);
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection, Actor hOwner)
    {
        Owner = hOwner;
        this.gameObject.transform.position = vPosition;
        RaycastHit vRaycast;
        if (Physics.Raycast(vPosition, vDirection, out vRaycast))
        {
            IDamageable hHit = vRaycast.collider.gameObject.GetComponent<IDamageable>();
            if (hHit != null)
            {
                this.target = vRaycast.collider.gameObject;
                this.gameObject.transform.forward = (vRaycast.collider.gameObject.transform.position - this.transform.position).normalized;
                this.current = follow;

                hHit.Damage(this);
                return;
            }
        }

        this.gameObject.transform.forward = vDirection;
        this.current = noTarget;
    }

    public void OnTriggerEnter(Collider other)
    {
        Explosion();
    }
    protected void Explosion()
    {
        this.controller.PlayHitVfx(this.transform.position, this.transform.up);
        GlobalFactory.Recycle(this.gameObject);
    }
    void FixedUpdate()
    {
        current.Track();
    }
    private interface ITrackingMode
    {
        void Track();
    }
    private class FollowTarget : ITrackingMode
    {
        BulletTracking own;
        public FollowTarget(BulletTracking own)
        {
            this.own = own;
        }
        public void Track()
        {
            own.ElapsedTime += Time.fixedDeltaTime;
            if (own.ElapsedTime <= own.Duration)
            {
                own.transform.position = Vector3.Lerp(own.transform.position, own.target.transform.position, own.Force * Time.fixedDeltaTime * own.ElapsedTime);
                own.transform.LookAt(own.target.transform);
            }
            else
            {
                own.Explosion();
            }
        }
    }
    private class NoTarget : ITrackingMode
    {
        BulletTracking own;
        public NoTarget(BulletTracking own)
        {
            this.own = own;
        }
        public void Track()
        {
            own.ElapsedTime += Time.fixedDeltaTime;
            if (own.ElapsedTime <= own.Duration)
            {
                own.transform.position += own.transform.forward * own.Force * Time.fixedDeltaTime * own.ElapsedTime;
            }
            else
            {
                own.Explosion();
            }
        }
    }
    #region IPoolable
    public Pool Pool { get; set; }

    public void Disable()
    {
        this.ElapsedTime = 0f;
        this.gameObject.SetActive(false);

    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public float GetDamage(ArmorType armor)
    {
        throw new NotImplementedException();
    }
    #endregion
}
