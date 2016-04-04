using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

internal class Test_ControllerAIWheelTracks : MonoBehaviour, IControllerAI
{

    public float Hp = 100f;
    public float SteerAngle = 30f;
    public float MaxSpeed = 50f;
    [Range(0f, 1f)]
    public float CenterOfMassY = 0.6f;

    private List<Wheel> m_hWheels;
    private List<FakeWheel> m_hFakeWheels;
    private Rigidbody m_hRigidbody;
    private Drive m_hEngine;
    private VehicleTurret m_hTurret;
    private IWeapon m_hCurrentWeapon;
    private bool m_hForward = false;
    private bool m_hBackward = false;
    private bool m_hRight = false;
    private bool m_hLeft = false;


    //DEVE ESSERE UTILIZZATO QUESTO PER LA FSM!!!
    private IState currentState;

    void Awake()
    {
        m_hWheels = new List<Wheel>();
        m_hRigidbody = this.GetComponent<Rigidbody>();
        m_hRigidbody.centerOfMass = new Vector3(m_hRigidbody.centerOfMass.x, CenterOfMassY, m_hRigidbody.centerOfMass.z);

        //Initialize effective wheels
        List<Transform> gfxPos = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<WheelCollider>() == null).ToList();
        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => m_hWheels.Add(new Wheel(hW, gfxPos.OrderBy(hP => Vector3.Distance(hP.position, hW.transform.position)).First().gameObject)));
        m_hWheels = m_hWheels.OrderByDescending(hW => hW.Collider.transform.position.z).ToList();

        //Initialize extra wheels
        m_hFakeWheels = GetComponentsInChildren<FakeWheel>().ToList();

        //Initialize VehicleTurret
        m_hTurret = GetComponentInChildren<VehicleTurret>();

        //Initialize IWeapon
        m_hCurrentWeapon = GetComponentInChildren<IWeapon>();

        //Initialize Drive/Brake System
        m_hEngine = new Drive(Hp, m_hWheels);


        //FSM
        StateIdle idle      = new StateIdle(this);
        StatePatrol patrol  = new StatePatrol(this);

        idle.Patrol         = patrol;
        patrol.Idle         = idle;

        currentState        = idle;
        currentState.OnStateEnter();
    }

    void Update()
    {
        currentState = currentState.Update();


        m_hWheels.ForEach(hW => hW.OnUpdate());
        m_hFakeWheels.ForEach(hfw => hfw.OnUpdate(m_hWheels.Last().Collider));

        m_hRigidbody.velocity = Vector3.ClampMagnitude(m_hRigidbody.velocity, MaxSpeed / 3.6f);

        if (m_hRigidbody.velocity.magnitude > 0f && m_hRigidbody.velocity.magnitude < 1f)
            m_hRigidbody.velocity = Vector3.zero;
    }


    #region IControllerAI
    private GameObject target;
    public GameObject Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
        }
    }

    public void Idle()
    {
    }

    public void Patrol()
    {
    }

    public void Attack()
    {
    }
    #endregion  //Inutile!

    #region FSM
    public interface IState
    {
        void OnStateEnter();
        IState Update();
    }

    private class StateIdle : IState
    {
        private Test_ControllerAIWheelTracks owner;
        public IState Patrol { get; internal set; }

        public StateIdle(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            Debug.Log("IDLE");
        }

        public IState Update()
        {
            if(owner.Target != null)
            {
                //Vector3 vDestination = owner.Target.transform.position;

                ////DELETE ME AFTER
                //vDestination.x = 0f;
                //vDestination.y = 0f;
                ////DELETE ME AFTER


                //float distance = Vector3.Distance(owner.transform.position, vDestination);

                //if (!(distance > 0f && distance < 1f))
                //{
                    Patrol.OnStateEnter();
                    return Patrol;
                //}
               
            }

            return this;
        }
    }
    private class StatePatrol : IState
    {
        private Test_ControllerAIWheelTracks owner;
        public IState Idle { get; internal set; }

        public StatePatrol(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            Debug.Log("PATROL");
        }

        public IState Update()
        {
            Vector3 vDestination = owner.Target.transform.position;


            //STEER: calcola meglio se gira a sx o dx
            Vector3 vDistance = vDestination - owner.transform.position;
            float angle = Vector3.Angle(owner.transform.forward, vDistance);

            float sign = Mathf.Sign(Vector3.Dot(vDistance, owner.transform.forward));
            Debug.Log(angle * sign);

            if (!(angle > 0f && angle < owner.SteerAngle))
            {
                if (angle >= 0f)
                    owner.BeginTurnRight();
                else
                    owner.BeginTurnLeft();
            }
            else
            {
                if (angle >= 0f)
                    owner.EndTurnRight();
                else
                    owner.EndTurnLeft();
            }

            //FORWARD
            owner.BeginForward();
            float distance = Vector3.Distance(owner.transform.position, vDestination);
            if(distance > 0f && distance < 5f)
            {
                owner.EndForward();
                owner.BeginBackward();

                if (owner.m_hRigidbody.velocity.magnitude > 0f && owner.m_hRigidbody.velocity.magnitude < 1f)
                {
                    owner.EndBackward();

                    owner.target = null;    //DELETE!!!

                    Idle.OnStateEnter();
                    return Idle;
                }
                   
            }

            return this;
        }
    }
    #endregion

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
            m_hCurrentWeapon.Press();
    }

    public void EndFire()
    {
        if (m_hCurrentWeapon != null)
            m_hCurrentWeapon.Release();
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        if(m_hTurret != null)
            m_hTurret.UpdateRotation(vMousePosition);
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

    #region Drive system

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


