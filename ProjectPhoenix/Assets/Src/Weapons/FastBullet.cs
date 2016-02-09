using UnityEngine;
using System.Collections;
using System;



public class FastBullet : MonoBehaviour, IBullet,IPoolable
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
        Vector3 forward = this.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(this.transform.position, forward, out hit, maxDistance))
        {
            int randomExplosion = UnityEngine.Random.Range(0, Particle.Length);

            GameObject effectParticles = (GameObject)Instantiate(Particle[randomExplosion],
                                          new Vector3(hit.point.x, hit.point.y, hit.point.z),
                                          Quaternion.identity);
            ApplyForce(10.0F);
            Debug.Log(Particle[randomExplosion].ToString());
        }
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
