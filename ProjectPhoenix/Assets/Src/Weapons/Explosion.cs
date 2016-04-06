using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    public GameObject target { get; set; }
    public bool FullExpanzion { get; private set; }

    SphereCollider Scollider;

    void Awake()
    {
        Scollider       = this.GetComponent<SphereCollider>();
        FullExpanzion   = false;
    }

    // Use this for initialization
	void Start ()
    {
        Debug.Log("Avvio Espolsione");
        //Scollider.enabled = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (FullExpanzion==true)
        {
            RetireExpolsion();
        }
        else
        {

        }
    }


    void  OnTriggerEnter(Collider collider)
    {  
        target = collider.gameObject;
    }

   public void ExendExpolsion()
    {
        Scollider.enabled = true;

        while (Scollider.radius<50f)
        {
        Debug.Log("Espanzione Esplosione");

        Scollider.radius += Mathf.Clamp(+10, 0f, 50f);
        this.transform.localScale = new Vector3(Scollider.radius,Scollider.radius,Scollider.radius);
        }
        FullExpanzion = true;
    }

    public void RetireExpolsion()
    {

        while (Scollider.radius > 0f)
        {
            Debug.Log("Ritiro Esplosione");

            Scollider.radius         -= Mathf.Clamp(+10, 0f, 50f);
            this.transform.localScale = new Vector3(Scollider.radius, Scollider.radius, Scollider.radius);
        }
      //  Scollider.enabled = false;
        FullExpanzion = false;
    }
}
