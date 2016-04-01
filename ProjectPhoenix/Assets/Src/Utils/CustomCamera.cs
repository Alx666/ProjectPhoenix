using UnityEngine;
using System.Collections;

public class CustomCamera : MonoBehaviour
{
    public GameObject Target;
    public Vector3 Offset;
    public float MinOffset;
    public KeyCode StateChanger;

    ICameraState current;
    Rigidbody rB;
    StandardCamera stdCamera;
    AimCamera aimCamera;

    void Awake()
    {
        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
        StateChanger = KeyCode.Mouse1;
        rB = Target.GetComponentInParent<Rigidbody>();

        if (Offset.y != MinOffset)
        {
            Offset.y = MinOffset;
        }
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

        this.transform.position = rB.transform.position + Offset;
    }

    interface ICameraState
    {
        void CalculateOffset();
    }
    class StandardCamera : ICameraState
    {
        CustomCamera camera;
        Rigidbody rB;
        Vector3 Offset;
        float MinOffset;

        public StandardCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            rB = MyCamera.Target.GetComponentInParent<Rigidbody>();
            MinOffset = MyCamera.MinOffset;
            Offset = camera.Offset;
        }

        public void CalculateOffset()
        {            
            Offset = new Vector3(Offset.x, Mathf.Lerp(Offset.y, rB.velocity.magnitude, Time.deltaTime), Offset.z);

            if (Offset.y < MinOffset)
            {
                Offset = new Vector3(Offset.x, MinOffset, Offset.z);
            }

            camera.Offset = Offset;
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera camera;
        Transform target;
        Rigidbody rB;
        KeyCode key;

        Vector3 Offset;
        bool offsetIsSaved;
        Vector3 savedOffset;

        public AimCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            rB = MyCamera.Target.GetComponentInParent<Rigidbody>();
            target = MyCamera.Target.GetComponentInParent<Transform>();
            key = MyCamera.StateChanger;
            Offset = camera.Offset;
        }

        public void CalculateOffset()
        {
            Vector3 distanceFromTarget;
            RaycastHit ray;

            if (!offsetIsSaved && Input.GetKey(key))
            {
                savedOffset = Offset;
                offsetIsSaved = true;
            }

            if (Input.GetKey(key))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ray))
                {
                    distanceFromTarget = new Vector3(ray.point.x - target.position.x, 0f, ray.point.z - target.position.z);
                    //Offset = new Vector3(Mathf.LerpAngle(savedOffset.x, distanceFromTarget.x, Time.deltaTime), savedOffset.y, Mathf.LerpAngle(savedOffset.z, distanceFromTarget.z, Time.deltaTime));
                    Offset.x = savedOffset.x + Mathf.LerpAngle(savedOffset.x, distanceFromTarget.x, Time.deltaTime);
                    Offset.z = savedOffset.z + Mathf.LerpAngle(savedOffset.z, distanceFromTarget.z, Time.deltaTime);
                    Offset.y = savedOffset.y;
                }
            }

            if (Input.GetKeyUp(key))
            {
                Offset = savedOffset;
                offsetIsSaved = false;
            }

            camera.Offset = Offset;
        }
    }
}
