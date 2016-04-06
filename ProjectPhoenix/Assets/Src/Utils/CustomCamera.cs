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
        Debug.DrawLine(this.transform.position, this.transform.forward * 20f, Color.red);
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
                    //camera.Offset = startingOffset;
                    //distanceFromTarget = new Vector3(vHit.point.x, camera.Target.transform.position.y, vHit.point.z);//new Ray(camera.Target.transform.position, camera.AimOffset * (vHit.point - camera.Target.transform.position));
                    //TargetAim.x = ((vHit.point.x - camera.Offset.x) * camera.AimOffset) - savedOffset.x;
                    //TargetAim.z = ((vHit.point.z - camera.Offset.z) * camera.AimOffset) - savedOffset.z;
                    camera.Offset.x = savedCamera.Offset.x + ((vHit.point.x - (camera.Target.transform.position.x - savedCamera.Target.transform.position.x)) * camera.AimOffset) * Time.deltaTime;
                    camera.Offset.z = savedCamera.Offset.z + ((vHit.point.z - (camera.Target.transform.position.z - savedCamera.Target.transform.position.z)) * camera.AimOffset) * Time.deltaTime;
                    camera.Offset = savedCamera.Offset;

                    cameraIsSaved = false;
                    //Debug.DrawRay(camera.Target.transform.position * camera.AimOffset, distanceFromTarget, Color.red);
                    //Debug.DrawRay(ray.origin, ray.direction * vHit.distance, Color.blue);
                    //
                }




                //    //distanceFromTarget = ray.point - target.position;
                //    //Offset = distanceFromTarget.normalized * Mathf.LerpAngle(0f, distanceFromTarget.magnitude, Time.deltaTime);
                //    //Offset.y = savedOffset.y;
                //    //-------------------------------------------------------------
                //    //mousePos.z = 10F;
                //    //Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                //    //player2.transform.position = worldPos;


                //    //Vector3 mid = player2.transform.position - player1.transform.position;
                //    //middlePoint = player1.transform.position + 0.5f * mid;

                //    //this.transform.position = new Vector3(middlePoint.x, 0, -10);
            }
            //if (Input.GetKeyUp(camera.StateChanger))
            //{
            //    camera.Offset = savedOffset;
            //    offsetIsSaved = false;
            //}
        }
    }
}

