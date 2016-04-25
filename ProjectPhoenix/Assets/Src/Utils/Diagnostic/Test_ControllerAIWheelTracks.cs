using Graph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class Test_ControllerAIWheelTracks : MonoBehaviour, IControllerAI
{
    public float Hp = 100f;
    public float SteerAngle = 30f;
    public float MaxSpeed = 50f;
    public string CurrentSpeed = string.Empty;

    [Range(0f, 1f)]
    public float CenterOfMassY = 0.6f;

    public float StoppingDistance = 5f;
    public float DownForce = 10f;

    private Graph<POI> graph;
    
    private List<Collider> colliders;
    private SphereCollider sphereCollider;
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

    public string DEBUG_CurrentState = string.Empty;


    //DEVE ESSERE UTILIZZATO QUESTO PER LA FSM!!!
    private StateIdle idle;
    private StatePatrol patrol;
    private StateOnAir onAir;
    private StateWait wait;
    private StateOverTurned overTurned;

    private IState currentState;

    private void Awake()
    {
        sphereCollider = this.GetComponent<SphereCollider>();

        colliders = GetComponents<Collider>().ToList();
        colliders = GetComponentsInChildren<Collider>().ToList();

        colliders.ForEach(hC => Physics.IgnoreCollision(sphereCollider, hC));

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
        idle = new StateIdle(this);
        patrol = new StatePatrol(this);
        onAir = new StateOnAir(this);
        wait = new StateWait(this);
        overTurned = new StateOverTurned(this);

        patrol.Idle = idle;
        patrol.OnAir = onAir;
        onAir.Wait = wait;
        wait.Patrol = patrol;

        //from Any State:
        overTurned.Wait = wait;

        currentState = idle;
        currentState.OnStateEnter();
    }
    private void Start()
    {
        graph = GraphParser.Instance.Parse("Graph.txt");
    }
    private void Update()
    {
        currentState = currentState.Update();
        DEBUG_CurrentState = currentState.ToString();

        m_hWheels.ForEach(hW => hW.OnUpdate());
        m_hFakeWheels.ForEach(hfw => hfw.OnUpdate(m_hWheels.Last().Collider));

        //ANYSTATE => TO OVERTURNED
        float cos = Vector3.Dot(Vector3.up, this.transform.up);
        if (cos >= -1.0f && cos <= 0f)
        {
            overTurned.OnStateEnter();
            currentState = overTurned;
        }
    }

    private void FixedUpdate()
    {
        if (currentState != overTurned)
            m_hRigidbody.AddForce((-this.transform.up * DownForce * m_hRigidbody.velocity.magnitude));

        m_hRigidbody.velocity = Vector3.ClampMagnitude(m_hRigidbody.velocity, MaxSpeed / 3.6f);

        if (m_hRigidbody.velocity.magnitude > 0f && m_hRigidbody.velocity.magnitude < 1f)
            m_hRigidbody.velocity = Vector3.zero;

        CurrentSpeed = (m_hRigidbody.velocity.magnitude * 3.6f).ToString();
    }

    #region IControllerAI

    public GameObject target;

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
        idle.OnStateEnter();
        currentState = idle;
    }

    public void Patrol()
    {
        patrol.OnStateEnter();
        currentState = patrol;
    }

    public void Attack()
    {
    }

    #endregion IControllerAI

    #region FSM

    public interface IState
    {
        void OnStateEnter();

        IState Update();
    }

    private class StateIdle : IState
    {
        private Test_ControllerAIWheelTracks owner;

        public StateIdle(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            owner.EndForward();
        }

        public IState Update()
        {

            if(owner.m_hRigidbody.velocity.magnitude > 1f)
            {
                float sign = Mathf.Sign(Vector3.Dot(this.owner.transform.forward, this.owner.m_hRigidbody.velocity));
                if (sign > 0f)
                {
                    owner.EndForward();
                    owner.BeginBackward();
                }
                else
                {
                    owner.EndBackward();
                    owner.BeginForward();
                }
            }
            else
            {
                owner.EndBackward();
                owner.EndForward();
            }
            

            return this;
        }

        public override string ToString()
        {
            return "IDLE";
        }
    }

    private class StatePatrol : IState
    {
        private Test_ControllerAIWheelTracks owner;
        public StateIdle Idle { get; internal set; }
        public StateOnAir OnAir { get; internal set; }

        bool pathComputed;
        Queue<Node<POI>> pois;
        private POI poi;

        public StatePatrol(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            if (!pathComputed)
            {
                Node<POI> nearestAI = owner.graph.m_hNodes.OrderBy(hN => Vector3.Distance(hN.value.Position, owner.transform.position)).First();
                Node<POI> nearestPlayer = owner.graph.m_hNodes.OrderBy(hN => Vector3.Distance(hN.value.Position, owner.Target.transform.position)).First();

                pois = new Queue<Node<POI>>(owner.graph.Dijkstra(nearestPlayer, nearestAI));
                pathComputed = true;
            }
            

            //GET POI FROM QUEUE
            if (pois.Count > 0)
            {
                poi = pois.Dequeue().value;
                Debug.Log("DEQUEUED");
            }
            else
            {
                poi = null;
            }
        }

        public IState Update()
        {
            if (owner.Target == null)
                return Idle;

            //STEERING
            Vector3 vDestination = poi.Position;
            Vector3 vDistance = vDestination - owner.transform.position;

            float angle = Vector3.Angle(owner.transform.forward, vDistance);
            float dot = (Mathf.Clamp(Vector3.Dot(owner.transform.right, vDistance), -1f, 1f));

            if (!(angle > 0f && angle < owner.SteerAngle * 0.5))
            {
                if (dot >= 0f)
                    owner.BeginTurnRight();
                else
                    owner.BeginTurnLeft();
            }
            else
            {
                if (owner.m_hRight)
                    owner.EndTurnRight();
                else if (owner.m_hLeft)
                    owner.EndTurnLeft();
            }

            //FORWARD
            owner.BeginForward();
            float distance = Vector3.Distance(owner.transform.position, vDestination);

            float sign = Mathf.Sign(Vector3.Dot(this.owner.transform.forward, this.owner.m_hRigidbody.velocity));
            if (distance > 0f && distance < owner.StoppingDistance && sign > 0f)
            {
                if (pois.Count > 0)
                {
                    this.OnStateEnter();
                    return this;
                }
                else
                {
                    owner.EndForward();
                    owner.BeginBackward();

                    if (owner.m_hRigidbody.velocity.magnitude < 1f)
                    {
                        owner.EndBackward();

                        owner.target = null;

                        if (owner.m_hRight)
                            owner.EndTurnRight();
                        else if (owner.m_hLeft)
                            owner.EndTurnLeft();

                        Idle.OnStateEnter();
                        return Idle;
                    }
                }
                  
            }

            //ONAIR ?
            RaycastHit vHit;
            Vector3 vPosition = owner.transform.position;
            vPosition.y += 1f;
            if (!(Physics.Raycast(new Ray(vPosition, -owner.transform.up), out vHit, 4f)))
            {
                owner.EndForward();

                OnAir.OnStateEnter();
                return OnAir;
            }

            return this;
        }

        public override string ToString()
        {
            return "PATROL";
        }
    }

    private class StateOnAir : IState
    {
        private Test_ControllerAIWheelTracks owner;
        public StateWait Wait { get; internal set; }

        public StateOnAir(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
        }

        public IState Update()
        {
            RaycastHit vHit;
            Vector3 vPosition = owner.transform.position;
            vPosition.y += 1f;
            if (Physics.Raycast(new Ray(vPosition, -owner.transform.up), out vHit, 4f))
            {
                Wait.OnStateEnter();
                return Wait;
            }

            return this;
        }

        public override string ToString()
        {
            return "ONAIR";
        }
    }

    private class StateWait : IState
    {
        private Test_ControllerAIWheelTracks owner;
        private float waitTime;

        public StatePatrol Patrol { get; internal set; }

        public StateWait(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            waitTime = 0.5f;
        }

        public IState Update()
        {
            waitTime = Mathf.Clamp(waitTime - Time.deltaTime, 0f, waitTime);
            if (waitTime == 0f)
            {
                Patrol.OnStateEnter();
                return Patrol;
            }

            return this;
        }

        public override string ToString()
        {
            return "WAIT";
        }
    }

    private class StateOverTurned : IState
    {
        private Test_ControllerAIWheelTracks owner;
        internal StateWait Wait;

        public StateOverTurned(Test_ControllerAIWheelTracks owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
        }

        public IState Update()
        {
            RaycastHit vHit;
            Vector3 vPosition = owner.transform.position;
            if (Physics.Raycast(new Ray(vPosition, Vector3.down), out vHit, 2.5f))
            {
                Wait.OnStateEnter();
                return Wait;
            }
            return this;
        }

        public override string ToString()
        {
            return "OVERTURNED";
        }
    }

    private class StateAttack : IState
    {
        public void OnStateEnter()
        {
        }

        public IState Update()
        {
            return this;
        }

        public override string ToString()
        {
            return "ATTACK";
        }
    }

    #endregion FSM

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
        if (m_hTurret != null)
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

    #endregion IControllerPlayer

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

    #endregion Drive system

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

    #endregion Wheel

    #region Monobehaviours

    public void OnTriggerEnter(Collider other)
    {
        IControllerAI ai = other.GetComponent<IControllerAI>();
        if (ai != null && this.currentState != idle)
        {
            ai.Idle();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        IControllerAI ai = other.GetComponent<IControllerAI>();
        if (ai != null)
        {
            if (this.Target == null)
            {
                ai.Patrol();
            }
            else
            {
                if (this.currentState != idle)
                {
                    ai.Idle();
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        IControllerAI ai = other.GetComponent<IControllerAI>();
        if (ai != null)
        {
            StartCoroutine(TurnOtherToPatrol(1.0f, ai));
        }
    }

    private IEnumerator TurnOtherToPatrol(float time, IControllerAI ai)
    {
        yield return new WaitForSeconds(time);
        ai.Patrol();
    }

    #endregion Monobehaviours
}