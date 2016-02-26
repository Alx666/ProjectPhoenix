using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class VehicleTester : MonoBehaviour
{
    private List<GameObject> Targets;
    private GameObject currentTarget;

    public GameObject Camera;
    public float CameraSpeed { get; private set; }
    public float CameraTolerance { get; private set; }

    public void Awake()
    {
        Targets = new List<GameObject>();

        CameraSpeed = 10f;
        CameraTolerance = 1f;
    }

    public void Start()
    {
        Targets = FindObjectsOfType<GameObject>().Where(GO => GO.GetComponent<IControllerPlayer>() != null).ToList();

        if (Targets[0] != null)
        {
            Targets.ForEach(GO => GO.GetComponent<InputProviderPCStd>().enabled = false);
            Targets[0].GetComponent<InputProviderPCStd>().enabled = true;
            currentTarget = Targets[0];
        }
    }

    public void Update()
    {
        InputUpdate();
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 target = currentTarget.transform.position;
        Camera.transform.position = new Vector3(target.x, Camera.transform.position.y, target.z);
    }

    private void InputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Targets[0] != null)
            {
                Targets.ForEach(GO => GO.GetComponent<InputProviderPCStd>().enabled = false);
                Targets[0].GetComponent<InputProviderPCStd>().enabled = true;
                currentTarget = Targets[0];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (Targets[1] != null)
            {
                Targets.ForEach(GO => GO.GetComponent<InputProviderPCStd>().enabled = false);
                Targets[1].GetComponent<InputProviderPCStd>().enabled = true;
                currentTarget = Targets[1];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (Targets[2] != null)
            {
                Targets.ForEach(GO => GO.GetComponent<InputProviderPCStd>().enabled = false);
                Targets[2].GetComponent<InputProviderPCStd>().enabled = true;
                currentTarget = Targets[2];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (Targets[3] != null)
            {
                Targets.ForEach(GO => GO.GetComponent<InputProviderPCStd>().enabled = false);
                Targets[3].GetComponent<InputProviderPCStd>().enabled = true;
                currentTarget = Targets[3];
            }
        }
    }
}