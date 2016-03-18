using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(CCDIK))]
public class IKLeg : MonoBehaviour
{
    public GameObject FrontLocator;
    public GameObject RearLocator;
    public GameObject CenterLocator;
    public GameObject UpLocator;

    private IKSolver                m_hIK;
    private ControllerPlayerMech    m_hController;
    private IKMech000Foot           m_hFoot;

    public Vector3 IKPosition       { get { return m_hIK.GetIKPosition(); } }
    public bool NeedFrontReposition { get { return Vector3.Distance(m_hIK.IKPosition, FrontLocator.transform.position) > m_hController.StepDistance; } }
    public bool NeedBackReposition  { get { return Vector3.Distance(m_hIK.IKPosition, RearLocator.transform.position)  > m_hController.StepDistance; } }
    public bool IsRepositioning     { get; private set; }
    public float RepositioningTime  { get; set; }

    void Start ()
    {
        m_hIK         = this.GetComponent<CCDIK>().GetIKSolver();
        m_hController = this.transform.root.GetComponent<ControllerPlayerMech>();
        m_hFoot       = this.GetComponent<IKMech000Foot>();
    }


    private void OnUpdateVector3(Vector3 vValue)
    {
        m_hIK.SetIKPosition(vValue);
    }

    private void OnForwardUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), FrontLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnLegDownComplete);
    }

    private void OnBackwardUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), RearLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnLegDownComplete);
    }

    private void OnCenterUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), CenterLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnLegDownComplete);
    }

    private void OnLegDownComplete()
    {
        IsRepositioning = false;

        m_hController.EndReposition(this);
    }

    public void BeginRepositionFront()
    {
        IsRepositioning = true;
        Debug.Log(this.gameObject +" "+ m_hIK.GetIKPosition() + " " + UpLocator.transform.position + " " + m_hController.RepositioningTime);        
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), UpLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnForwardUpComplete);
    }

    public void BeginRepositionRear()
    {
        IsRepositioning = true;
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), UpLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnBackwardUpComplete);        
    }

    public void BeginRepositionCenter()
    {
        IsRepositioning = true;
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), UpLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnCenterUpComplete);
    }
}
