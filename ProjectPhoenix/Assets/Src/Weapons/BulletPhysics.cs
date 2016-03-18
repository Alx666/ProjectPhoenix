using UnityEngine;
using System.Collections;


/// <summary>
/// Handles physics based bullets (OnCollisionEnter)
/// artillery shoots, granades, etc...
/// </summary>
public class BulletPhysics : MonoBehaviour, IBullet, IPoolable
{
    public float MaxDistance;
    public float Force;

    private Rigidbody m_hRigidBody;

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

    void OnCollisionEnter(Collision collision)
    {

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
