using System.Collections.Generic;
using UnityEngine;

public class Test_ProviderAI : MonoBehaviour
{
    public List<GameObject> Targets;
    public GameObject POI;
    private List<IControllerAI> targets;

    void Start()
    {
        targets = new List<IControllerAI>();
        Targets.ForEach(hT => targets.Add(hT.GetComponent<IControllerAI>()));
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
                targets.ForEach(hT => 
                {
                    hT.Target = POI;
                    hT.Patrol();
                });
            }
        }
        
    }
}