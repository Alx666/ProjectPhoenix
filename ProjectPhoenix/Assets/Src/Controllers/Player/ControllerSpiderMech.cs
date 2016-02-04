using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
internal class ControllerSpiderMech : MonoBehaviour, IControllerPlayer
{
    public float MovementSpeed = 3f;
    public float TurnSpeed = 0.3f;
    public float StepDistance = 2f;
    public GameObject Torso;
    public GameObject BreatRoot;
    public float BreathExcursion = 0.003f;
    public float BreatFreq = 0.2f;
    public float RotationSpeed = 5f;

    private Rigidbody m_hBody;
    private Vector3 m_vTurn;
    private Vector3 m_vTorsoDirection;
    private bool m_bMoveForward;
    private bool m_bMoveBackward;
    private bool switchLeg;

    private Quaternion baseRotation;
    private Quaternion targetRotation;

    private Queue<LegCouple> m_hLegs;
    private LegCouple m_hLeg;
    private List<IKLegSpider> legsList;

    public float RepositioningTime = 0.5f;

    void Start()
    {
        legsList = new List<IKLegSpider>(this.GetComponentsInChildren<IKLegSpider>());

        LegCouple firstCouple = new LegCouple(legsList[0], legsList[1]);
        LegCouple secondCouple = new LegCouple(legsList[2], legsList[3]);

        m_hLegs = new Queue<LegCouple>();
        m_hLegs.Enqueue(firstCouple);
        m_hLegs.Enqueue(secondCouple);

        m_hLeg = m_hLegs.Dequeue();

        m_hBody = this.GetComponent<Rigidbody>();
        baseRotation = Torso.transform.rotation;
    }

    void Update()
    {
        //float fy = BreathExcursion * Mathf.Sin(2.0f * Mathf.PI * BreatFreq * Time.time);
        //Vector3 vRootPos = BreatRoot.transform.position;
        //vRootPos.y += fy;
        //BreatRoot.transform.position = vRootPos;


        if (m_bMoveForward)
        {
            if (switchLeg)
            {
                SwitchLeg(m_hLeg);
                switchLeg = false;
            }

            m_hBody.velocity = this.transform.forward * this.MovementSpeed;
            
            if (!m_hLeg.leg1.IsRepositioning && !m_hLeg.leg1.IsRepositioning)
            {
                m_hLeg.leg1.BeginRepositionFront();
                m_hLeg.leg2.BeginRepositionFront();
            }

        }
        else if (m_bMoveBackward)
        {
            if (!switchLeg)
            {
                SwitchLeg(m_hLeg);
                switchLeg = true;
            }

            m_hBody.velocity = -this.transform.forward * this.MovementSpeed;

            if (!m_hLeg.leg1.IsRepositioning && !m_hLeg.leg1.IsRepositioning)
            {
                m_hLeg.leg1.BeginRepositionRear();
                m_hLeg.leg2.BeginRepositionRear();
            }
        }

        if (m_vTurn.magnitude > 0.1)
        {
            m_hBody.angularVelocity = m_vTurn.normalized * this.TurnSpeed;

            if (!m_hLeg.leg1.IsRepositioning && !m_hLeg.leg1.IsRepositioning)
            {
                m_hLeg.leg1.BeginRepositionCenter();
                m_hLeg.leg2.BeginRepositionCenter();
            }

            baseRotation = Torso.transform.rotation;
        }
        else
        {
            m_hBody.angularVelocity = Vector3.zero;
        }
    }

    public void EndReposition(IKLegSpider hLeg)
    {
        if (hLeg == m_hLeg.leg1)
            m_hLeg.leg1done = true;
        else
            m_hLeg.leg2done = true;

        if (m_hLeg.leg1done && m_hLeg.leg2done)
            EndReposition(m_hLeg);
    }

    private void EndReposition(LegCouple hLeg)
    {
        m_hLeg.leg1done = false;
        m_hLeg.leg2done = false;
        m_hBody.velocity = Vector3.zero;
        m_hLegs.Enqueue(hLeg);
        m_hLeg = m_hLegs.Dequeue();
    }

    private void SwitchLeg(LegCouple hLeg)
    {
        m_hLegs.Enqueue(hLeg);
        m_hLeg = m_hLegs.Dequeue();
    }


    public void BeginForward()
    {
        m_bMoveForward = true;
    }
    public void EndForward()
    {
        m_bMoveForward = false;
    }

    public void BeginBackward()
    {
        m_bMoveBackward = true;
    }

    public void EndBackward()
    {
        m_bMoveBackward = false;
    }

    public void BeginTurnLeft()
    {
        m_vTurn -= this.transform.up;
    }

    public void BeginTurnRight()
    {
        m_vTurn += this.transform.up;
    }

    public void EndTurnLeft()
    {
        m_vTurn += this.transform.up;
    }

    public void EndTurnRight()
    {
        m_vTurn -= this.transform.up;
    }

    public void BeginDown()
    {

    }

    public void BeginFire()
    {

    }

    public void BeginPanLeft()
    {

    }

    public void BeginPanRight()
    {

    }



    public void BeginUp()
    {

    }



    public void EndDown()
    {

    }

    public void EndFire()
    {

    }

    public void EndPanLeft()
    {

    }

    public void EndPanRight()
    {

    }



    public void EndUp()
    {

    }



    //Turn the torso in the direction of mouse position
    public void MousePosition(Vector3 vMousePosition)
    {
        Plane vPlane = new Plane(Vector3.up, this.gameObject.transform.position);
        float fRes;
        Ray vRay = Camera.main.ScreenPointToRay(Input.mousePosition);


        if (vPlane.Raycast(vRay, out fRes))
        {
            Vector3 target = vRay.GetPoint(fRes);
            Vector3 look = target - this.transform.position;


            Quaternion q = Quaternion.LookRotation(look);
            if (Quaternion.Angle(q, baseRotation) <= 35f)
                targetRotation = q;

            Torso.transform.rotation = Quaternion.Slerp(Torso.transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        }
    }


    private interface IState
    {
        IState Update();
    }

    private class StateMove
    {

    }

    internal class LegCouple
    {
        public IKLegSpider leg1;
        public IKLegSpider leg2;

        public bool leg1done { get; set; }
        public bool leg2done { get; set; }

        public LegCouple(IKLegSpider leg1, IKLegSpider leg2)
        {
            this.leg1 = leg1;
            this.leg2 = leg2;
        }


    }
}
