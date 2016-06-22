using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

[RequireComponent(typeof(ConstantForce))]
public class ControllerWheels : NetworkBehaviour, IControllerPlayer
{

    public bool OverrideCenterOfMass;
    public bool SyncGfxWheels;

    public string CurrentSpeed = string.Empty;

    public float Hp = 100f;
    public float SteerAngle = 30f;
    public float MaxSpeed = 50f;

    public Vector3 OverrideCOM;


    public bool m_hForward  { get; set; }
    public bool m_hBackward { get; set; }
    public bool m_hRight    { get; set; }
    public bool m_hLeft { get; set; }

    internal bool IsFlying = false;

    private IFlyState m_hFlyState;

    private List<Wheel> m_hWheels;
    private List<FakeWheel> m_hFakeWheels;
    private Drive m_hEngine;
    private VehicleTurret m_hTurret;

    private Rigidbody m_hRigidbody;
    private ConstantForce m_hConstanForce;

    private IWeapon m_hCurrentWeapon;

    private Vector3 m_hReverseCOM;
    private Vector3 m_hOriginalCOM;



    void Awake()
    {
        m_hForward = false;
        m_hBackward = false;
        m_hRight = false;
        m_hLeft = false;

        m_hWheels = new List<Wheel>();
        m_hRigidbody = this.GetComponent<Rigidbody>();

        //Initialize effective wheels
        List<Transform> gfxPos = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<WheelCollider>() == null).ToList();
        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => m_hWheels.Add(new Wheel(hW, gfxPos.OrderBy(hP => Vector3.Distance(hP.position, hW.transform.position)).First().gameObject)));
        m_hWheels = m_hWheels.OrderByDescending(hW => hW.Collider.transform.localPosition.z).ToList();

        //Initialize extra wheels
        m_hFakeWheels = GetComponentsInChildren<FakeWheel>().ToList();

        //Initialize VehicleTurret
        m_hTurret = GetComponentInChildren<VehicleTurret>();

        //Initialize IWeapon
        m_hCurrentWeapon = GetComponentInChildren<IWeapon>();

        //Initialize Drive/Brake System
        m_hEngine = new Drive(Hp, m_hWheels);

        m_hConstanForce = this.GetComponent<ConstantForce>();
        m_hReverseCOM = new Vector3(0.0f, -2.0f, 0.0f);

        m_hOriginalCOM = m_hRigidbody.centerOfMass;


        GroundState hGroundState = new GroundState(this);
        FlyState hFlyState = new FlyState(this);
        TurnedState hTurned = new TurnedState(this, m_hReverseCOM);

        hGroundState.Next = hFlyState;
        hFlyState.Grounded = hGroundState;
        hFlyState.Turned = hTurned;
        hTurned.Next = hFlyState;
        m_hFlyState = hFlyState;
    }

    void Start()
    {
        if (!this.isLocalPlayer)
            GameObject.Destroy(this.GetComponent<InputProviderPCStd>());

        if (OverrideCenterOfMass)
            m_hRigidbody.centerOfMass = OverrideCOM;
    }

    void Update()
    {
        if (SyncGfxWheels)
        {
            m_hWheels.ForEach(hW => hW.OnUpdate());
            m_hFakeWheels.ForEach(hfw => hfw.OnUpdate(m_hWheels.Last().Collider));
        }

        m_hEngine.OnUpdate(IsFlying);

        Debug.Log(m_hRigidbody.centerOfMass);


        CurrentSpeed = (m_hRigidbody.velocity.magnitude * 3.6f).ToString();

        m_hFlyState = m_hFlyState.Update();
    }



    public void FixedUpdate()
    {
        m_hRigidbody.velocity = Vector3.ClampMagnitude(m_hRigidbody.velocity, MaxSpeed / 3.6f);
    }

    #region IControllerPlayer

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
        {
            m_hCurrentWeapon.Press();
        }
    }

    public void EndFire()
    {
        if (m_hCurrentWeapon != null)
        {
            m_hCurrentWeapon.Release();
        }
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        if (m_hTurret != null && isLocalPlayer)
            m_hTurret.UpdateRotation(vMousePosition);
    }

    public void BeginUp()
    {

    }

    public void EndUp()
    {

    }

    public void StopVehicle()
    {
        Vector3 currentSpeed = this.GetComponent<Rigidbody>().velocity;
        Vector3 stop;
        stop.x = Mathf.Lerp(0f, -currentSpeed.x, Time.deltaTime);
        stop.y = Mathf.Lerp(0f, -currentSpeed.y, Time.deltaTime);
        stop.z = Mathf.Lerp(0f, -currentSpeed.z, Time.deltaTime);
        if (stop.magnitude > 0.2f)
            this.GetComponent<Rigidbody>().AddForce(stop, ForceMode.VelocityChange);
        else
            this.GetComponent<Rigidbody>().AddForce(-currentSpeed, ForceMode.VelocityChange);
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

    #region Drive system

    private class Drive
    {
        private float m_fHp;
        private float m_fMotorTorqueMultiplier;
        private List<Wheel> m_hWheels;


        public Drive(float fHp, List<Wheel> hWheels)
        {
            m_fHp = fHp;
            m_hWheels = hWheels;
            m_fMotorTorqueMultiplier = 0.25f;
        }

        public void OnUpdate(bool IsCarflying)
        {
            //if (IsCarflying)
            //{
            //    m_fMotorTorqueMultiplier = 0f;
            //    if (m_hWheels.TrueForAll(hW => hW.Collider.rpm > 200f))
            //    {
            //        m_hWheels.ForEach(hW => hW.Collider.brakeTorque = m_fHp);
            //    }
            //    else
            //    {
            //        m_hWheels.ForEach(hW => hW.Collider.brakeTorque = 0f);
            //    }
            //}
            //else
            //{
            //    m_hWheels.ForEach(hW => hW.Collider.brakeTorque = 0f);
            //    m_fMotorTorqueMultiplier = 0.25f;
            //}
        }

        public void BeginAccelerate()
        {
            //AWD
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = m_fHp * m_fMotorTorqueMultiplier);
        }

        public void EndAccelerate()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }

        public void BeginBackward()
        {
            //AWD
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = -(m_fHp * m_fMotorTorqueMultiplier));
        }

        public void EndBackward()
        {
            m_hWheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }
    }

    #endregion

    #region Wheel

    internal class Wheel
    {
        internal WheelCollider Collider { get; private set; }
        internal GameObject Gfx { get; private set; }

        //Bool for motor torque handling while the vehicle is in the air
        internal bool IsWheelGrounded { get; private set; }

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

            if (Collider.isGrounded)
                IsWheelGrounded = true;
            else
                IsWheelGrounded = false;
        }
    }

    #endregion

    #region Vehicle Handling



    private interface IFlyState
    {
        void OnEnter();
        IFlyState Update();
    }

    private class GroundState : IFlyState
    {
        private ControllerWheels m_hOwner;
        public GroundState(ControllerWheels hOwner)
        {
            m_hOwner = hOwner;
        }
        public IFlyState Next { get; set; }
        public void OnEnter()
        {
            if (m_hOwner.m_hRigidbody.velocity.magnitude > 0f && m_hOwner.m_hRigidbody.velocity.magnitude < 1f && m_hOwner.m_hForward == false && m_hOwner.m_hBackward == false)
                m_hOwner.m_hRigidbody.velocity = Vector3.zero;

            //m_hOwner.m_hConstanForce.enabled = false;
            //LeanTween.value(m_hOwner.gameObject, new Vector3(m_hOwner.m_hRigidbody.centerOfMass.x, m_hOwner.m_hRigidbody.centerOfMass.y, m_hOwner.m_hRigidbody.centerOfMass.z), new Vector3(m_hOwner.m_hRigidbody.centerOfMass.x, m_hOwner.OverrideCOM.y, m_hOwner.m_hRigidbody.centerOfMass.z), 0.5f).setOnUpdateVector3(SetCenterOfMass);
            

            m_hOwner.m_hConstanForce.enabled = false;
            LeanTween.value(m_hOwner.gameObject, new Vector3(m_hOwner.m_hRigidbody.centerOfMass.x, m_hOwner.m_hRigidbody.centerOfMass.y, m_hOwner.m_hRigidbody.centerOfMass.z), new Vector3(m_hOwner.OverrideCOM.x, m_hOwner.OverrideCOM.y, m_hOwner.OverrideCOM.z), 1.5f).setOnUpdateVector3(SetCenterOfMass);

            m_hOwner.IsFlying = false;
        }

        public IFlyState Update()
        {
            if (m_hOwner.m_hWheels.TrueForAll(hW => !hW.IsWheelGrounded))
            {
                Next.OnEnter();
                return Next;
            }
            else
            {

                return this;
            }
        }

        private void SetCenterOfMass(Vector3 val)
        {
            m_hOwner.m_hRigidbody.centerOfMass = val;
        }
    }

    private class FlyState : IFlyState
    {
        private ControllerWheels m_hOwner;
        public FlyState(ControllerWheels hOwner)
        {
            m_hOwner = hOwner;
        }

        public GroundState Grounded { get; set; }
        public TurnedState Turned { get; set; }

        public void OnEnter()
        {
            //LeanTween.value(m_hOwner.gameObject, new Vector3(m_hOwner.m_hRigidbody.centerOfMass.x, m_hOwner.m_hRigidbody.centerOfMass.y, m_hOwner.m_hRigidbody.centerOfMass.z), new Vector3(m_hOwner.m_hRigidbody.centerOfMass.x, m_hOwner.m_hOriginalCOM.y, m_hOwner.m_hRigidbody.centerOfMass.z), 0.5f).setOnUpdateVector3(SetCenterOfMass);
            m_hOwner.m_hConstanForce.enabled = true;
            m_hOwner.IsFlying = true;
        }

        public IFlyState Update()
        {
            if (m_hOwner.m_hWheels.TrueForAll(hW => hW.IsWheelGrounded))
            {
                Grounded.OnEnter();
                return Grounded;
            }
            else if (Vector3.Angle(Vector3.up, m_hOwner.transform.up) > 60f)
            {
                Turned.OnEnter();
                return Turned;
            }
            else
                return this;

        }

        private void SetCenterOfMass(Vector3 val)
        {
            m_hOwner.m_hRigidbody.centerOfMass = val;
        }
    }

    private class TurnedState : IFlyState
    {
        private ControllerWheels m_hOwner;
        private Vector3 m_vTurnOverride;
        public TurnedState(ControllerWheels hOwner, Vector3 vTurnOverride)
        {
            m_hOwner = hOwner;
            m_vTurnOverride = vTurnOverride;
        }

        public IFlyState Next { get; set; }

        public void OnEnter()
        {
            //m_hOwner.m_hRigidbody.centerOfMass = m_vTurnOverride;
        }

        public IFlyState Update()
        {
            if (Vector3.Angle(Vector3.up, m_hOwner.transform.up) < 25f)
            {
                Next.OnEnter();
                return Next;
            }
            else
            {
                return this;
            }
        }

    }

    #endregion


}


