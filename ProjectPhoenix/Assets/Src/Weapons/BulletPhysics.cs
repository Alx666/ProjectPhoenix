using UnityEngine;
using System.Collections;

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

    private Rigidbody m_hRigidBody;
    private float m_fTotalDistance;

    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward  = vDirection;

        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.VelocityChange);
    }

    void OnTriggerEnter(Collider collider)
    {
        IDamageable hHit = collider.gameObject.GetComponent<IDamageable>();
        if (hHit != null)
        {
            hHit.Damage(Damage);
        }
        else
        {

        }
        this.m_hRigidBody.velocity = Vector3.zero;
        GlobalFactory.Recycle(this.gameObject);
    }
    void Update()
    {
        m_fTotalDistance = this.transform.position.magnitude;

        if (m_fTotalDistance > MaxDistance)
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
        this.gameObject.SetActive(false);
    }

    #endregion
}
