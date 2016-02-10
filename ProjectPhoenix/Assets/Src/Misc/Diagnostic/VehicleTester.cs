using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

internal class VehicleTester : MonoBehaviour
{
    public Camera MyCamera;
    public Vector3 Offset = new Vector3(-20f, 30f, -20f);

    private Rigidbody rB;

    [SerializeField]
    internal List<GameObject> InputReceivers;

	void Awake ()
    {
        InputReceivers.ForEach(hGO => hGO.GetComponent<InputProviderPCStd>().enabled = false);
        InputReceivers.First().GetComponent<InputProviderPCStd>().enabled = true;
    }
	
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                InputProviderPCStd provider = hit.collider.transform.root.GetComponent<InputProviderPCStd>();
                if (provider && InputReceivers.Contains(provider.gameObject))
                {
                    Debug.Log(hit.transform.root.gameObject.name + " Selected!");

                    InputReceivers.Where(hGO => hGO != provider.gameObject).ToList().ForEach(hGO => hGO.GetComponent<InputProviderPCStd>().enabled = false);
                    provider.enabled = true;

                    rB = provider.GetComponentInParent<Rigidbody>();
                }
            }
        }
	}

    void LateUpdate()
    {
        if(rB != null)
            this.MyCamera.transform.position = rB.transform.position + Offset;
        else
            this.MyCamera.transform.position = InputReceivers[0].transform.position + Offset;
    }
}
