﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

internal class ControllerPlayerTracks : MonoBehaviour, IControllerPlayer
{
    [SerializeField]
    internal float Hp = 100f;
    [SerializeField]
    internal float SteerAngle = 30f;
    [SerializeField]
    internal float MaxSpeed = 50f;
    [SerializeField] [Range(0f, 1f)]
    internal float CenterOfMassY = 0.6f;

    private List<Wheel> m_hWheels;
    private Tracks tracks;
    private Drive m_hEngine;
    private Rigidbody m_hRigidbody;
    private VehicleTurret m_hTurret;
    private IWeapon m_hCurrentWeapon;
    private bool m_hLeft = false;
    private bool m_hRight = false;
    private bool m_hForward = false;
    private bool m_hBackward = false;
    private float sign;

    private GameObject obj;
    public float vel;

    void Awake()
    {
        m_hWheels = new List<Wheel>();
        tracks = GetComponentInChildren<Tracks>();
        m_hRigidbody = this.GetComponent<Rigidbody>();

        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => m_hWheels.Add(new Wheel(hW)));
        m_hWheels = m_hWheels.OrderByDescending(hW => hW.Collider.transform.position.z).ToList();

        m_hRigidbody.centerOfMass = new Vector3(m_hRigidbody.centerOfMass.x, CenterOfMassY, m_hRigidbody.centerOfMass.z);

        //DELETEME
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.GetComponent<Collider>().enabled = false;
        obj.transform.parent = this.transform;
        obj.transform.localPosition = m_hRigidbody.centerOfMass;

        //Initialize VehicleTurret
        m_hTurret = GetComponentInChildren<VehicleTurret>();

        //Initialize IWeapon
        m_hCurrentWeapon = GetComponentInChildren<IWeapon>();

        //Initialize Drive/Brake State Machine
        m_hEngine = new Drive(Hp, m_hWheels);
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
        m_hForward = true;
        m_hEngine.BeginAccelerate();
    }

    public void EndForward()
    {
        m_hForward = false;
        if (m_hBackward)
        {
            m_hEngine.BeginBackward();
        }
        else
        {
            m_hEngine.EndAccelerate();
        }
    }

    public void BeginBackward()
    {
        m_hBackward = true;
        m_hEngine.BeginBackward();
    }

    public void EndBackward()
    {
        m_hBackward = false;
        if (m_hForward)
        {
            m_hEngine.BeginAccelerate();
        }
        else
        {
            m_hEngine.EndBackward();
        }
    }

    public void BeginTurnRight()
    {
        m_hRight = true;
        m_hWheels[0].Steer(this.SteerAngle);
        m_hWheels[1].Steer(this.SteerAngle);
    }

    public void EndTurnRight()
    {
        m_hRight = false;
        if (m_hLeft)
        {
            m_hWheels[0].Steer(-this.SteerAngle);
            m_hWheels[1].Steer(-this.SteerAngle);
        }
        else
        {
            m_hWheels[0].Steer(0);
            m_hWheels[1].Steer(0);
        }
    }

    public void BeginTurnLeft()
    {
        m_hLeft = true;
        m_hWheels[0].Steer(-this.SteerAngle);
        m_hWheels[1].Steer(-this.SteerAngle);
    }

    public void EndTurnLeft()
    {
        m_hLeft = false;
        if (m_hRight)
        {
            m_hWheels[0].Steer(this.SteerAngle);
            m_hWheels[1].Steer(this.SteerAngle);
        }
        else
        {
            m_hWheels[0].Steer(0);
            m_hWheels[1].Steer(0);
        }
    }

    public void BeginFire()
    {
        if (m_hCurrentWeapon != null)
            m_hCurrentWeapon.OnFireButtonPressed();
    }

    public void EndFire()
    {
        if (m_hCurrentWeapon != null)
            m_hCurrentWeapon.OnFireButtonReleased();
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        if (m_hTurret != null)
            m_hTurret.UpdateRotation(vMousePosition);
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

        public void BeginBackward()
        {
            //AWD
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = -(m_fHp * 0.25f));
        }

        public void EndBackward()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
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