using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

internal class ControllerPlayerWheels : MonoBehaviour, IControllerPlayer
{
    public float Brake = 100f;
    public float Hp = 100f;
    public float SteerAngle = 30f;
    public float MaxSpeed = 50f;

    private List<Wheel> m_hWheels;
    private Rigidbody m_hRigidbody;
    private Drive m_hEngine;
    private BrakeSystem m_hBrake;
    private bool m_hBackward = false;
    private bool m_hRight = false;
    private bool m_hLeft = false;

    void Awake()
    {
        m_hWheels = new List<Wheel>();
        m_hRigidbody = this.GetComponent<Rigidbody>();
        m_hRigidbody.centerOfMass = new Vector3(m_hRigidbody.centerOfMass.x, 0.6f, m_hRigidbody.centerOfMass.z);

        //Initialize effective wheels
        List<Transform> gfxPos = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<WheelCollider>() == null).ToList();
        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => m_hWheels.Add(new Wheel(hW, gfxPos.OrderBy(hP => Vector3.Distance(hP.position, hW.transform.position)).First().gameObject)));
        m_hWheels = m_hWheels.OrderByDescending(hW => hW.Collider.transform.position.z).ToList();

        //Todo: initialize extra wheels

        //Initialize Drive/Brake System
        m_hEngine = new Drive(Hp, m_hWheels);
        m_hBrake = new BrakeSystem(0.4f * Brake, 0.6f * Brake, m_hWheels);
    }

    void Update()
    {
        m_hWheels.ForEach(hW => hW.OnUpdate());
        m_hRigidbody.velocity = Vector3.ClampMagnitude(m_hRigidbody.velocity, MaxSpeed / 3.6f);

        if (m_hRigidbody.velocity.magnitude > 0f && m_hRigidbody.velocity.magnitude < 1f)
            m_hRigidbody.velocity = Vector3.zero;
    }

    #region IControllerPlayer

    public void BeginForward()
    {
        m_hBackward = false;
        m_hBrake.EndBrake();

        m_hEngine.BeginAccelerate();
    }

    public void EndForward()
    {
        m_hEngine.EndAccelerate();
    }

    public void BeginBackward()
    {
        m_hBrake.EndBrake();
        m_hEngine.EndAccelerate();

        if (Mathf.Approximately(m_hRigidbody.velocity.magnitude, 0f))
        {
            m_hEngine.BeginReverse();
            m_hBackward = true;
        }
        else if (!m_hBackward)
            m_hBrake.BeginBrake();
    }

    public void EndBackward()
    {
        if (!m_hBackward)
            m_hBrake.EndBrake();
        else
        {
            m_hEngine.EndReverse();
            m_hBackward = false;
        }
    }

    public void BeginTurnRight()
    {
        if (m_hLeft)
            EndTurnLeft();

        m_hRight = true;
        m_hWheels[0].Steer(this.SteerAngle);
        m_hWheels[1].Steer(this.SteerAngle);
    }

    public void EndTurnRight()
    {
        if(m_hRight)
        {
            m_hRight = false;
            m_hWheels[0].Steer(0);
            m_hWheels[1].Steer(0);
        } 
    }

    public void BeginTurnLeft()
    {
        if (m_hRight)
            EndTurnRight();

        m_hLeft = true;
        m_hWheels[0].Steer(-this.SteerAngle);
        m_hWheels[1].Steer(-this.SteerAngle);
    }

    public void EndTurnLeft()
    {
        if (m_hLeft)
        {
            m_hLeft = false;
            m_hWheels[0].Steer(0);
            m_hWheels[1].Steer(0);
        }
    }

    public void BeginFire()
    {

    }

    public void EndFire()
    {

    }

    public void MousePosition(Vector3 vMousePosition)
    {
        //RaycastHit hit;
        //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        //    this.m_hTurretPosition.LookAt(hit.point);
    }

    public void BeginUp()
    {

    }

    public void EndUp()
    {

    }

    public void BeginDown()
    {

    }

    public void EndDown()
    {

    }

    public void BeginPanLeft()
    {

    }

    public void EndPanLeft()
    {

    }

    public void BeginPanRight()
    {

    }

    public void EndPanRight()
    {

    }

    #endregion

    #region Drive and Brake system

    private class Drive
    {
        private float m_fHp;
        private List<Wheel> m_hWheels;

        public Drive(float fHp, List<Wheel> hWheels)
        {
            m_fHp = fHp;
            m_hWheels = hWheels;
        }
        public void BeginRotate()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = m_fHp * 0.25f);
        }
        public void EndRotate()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }
        public void BeginAccelerate()
        {
            //AWD
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = m_fHp * 0.25f);
        }

        public void EndAccelerate()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }

        public void BeginReverse()
        {
            //AWD
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = -(m_fHp * 0.25f));
        }

        public void EndReverse()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }
    }

    private class BrakeSystem
    {
        private float m_fForwardBrake;
        private float m_fBackwardBrake;
        private List<Wheel> m_hWheels;

        public BrakeSystem(float fForwardBrake, float fBackwardBrake, List<Wheel> hWheels)
        {
            m_fForwardBrake = fForwardBrake;
            m_fBackwardBrake = fBackwardBrake;
            m_hWheels = hWheels;
        }

        public void BeginBrake()
        {
            m_hWheels.Take(2).ToList().ForEach(hW => hW.Collider.brakeTorque = m_fForwardBrake);
            m_hWheels.Skip(2).ToList().ForEach(hW => hW.Collider.brakeTorque = m_fBackwardBrake);
        }

        public void EndBrake()
        {
            m_hWheels.ForEach(hW => hW.Collider.brakeTorque = 0f);
        }
    }

    #endregion

    #region Wheel

    internal class Wheel
    {
        internal WheelCollider Collider { get; private set; }
        internal GameObject Gfx { get; private set; }

        internal Wheel(WheelCollider coll, GameObject gfx)
        {
            Collider = coll;
            Gfx = gfx;
        }

        internal void Steer(float fSteer)
        {
            Collider.steerAngle = fSteer;
        }

        internal void OnUpdate()
        {
            Vector3 position;
            Quaternion rotation;
            Collider.GetWorldPose(out position, out rotation);

            Gfx.transform.position = position;
            Gfx.transform.rotation = rotation;
        }
    }

    #endregion
}


