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
    private ParticlesController m_hParticlesController;
    private SphereCollider m_hAOE ;

    void Awake()
    {
        m_hRigidBody = this.GetComponent<Rigidbody>();
        m_hParticlesController = this.GetComponentInChildren<ParticlesController>();
        m_hAOE = this.GetComponent<SphereCollider>();
    }

    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        if(m_hParticlesController!= null)
            m_hParticlesController.PlayMuzzleVfx(vPosition, vDirection);

        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward  = vDirection;
        m_hRigidBody.AddForce(this.gameObject.transform.forward * Force, ForceMode.Impulse);
        m_vShootPosition = this.transform.position;
    }

    void OnTriggerEnter(Collider collider)
    {

        //1/2 arcsin()
        //Attivo AOE
        this.m_hAOE.enabled = true;
        //

        IDamageable hHit = collider.gameObject.GetComponent<IDamageable>();

        //ToDo: play collision vfx  come prendo la normale da una trigger collision?
        
        //// non controllo piu se colpisco un Idamagable ma se colpisce(qualsiasi cosa) rilascia un AOE Che danneggia chi è in area
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
