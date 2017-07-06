using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class VehicleTester : MonoBehaviour
{
    [SerializeField]
    internal List<GameObject> InputReceivers = new List<GameObject>();

    void Awake()
    {
        InputReceivers.ForEach(hGO => hGO.GetComponent<InputProviderPCStd>().enabled = false);
        InputReceivers.First().GetComponent<InputProviderPCStd>().enabled = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                InputProviderPCStd provider = hit.collider.transform.root.GetComponent<InputProviderPCStd>();
                if (provider && InputReceivers.Contains(provider.gameObject))
                {
                    Debug.Log(hit.transform.root.gameObject.name + " Selected!");

                    InputReceivers.Where(hGO => hGO != provider.gameObject).ToList().ForEach(hGO => hGO.GetComponent<InputProviderPCStd>().enabled = false);
                    provider.enabled = true;
                }
            }
        }
    }
}