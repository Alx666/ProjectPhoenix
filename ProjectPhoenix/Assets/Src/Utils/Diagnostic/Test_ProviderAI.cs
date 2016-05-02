using System.Collections.Generic;
using UnityEngine;

public class Test_ProviderAI : MonoBehaviour
{
    public GameObject testplayer;
    public List<GameObject> Targets;
    private List<IControllerAI> targets;

    void Start()
    {
        targets = new List<IControllerAI>();
        Targets.ForEach(hT => targets.Add(hT.GetComponent<IControllerAI>()));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targets.ForEach(hT => hT.Target = testplayer);
            targets.ForEach(hT => hT.Patrol());
        }
        
    }
}