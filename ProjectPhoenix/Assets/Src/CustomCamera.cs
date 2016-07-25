using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
public class CustomCamera : MonoBehaviour
{
    public float MinOffset = 20.0f;
    public float MaxOffset = 40.0f;
    public KeyCode StateChanger = KeyCode.Mouse1;
    public float LerpSpeed = 10f;

    [Range(0, 1)]
    public float DistanceFromTarget = 0.3f;

    public GameObject Target { get; private set; }
    Vector3 Offset;
    Vector3 tempOffset;
    Vector3 stdOffset;
    Vector3 aimOffset;
    Vector3 startingOffset;
    float currentSpeed;

    Transform m_hDepthOfField;
    UnityStandardAssets.CinematicEffects.DepthOfField DoF;

    void Awake()
    {
        DoF = this.GetComponent<UnityStandardAssets.CinematicEffects.DepthOfField>();
        m_hDepthOfField = DoF.focus.transform;

        Target = this.transform.root.gameObject;

        InitialOffset();
        startingOffset = Offset;
    }


    void Start()
    {
        NetworkBehaviour hNetworked = Target.GetComponent<NetworkBehaviour>();
        if (!hNetworked.isLocalPlayer)
        {
            GameObject.Destroy(this.gameObject);
        }
    }


    void Update()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, Target.GetComponentInParent<Rigidbody>().velocity.magnitude, Time.deltaTime);
        m_hDepthOfField.position = Target.transform.position;
    }

    void LateUpdate()
    {
        tempOffset.y = Mathf.Clamp((MaxOffset / MinOffset) * currentSpeed, MinOffset, MaxOffset);

        // Modalita' "torretta"
        if (Input.GetKey(StateChanger) && Offset.y > tempOffset.y)
        {
            tempOffset.y = Offset.y;
        }

        stdOffset.x = (tempOffset.y * startingOffset.x) / startingOffset.y;
        stdOffset.z = (tempOffset.y * startingOffset.z) / startingOffset.y;

        if (!Input.GetKey(StateChanger))
        {
            tempOffset.x = stdOffset.x;
            tempOffset.z = stdOffset.z;
        }
        else
            AimOffset();

        Offset = Vector3.Lerp(Offset, tempOffset, Time.deltaTime * LerpSpeed);
        this.transform.position = Target.transform.position + Offset;
        this.transform.parent = null;
    }

    Vector3 InitialOffset()
    {
        RaycastHit ray;
        if (Physics.Raycast(this.transform.position, this.transform.forward, out ray))
        {
            tempOffset = this.transform.position - ray.point;
            Offset.y = MinOffset;
            Offset.x = (Offset.y * tempOffset.x) / tempOffset.y;
            Offset.z = (Offset.y * tempOffset.z) / tempOffset.y;
        }
        return Offset;
    }

    void AimOffset()
    {
        Camera myCamera = this.GetComponent<Camera>();
        Vector3 screenTarget = myCamera.WorldToScreenPoint(Target.transform.position);
        Vector3 BiDTarget = (Input.mousePosition - screenTarget) * DistanceFromTarget;
        Ray vRay;

        if ((screenTarget + BiDTarget).x < Screen.width && (screenTarget + BiDTarget).y < Screen.height)
        {
            vRay = myCamera.ScreenPointToRay(screenTarget + BiDTarget);
        }
        else
            vRay = myCamera.ScreenPointToRay(screenTarget);

        Plane vPlane = new Plane(Vector3.up, Target.transform.position);
        float fDistance;
        vPlane.Raycast(vRay, out fDistance);
        Vector3 vPoint = vRay.GetPoint(fDistance);

        aimOffset.x = stdOffset.x + vPoint.x - Target.transform.position.x;
        aimOffset.z = stdOffset.z + vPoint.z - Target.transform.position.z;

        tempOffset.x = aimOffset.x;
        tempOffset.z = aimOffset.z;

        SetDepthOfField();
    }

    void SetDepthOfField()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float DofIncrement;

        if (Physics.Raycast(ray, out hit))
        {
            DofIncrement = Mathf.Abs((new Vector3(hit.point.x, Target.transform.position.y, hit.point.z) - Target.transform.position).magnitude) / 5f;
            m_hDepthOfField.position = new Vector3(hit.point.x, Target.transform.position.y + DofIncrement, hit.point.z);
        }
    }
}