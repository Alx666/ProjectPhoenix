using UnityEngine;
using System.Collections;

public class SlowBullet : MonoBehaviour,IBullet,IPoolable
{

    public Rigidbody Rigidbody { get; private set; }
    public float maxDistance;
    public GameObject[] Particle;
    private RaycastHit hit;

    void Awake()
    {
        this.Rigidbody = this.GetComponent<Rigidbody>();
    }

    void Start()
    {
    }
    public Transform Transform
    {
        get
        {
            return this.transform;
        }
    }


    public void Shoot(Vector3 vPosition, Vector3 vDirection, float fForce, ForceMode eMode)
    {
        this.gameObject.transform.position = vPosition;
        this.gameObject.transform.forward = vDirection;
        this.Rigidbody.AddForce(this.gameObject.transform.forward * fForce, eMode);



    }

    void Update()
    {
       
    }

    void OnCollisionEnter(Collision collision)
    {
        int randomExplosion = UnityEngine.Random.Range(0, Particle.Length);
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;
        GameObject effectParticles = (GameObject)Instantiate(Particle[randomExplosion],
                                      new Vector3(contact.point.x, contact.point.y, contact.point.z),
                                      rot);
        Destroy(this.gameObject);
    }


    void ApplyForce(float force)
    {
        if (hit.rigidbody != null)
            hit.rigidbody.AddForceAtPosition(transform.forward * force, hit.point, ForceMode.VelocityChange);
    }
    public Pool Pool { get; set; }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
}
