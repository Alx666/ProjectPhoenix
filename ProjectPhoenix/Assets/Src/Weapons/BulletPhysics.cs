using UnityEngine;
using System.Collections;

/// <summary>
/// Handles physics based bullets (OnCollisionEnter)
/// artillery shoots, granades, etc...
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletPhysics : MonoBehaviour, IBullet, IPoolable
{
    public float MaxDistance;
    public float Force;
    public float Damage;

    private Rigidbody m_hRigidBody;
    private Vector3 m_vShootPosition;

    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward  = vDirection;
        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.VelocityChange);
        m_vShootPosition = this.transform.position;
    }

    void OnTriggerEnter(Collider collider)
    {
        IDamageable hHit = collider.gameObject.GetComponent<IDamageable>();

        if (hHit != null)
        {
            hHit.Damage(Damage);
        }
        
        GlobalFactory.Recycle(this.gameObject);
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
        this.gameObject.SetActive(false);
        this.m_hRigidBody.velocity = Vector3.zero;
    }

    #endregion
}
