using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(CCDIK))]
public class IKLegSpider : MonoBehaviour
{
    public GameObject FrontLocator;
    public GameObject RearLocator;
    public GameObject CenterLocator;
    public GameObject UpLocator;
    public GameObject DownLocator;

    private IKSolver m_hIK;
    private ControllerSpiderMech m_hController;

    public Vector3 IKPosition { get { return m_hIK.GetIKPosition(); } }
    public bool IsRepositioning { get; private set; }
    public float RepositioningTime { get; set; }

    void Start()
    {
        m_hIK = this.GetComponent<CCDIK>().GetIKSolver();
        m_hController = this.transform.root.GetComponent<ControllerSpiderMech>();
    }


    private void OnUpdateVector3(Vector3 vValue)
    {
        m_hIK.SetIKPosition(vValue);
    }

    private void OnForwardUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), FrontLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(BeginRepositionDown);
    }

    private void OnBackwardUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), RearLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(BeginRepositionDown);
    }

    private void OnCenterUpComplete()
    {
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), CenterLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(BeginRepositionDown);
    }

    private void OnLegDownComplete()
    {
        IsRepositioning = false;

        m_hController.EndReposition(this);
    }

    public void BeginRepositionFront()
    {
        IsRepositioning = true;
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

    public void BeginRepositionDown()
    {
        IsRepositioning = true;
        LeanTween.value(this.gameObject, m_hIK.GetIKPosition(), DownLocator.transform.position, m_hController.RepositioningTime).setOnUpdateVector3(OnUpdateVector3).setOnComplete(OnLegDownComplete);
    }
}

