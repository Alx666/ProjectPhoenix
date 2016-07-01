using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
public class CustomCamera : MonoBehaviour
{
    public float MinOffset = 20.0f;
    public float MaxOffset = 40.0f;
    public KeyCode StateChanger = KeyCode.Mouse1;
    public float LerpSpeed = 10.0f;

    [Range(0, 1)]
    public float DistanceFromTarget = 0.3f;


    GameObject Target;
    ICameraState current;
    StandardCamera stdCamera;
    AimCamera aimCamera;
    Vector3 Offset;
    Vector3 startingOffset;

    Transform m_hDepthOfField;
    UnityStandardAssets.ImageEffects.DepthOfField DoF;

    void Awake()
    {
        DoF = this.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
        m_hDepthOfField = DoF.focalTransform;

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
        {
            current = stdCamera;
            //this.ClampToRoot();
        }
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
        float currentSpeed;
        internal Vector3 stdOffset;

        public StandardCamera(CustomCamera MyCamera)
        {
            camera = MyCamera;
            stdOffset = camera.startingOffset;
        }

        public void CalculateOffset()
        {
            currentSpeed = camera.Target.GetComponentInParent<Rigidbody>().velocity.magnitude;
            stdOffset.y = Mathf.Clamp(Mathf.Lerp(stdOffset.y, (camera.MaxOffset / camera.MinOffset) * currentSpeed, Time.deltaTime), camera.MinOffset, camera.MaxOffset);
            stdOffset.x = (stdOffset.y * camera.startingOffset.x) / camera.startingOffset.y;
            stdOffset.z = (stdOffset.y * camera.startingOffset.z) / camera.startingOffset.y;
            camera.Offset = Vector3.Lerp(camera.Offset, stdOffset, Time.deltaTime * camera.LerpSpeed);
        }
    }

    class AimCamera : ICameraState
    {
        CustomCamera camera;
        internal Vector3 aimOffset;
        Camera myCamera;

        public AimCamera(CustomCamera Camera)
        {
            camera = Camera;
            aimOffset = camera.Offset;
            myCamera = Camera.GetComponent<Camera>();
        }


        public void CalculateOffset()
        {
            Vector3 screenTarget = myCamera.WorldToScreenPoint(camera.Target.transform.position);
            Vector3 BiDTarget = (Input.mousePosition - screenTarget) * camera.DistanceFromTarget;
            Ray vRay;

            if ((screenTarget + BiDTarget).x < Screen.width && (screenTarget + BiDTarget).y < Screen.height)
            {
                vRay = myCamera.ScreenPointToRay(screenTarget + BiDTarget);
            }
            else
                vRay = myCamera.ScreenPointToRay(screenTarget);

            Plane vPlane = new Plane(Vector3.up, camera.Target.transform.position);
            float fDistance;
            vPlane.Raycast(vRay, out fDistance);
            Vector3 vPoint = vRay.GetPoint(fDistance);
            
            aimOffset = camera.stdCamera.stdOffset + vPoint - camera.Target.transform.position;
            aimOffset.y = camera.Offset.y;

            camera.Offset = Vector3.Lerp(camera.Offset, aimOffset, Time.deltaTime * camera.LerpSpeed);
        }
    }
    void SetDepthOfField()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit))
        //{
        //    m_hDepthOfField.position = new Vector3(Mathf.Lerp(m_hDepthOfField.position.x, hit.point.x, Time.deltaTime), m_hDepthOfField.position.y, Mathf.Lerp(m_hDepthOfField.position.z, hit.point.z, Time.deltaTime));
        //}
    }

    void ClampToRoot()
    {
        m_hDepthOfField.position = this.Target.transform.position;
    }
}