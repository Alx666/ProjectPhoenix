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
    public float MaxDistance;


    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    LerpCamera lerpCamera;
    Vector3 Offset;
    static Vector3 startingOffset;

    void Awake()
    {
        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
        lerpCamera = new LerpCamera(this);
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

        if ((Offset.x != stdCamera.stdOffset.x || Offset.z != stdCamera.stdOffset.z) && stdCamera.stdOffset != Vector3.zero)
            current = lerpCamera;

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
            camera.Offset = stdOffset;
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera camera;
        internal Vector3 aimOffset;

        public AimCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            aimOffset = camera.Offset;
        }

        public void CalculateOffset()
        {
            if (Input.GetKey(camera.StateChanger))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit vHit;

                if (Physics.Raycast(ray, out vHit))
                {
                    aimOffset = camera.Offset + (camera.DistanceFromTarget * (vHit.point - camera.Target.transform.position));

                    if ((camera.DistanceFromTarget * (vHit.point - camera.Target.transform.position)).magnitude < camera.MaxDistance)
                    {
                        camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, aimOffset.x, Time.deltaTime), camera.Offset.y, Mathf.Lerp(camera.Offset.z, aimOffset.z, Time.deltaTime));
                    }
                }
            }
        }
    }

    class LerpCamera : ICameraState
    {
        CustomCamera camera;
        internal Vector3 lerpOffset;

        public LerpCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            lerpOffset = camera.Offset;
        }

        public void CalculateOffset()
        {
            lerpOffset = camera.stdCamera.stdOffset;
            camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, lerpOffset.x, Time.deltaTime), camera.Offset.y, Mathf.Lerp(camera.Offset.z, lerpOffset.z, Time.deltaTime));
        }
    }
}

