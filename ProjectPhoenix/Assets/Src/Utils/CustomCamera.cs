using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CustomCamera : MonoBehaviour
{
    private Vector3     m_vPlayerOffset;
    private Vector3     m_vDestination;
    private GameObject  m_hTarget;
    private Camera      m_hCamera;
    private Rigidbody   m_hTargetRb;

    [Range(0f, 1f)]
    public float       RelativePosition = 0.3f;

    [Range(1f, 10f)]
    public float       LerpSpeed        = 5f;

    [Range(0f, 1f)]
    public float       VelocityZoomCoeff = 10f;

    private void Awake()
    {
        m_hCamera = this.GetComponent<Camera>();        
    }

    private void Start()
    {
        m_hTarget               = this.transform.root.gameObject;
        m_vPlayerOffset         = this.transform.position - this.transform.root.position;
        this.transform.parent   = null;
        m_hTargetRb             = m_hTarget.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 vPOnScreen = m_hCamera.WorldToScreenPoint(m_hTarget.transform.position);
            vPOnScreen.z = 0f;

            Vector3 vMOnScreen = Input.mousePosition;
            Vector3 vDOnScreen = (vMOnScreen - vPOnScreen) * RelativePosition; //problem here

            Ray vRay = m_hCamera.ScreenPointToRay(vPOnScreen + vDOnScreen);

            Plane vPlane = new Plane(Vector3.up, m_hTarget.transform.position);
            float fDist;
            vPlane.Raycast(vRay, out fDist);
            Vector3 vPoint = vRay.GetPoint(fDist);

            m_vDestination = vPoint + m_vPlayerOffset;
        }
        else
        {
            m_vDestination = m_hTarget.transform.position + m_vPlayerOffset;
        }

       
        if (m_hTargetRb != null)
            m_vDestination -= m_hTargetRb.velocity.magnitude * this.transform.forward * VelocityZoomCoeff;

        
        this.transform.position = Vector3.Lerp(this.transform.position, m_vDestination, LerpSpeed * Time.deltaTime);
    }
}
