using UnityEngine;
using System.Collections;

public class Test_Rocket : MonoBehaviour
{
    public float Velocity;
    public GameObject Cube;
    bool startRotate;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray vRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit vRaycast;
            if (Physics.Raycast(vRay, out vRaycast))
            {
                if (vRaycast.collider.gameObject == this.Cube)
                    this.startRotate = true;
            }
        }
        if (startRotate)
            this.transform.Rotate(Vector3.forward * Time.deltaTime * Velocity);
    }
}
