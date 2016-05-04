using UnityEngine;
using System.Collections;

public class CustomCamera : MonoBehaviour
{
    public GameObject Target;
    public float MinOffset;
    public float MaxOffset;
    public KeyCode StateChanger;

    [Range(0, 1)]
    public float DistanceFromTarget;

    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    Vector3 Offset;
    static Vector3 startingOffset;

    void Start()
    {
        InitialOffset();
        startingOffset = Offset;

        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
    }

    void Update()
    {
        if (!Input.GetKey(StateChanger))
            current = stdCamera;

        if (Input.GetKey(StateChanger))
            current = aimCamera;     
    }

    void LateUpdate()
    {
        current.CalculateOffset();

        this.transform.position = Target.transform.position + Offset;
    }

    Vector3 InitialOffset()
    {
        RaycastHit ray;
        Vector3 tempOffset;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out ray))
        {
            tempOffset = this.transform.position - ray.point;
            Offset.y = MinOffset;
            Offset.x = (Offset.y * tempOffset.x) / tempOffset.y;
            Offset.z = (Offset.y * tempOffset.z) / tempOffset.y;
        }

        return Offset;
    }

    interface ICameraState
    {
        void CalculateOffset();
    }
    class StandardCamera : ICameraState
    {
        CustomCamera camera;
        internal Vector3 stdOffset;

        public StandardCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            stdOffset = startingOffset;
        }

        public void CalculateOffset()
        {
            stdOffset.y = Mathf.Clamp(Mathf.Lerp(stdOffset.y, (camera.MaxOffset / camera.MinOffset) * camera.Target.GetComponentInParent<Rigidbody>().velocity.magnitude, Time.deltaTime), camera.MinOffset, camera.MaxOffset);
            stdOffset.x = (stdOffset.y * startingOffset.x) / startingOffset.y;
            stdOffset.z = (stdOffset.y * startingOffset.z) / startingOffset.y;
            camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, stdOffset.x, Time.deltaTime), Mathf.Lerp(camera.Offset.y, stdOffset.y, Time.deltaTime), Mathf.Lerp(camera.Offset.z, stdOffset.z, Time.deltaTime));
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera camera;      
        Vector3 screenAimTarget;
        internal Vector3 aimOffset;
        float screenDistance;
        float prop;

        public AimCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            aimOffset = camera.Offset;
            screenDistance = Mathf.Pow((Mathf.Pow(Screen.width, 2f) + Mathf.Pow(Screen.height, 2f)), 0.5f) / 2;
        }

        public void CalculateOffset()
        {
            prop = camera.stdCamera.stdOffset.magnitude / screenDistance;

            screenAimTarget.x = (Screen.width / 2 - Input.mousePosition.x) * camera.DistanceFromTarget;
            screenAimTarget.z = (Screen.height / 2 - Input.mousePosition.y) * camera.DistanceFromTarget;

            aimOffset.x = camera.stdCamera.stdOffset.x - (prop * screenAimTarget.x);
            aimOffset.z = camera.stdCamera.stdOffset.z - (prop * screenAimTarget.z);

            camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, aimOffset.x, Time.deltaTime), camera.Offset.y, Mathf.Lerp(camera.Offset.z, aimOffset.z, Time.deltaTime));
            
        }
    }
}

