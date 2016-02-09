using UnityEngine;
using System.Collections.Generic;
using System;
using RootMotion.FinalIK;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
internal class ControllerPlayerMech : MonoBehaviour, IControllerPlayer
{
    public float        MovementSpeed     = 3f;
    public float        TurnSpeed         = 0.3f;
    public float        StepDistance      = 2f;
    public GameObject   Torso;
    public GameObject   BreatRoot;
    public float        BreathExcursion   = 0.003f;
    public float        BreatFreq         = 0.2f;
    public float RotationSpeed = 5f;

    private Rigidbody   m_hBody;    
    private Vector3     m_vTurn;
    private Vector3     m_vTorsoDirection;
    private bool        m_bMoveForward;
    private bool        m_bMoveBackward;
    private bool switchLeg;
    private bool done;

    private Quaternion baseRotation;
    private Quaternion targetRotation;

    private Queue<IKLeg> m_hLegs;
    private IKLeg        m_hLeg;
    public float RepositioningTime = 0.5f;
    private bool isRotating;

    void Start()
    {
        m_hLegs = new Queue<IKLeg>(this.GetComponentsInChildren<IKLeg>());        
        m_hLeg  = m_hLegs.Dequeue();
        
        m_hBody = this.GetComponent<Rigidbody>();
        baseRotation = Torso.transform.rotation;
    }

    void Update()
    {
        float fy = BreathExcursion * Mathf.Sin(2.0f * Mathf.PI * BreatFreq * Time.time);
        Vector3 vRootPos = BreatRoot.transform.position;
        vRootPos.y += fy;
        BreatRoot.transform.position = vRootPos;

        if (m_bMoveForward)
        {
            if (switchLeg)
            {
                SwitchLeg(m_hLeg);
                switchLeg = false;
            }

            m_hBody.velocity = this.transform.forward * this.MovementSpeed;

            if (!m_hLeg.IsRepositioning)
            {
                m_hLeg.BeginRepositionFront();
            }           
                
        }
        else if(m_bMoveBackward)
        {
            if (!switchLeg)
            {
                SwitchLeg(m_hLeg);
                switchLeg = true;
            }

            m_hBody.velocity = -this.transform.forward * this.MovementSpeed;

            if (!m_hLeg.IsRepositioning)
            {
                m_hLeg.BeginRepositionRear();
            }
        }

        if (m_vTurn.magnitude > 0.1)
        {
            isRotating = true;

            

            m_hBody.angularVelocity = m_vTurn.normalized * this.TurnSpeed;

            if (!m_hLeg.IsRepositioning)
            {
                m_hLeg.BeginRepositionCenter();
            }

            baseRotation = Torso.transform.rotation;
            targetRotation = Torso.transform.rotation;
        }
        else
        {
            m_hBody.angularVelocity = Vector3.zero;
            isRotating = false;
        }
    }

    public void EndReposition(IKLeg hLeg)
    {
        m_hBody.velocity = Vector3.zero;
        m_hLegs.Enqueue(hLeg);
        m_hLeg = m_hLegs.Dequeue();
    }

    private void SwitchLeg(IKLeg hLeg)
    {
        m_hLegs.Enqueue(hLeg);
        m_hLeg = m_hLegs.Dequeue();
    }


    public void BeginForward()
    {
        m_bMoveForward  = true;
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
        if (!isRotating)
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
    }


    private interface IState
    {
        IState Update();
    }

    private class StateMove
    {

    }

}
