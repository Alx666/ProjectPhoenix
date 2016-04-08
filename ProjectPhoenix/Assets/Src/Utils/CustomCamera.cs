using UnityEngine;
using System.Collections;

public class CustomCamera : MonoBehaviour
{
    public GameObject Target;
    public float MinOffset;
    public float MaxOffset;
    public KeyCode StateChanger;

    [Range(0, 1)]
    public float AimOffset;


    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    Vector3 Offset;
    static Vector3 startingOffset;

    void Awake()
    {
        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
    }

    void Start()
    {
        InitialOffset();
        startingOffset = Offset;
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

        this.transform.position = Target.GetComponent<Rigidbody>().transform.position + Offset;
    }

    Vector3 InitialOffset()
    {
        RaycastHit ray;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out ray))
        {
            Offset = this.transform.position - ray.point;
        }

        Offset = Offset.normalized * MinOffset;

        return Offset;
    }

    interface ICameraState
    {
        void CalculateOffset();
    }
    class StandardCamera : ICameraState
    {
        CustomCamera camera;

        public StandardCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
        }

        public void CalculateOffset()
        {
            camera.Offset.y = Mathf.Clamp(Mathf.Lerp(camera.Offset.y, (camera.MaxOffset / camera.MinOffset) * camera.Target.GetComponentInParent<Rigidbody>().velocity.magnitude, Time.deltaTime), camera.MinOffset, camera.MaxOffset);
            camera.Offset.x = (camera.Offset.y * startingOffset.x) / startingOffset.y;
            camera.Offset.z = (camera.Offset.y * startingOffset.z) / startingOffset.y;
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera camera;
        Vector3 TargetAim;
        CustomCamera savedCamera;
        bool cameraIsSaved;
        

        public AimCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
        }

        public void CalculateOffset()
        {
            if (Input.GetKey(camera.StateChanger))
            {
                if (!cameraIsSaved)
                {
                    savedCamera = camera;
                    cameraIsSaved = true;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit vHit;

                if (Physics.Raycast(ray, out vHit))
                {
                        TargetAim = camera.Offset + (camera.AimOffset * (vHit.point - camera.Target.transform.position));
                        camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, TargetAim.x, Time.deltaTime), camera.Offset.y, Mathf.Lerp(camera.Offset.z, TargetAim.z, Time.deltaTime));
                }
            }
        }
    }
}

