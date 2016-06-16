using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
public class CustomCamera : MonoBehaviour
{

    public float MinOffset = 20.0f;
    public float MaxOffset = 40.0f;
    public KeyCode StateChanger = KeyCode.Mouse1;


    [Range(0, 1)]
    public float DistanceFromTarget = 0.3f;


    GameObject Target;
    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    Vector3 Offset;
    Vector3 startingOffset;


    void Awake()
    {
        Target = this.transform.root.gameObject;

        InitialOffset();
        startingOffset = Offset;

        stdCamera = new StandardCamera(this);
        aimCamera = new AimCamera(this);
        current = stdCamera;
    }


    void Start()
    {
        NetworkBehaviour hNetworked = Target.GetComponent<NetworkBehaviour>();
        if(!hNetworked.isLocalPlayer)
        {
            GameObject.Destroy(this.gameObject);
        }
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
        CustomCamera camera;
        internal Vector3 stdOffset;

        public StandardCamera(CustomCamera MyCamera)
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
        CustomCamera camera;
        internal Vector3 aimOffset;

        public AimCamera(CustomCamera MyCamera)
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