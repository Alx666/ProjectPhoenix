using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

internal class ControllerPlayerTracks : MonoBehaviour, IControllerPlayer
{
    private List<Wheel> m_hWheels;
    private Tracks tracks;

    public float Brake = 100f;
    public float Hp = 100f;
    public float SteerAngle = 30f;
    public float MaxSpeed = 50f;

    private Drive m_hEngine;
    private BrakeSystem m_hBrake;
    public bool m_hReverse;
    private Rigidbody m_hRigidbody;
    private Transform m_hTurretPosition;
    public float vel;
    private float sign;
    private GameObject obj;

    void Awake()
    {
        m_hWheels = new List<Wheel>();
        tracks = GetComponentInChildren<Tracks>();
        m_hRigidbody = this.GetComponent<Rigidbody>();

        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => m_hWheels.Add(new Wheel(hW)));
        m_hWheels = m_hWheels.OrderByDescending(hW => hW.Collider.transform.position.z).ToList();

        m_hRigidbody.centerOfMass = new Vector3(m_hRigidbody.centerOfMass.x, 0.6f, m_hRigidbody.centerOfMass.z);

        //DELETEME
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.GetComponent<Collider>().enabled = false;
        obj.transform.parent = this.transform;
        obj.transform.localPosition = m_hRigidbody.centerOfMass;

        //m_hTurretPosition = this.GetComponentInChildren<Turret>().transform;

        //Initialize Drive/Brake State Machine
        m_hEngine = new Drive(Hp, m_hWheels);
        m_hBrake = new BrakeSystem(0.4f * Brake, 0.6f * Brake, m_hWheels);
    }

    void Update()
    {
        m_hRigidbody.velocity = Vector3.ClampMagnitude(m_hRigidbody.velocity, MaxSpeed / 3.6f);

        vel = m_hRigidbody.velocity.magnitude * 3.6f;
        tracks.TracksSpeed = (vel * sign);

        if (m_hRigidbody.velocity.magnitude > 0f && m_hRigidbody.velocity.magnitude < 1f)
            m_hRigidbody.velocity = Vector3.zero;
    }

    public void BeginForward()
    {
        m_hReverse = false;
        m_hBrake.EndBrake();

        sign = 1f;
        m_hEngine.BeginAccelerate();
    }

    public void EndForward()
    {
        m_hEngine.EndAccelerate();
    }

    public void BeginBackward()
    {
        m_hBrake.EndBrake();

        if (Mathf.Approximately(m_hRigidbody.velocity.magnitude, 0f))
        {
            sign = -1f;
            m_hEngine.BeginReverse();
            m_hReverse = true;
        }
        else if (!m_hReverse)
            m_hBrake.BeginBrake();
    }

    public void EndBackward()
    {
        if (!m_hReverse)
            m_hBrake.EndBrake();
        else
        {
            m_hEngine.EndReverse();
            m_hReverse = false;
        }
    }

    public void BeginTurnRight()
    {
        m_hWheels[0].Steer(this.SteerAngle);
        m_hWheels[1].Steer(this.SteerAngle);
    }

    public void EndTurnRight()
    {
        m_hWheels[0].Steer(0);
        m_hWheels[1].Steer(0);
    }

    public void BeginTurnLeft()
    {
        m_hWheels[0].Steer(-this.SteerAngle);
        m_hWheels[1].Steer(-this.SteerAngle);
    }

    public void EndTurnLeft()
    {
        m_hWheels[0].Steer(0);
        m_hWheels[1].Steer(0);
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

    #region notimplemented
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

    #region Wheel

    private class Wheel
    {
        public WheelCollider Collider { get; private set; }

        public Wheel(WheelCollider collider)
        {
            Collider = collider;
        }

        public void Steer(float fSteer)
        {
            Collider.steerAngle = fSteer;
        }
    }

    #endregion

    #region Drive

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

    [System.Serializable]
    public enum DriveType
    {
        Forward,
        Backward,
        AWD
    }
}