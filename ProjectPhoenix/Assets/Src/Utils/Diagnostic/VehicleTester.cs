using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class VehicleTester : MonoBehaviour
{
    public Camera camera;
    public Vector3 Offset;
    public float MaxOffset;
    public float MinOffset;

    CustomCamera MyCamera;
    Rigidbody rB;
    bool LogWritten;

    [SerializeField]
    internal List<GameObject> InputReceivers;

    void Awake()
    {
        InputReceivers.ForEach(hGO => hGO.GetComponent<InputProviderPCStd>().enabled = false);
        InputReceivers.First().GetComponent<InputProviderPCStd>().enabled = true;


        LogWritten = true;
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

                    MyCamera = new CustomCamera(camera, provider.GetComponentInParent<Rigidbody>(), Offset);
                }
            }
        }
    }

    void LateUpdate()
    {
        try
        {
            MyCamera.ZoomOnTarget(MinOffset, MaxOffset);
            //MyCamera.AimHelper(Input.mousePosition, KeyCode.Mouse1);
        }
        catch
        {
            if (LogWritten == true)
            {
                Debug.Log("No Player Selected");
                LogWritten = false;
            }
        }
    }
}