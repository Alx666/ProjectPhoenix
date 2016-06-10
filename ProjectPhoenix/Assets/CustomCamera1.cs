using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CustomCamera1 : MonoBehaviour
{
    //[HideInInspector]
    public GameObject Target;


    float MinOffset;
    float MaxOffset;
    KeyCode StateChanger;
    float DistanceFromTarget;
    Vector3 rotation;
    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    Vector3 Offset;
    Vector3 startingOffset;

    void Awake()
    {
        MinOffset = 20f;
        MaxOffset = 50f;
        StateChanger = KeyCode.Mouse1;
        DistanceFromTarget = 0.3f;
        this.enabled = false;     // <=
    }

    void OnEnable() //usato al posto dello Start()
    {
        rotation = new Vector3(45f, 45f, 0f);
        this.transform.eulerAngles = rotation;
        InitialOffset();
        startingOffset = Offset;

        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
        current = stdCamera;  //c'e' un frame in cui non e' settato; serve solo per evitare di sollevare un'eccezione
    }

    void Update()
    {
        if (!Input.GetKey(StateChanger))
            current = stdCamera;

        else
            current = aimCamera;
    }

    void LateUpdate()
    {
        current.CalculateOffset();

        this.transform.parent = null;
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
        CustomCamera1 camera;
        internal Vector3 stdOffset;

        public StandardCamera(CustomCamera1 MyCamera)
        {
            camera = MyCamera;
            stdOffset = camera.startingOffset;
        }

        public void CalculateOffset()
        {
            stdOffset.y = Mathf.Clamp(Mathf.Lerp(stdOffset.y, (camera.MaxOffset / camera.MinOffset) * camera.Target.GetComponentInParent<Rigidbody>().velocity.magnitude, Time.deltaTime), camera.MinOffset, camera.MaxOffset);
            stdOffset.x = (stdOffset.y * camera.startingOffset.x) / camera.startingOffset.y;
            stdOffset.z = (stdOffset.y * camera.startingOffset.z) / camera.startingOffset.y;
            camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, stdOffset.x, Time.deltaTime), Mathf.Lerp(camera.Offset.y, stdOffset.y, Time.deltaTime), Mathf.Lerp(camera.Offset.z, stdOffset.z, Time.deltaTime));
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera1 camera;
        internal Vector3 aimOffset;

        public AimCamera(CustomCamera1 MyCamera)
        {
            camera = MyCamera;
            aimOffset = camera.Offset;
        }

        public void CalculateOffset()
        {
            Vector3 screenTarget = camera.GetComponent<Camera>().WorldToScreenPoint(camera.Target.transform.position);
            Vector3 BiDTarget = (Input.mousePosition - screenTarget) * camera.DistanceFromTarget;
            Ray vRay;

            if ((screenTarget + BiDTarget).x < Screen.width && (screenTarget + BiDTarget).y < Screen.height)
            {
                vRay = camera.GetComponent<Camera>().ScreenPointToRay(screenTarget + BiDTarget);
            }
            else
                vRay = camera.GetComponent<Camera>().ScreenPointToRay(screenTarget);
                
            Plane vPlane = new Plane(Vector3.up, camera.Target.transform.position);
            float fDistance;
            vPlane.Raycast(vRay, out fDistance);
            Vector3 vPoint = vRay.GetPoint(fDistance);
        

            aimOffset.x = camera.stdCamera.stdOffset.x + vPoint.x - camera.Target.transform.position.x;
            aimOffset.z = camera.stdCamera.stdOffset.z + vPoint.z - camera.Target.transform.position.z;

            camera.Offset = new Vector3(Mathf.Lerp(camera.Offset.x, aimOffset.x, Time.deltaTime), camera.Offset.y, Mathf.Lerp(camera.Offset.z, aimOffset.z, Time.deltaTime));
        }
    }
}

