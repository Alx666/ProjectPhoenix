using UnityEngine;
using System.Collections;

public class CustomCamera : MonoBehaviour
{
    public GameObject Target;
    public Vector3 Offset;
    public float MinOffset;
    public float MaxOffset;
    public KeyCode StateChanger;

    ICameraState current;
    Rigidbody rB;
    StandardCamera stdCamera;
    AimCamera aimCamera;

    void Awake()
    {
        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
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
        float MaxOffset;
        float yOffsetVar;
        float targetMaxSpeed;

        public StandardCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            rB = MyCamera.Target.GetComponentInParent<Rigidbody>();
            MinOffset = MyCamera.MinOffset;
            MaxOffset = MyCamera.MaxOffset;
            Offset = camera.Offset;
        }

        public void CalculateOffset()
        {
            Offset.y = Mathf.Lerp(Offset.y, (MaxOffset / MinOffset) * rB.velocity.magnitude, Time.deltaTime);

            if (Offset.y < MinOffset)
            {
                Offset.y = MinOffset;
            }
            if (Offset.y > MaxOffset)
            {
                Offset.y = MaxOffset;
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

            if (!offsetIsSaved && Input.GetKey(key))
            {
                savedOffset = Offset;
                offsetIsSaved = true;
            }

            if (Input.GetKey(key))
            {
                RaycastHit ray;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ray))
                {
                    distanceFromTarget = ray.point - target.position;
                    Offset = distanceFromTarget.normalized * Mathf.LerpAngle(0f, distanceFromTarget.magnitude, Time.deltaTime);
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

