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

    private Rigidbody   m_hBody;    
    private Vector3     m_vTurn;
    private Vector3     m_vTorsoDirection;
    private bool        m_bMoveForward;
    private bool        m_bMoveBackward;
    private bool switchLeg;

    private Queue<IKLeg> m_hLegs;
    private IKLeg        m_hLeg;
    public float RepositioningTime = 0.5f;

    void Start()
    {
        m_hLegs = new Queue<IKLeg>(this.GetComponentsInChildren<IKLeg>());        
        m_hLeg  = m_hLegs.Dequeue();
        
        m_hBody = this.GetComponent<Rigidbody>();
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
            m_hBody.angularVelocity = m_vTurn.normalized * this.TurnSpeed;

            if (!m_hLeg.IsRepositioning)
                m_hLeg.BeginRepositionCenter();
        }
        else
        {
            m_hBody.angularVelocity = Vector3.zero;
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
        Plane vPlane = new Plane(Vector3.up, this.gameObject.transform.position);

        float fRes;
        Ray vRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        vPlane.Raycast(vRay, out fRes);
        
        float angle = Mathf.Acos( Vector3.Dot(this.transform.position.normalized, m_vTorsoDirection.normalized));
        Debug.Log(angle);
        m_vTorsoDirection = vRay.GetPoint(fRes);
        m_vTorsoDirection.x = 0f;
        m_vTorsoDirection.z = 0f;
        Debug.Log(m_vTorsoDirection);
        Torso.transform.rotation = Quaternion.Euler(m_vTorsoDirection);
        //Torso.transform.LookAt(m_vTorsoDirection);
    }


    private interface IState
    {
        IState Update();
    }

    private class StateMove
    {

    }

}
