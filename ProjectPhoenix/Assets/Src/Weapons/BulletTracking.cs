using UnityEngine;
using System.Collections;
using System;

public class BulletTracking : MonoBehaviour, IBullet, IPoolable
{
    protected GameObject target;
    protected float ElapsedTime;

    ParticlesController controller;
    FollowTarget follow;
    NoTarget noTarget;
    ITrackingMode current;
    [Range(3f, 5f)]
    public float Duration = 0f;
    [Range(1f, 5f)]
    public float Force = 2f;
    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
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
                return;
            }
        }

        this.gameObject.transform.forward = vDirection;
        this.current = noTarget;
    }
    void Awake()
    {
        this.controller = GetComponent<ParticlesController>();
        this.follow = new FollowTarget(this);
        this.noTarget = new NoTarget(this);
        this.gameObject.SetActive(false);
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
    #endregion
}
