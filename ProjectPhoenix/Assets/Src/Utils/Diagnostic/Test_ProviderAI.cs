using UnityEngine;

public class Test_ProviderAI : MonoBehaviour
{
    public GameObject Target;
    public GameObject POI;
    private IControllerAI target;

    void Start()
    {
        target = Target.GetComponent<IControllerAI>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                Vector3 vPosition = hit.point;
                //vPosition.x = 0f;
                //vPosition.y = 0.51f;
                POI.transform.position = vPosition;
                target.Target = POI;
            }
        }
        
    }
}