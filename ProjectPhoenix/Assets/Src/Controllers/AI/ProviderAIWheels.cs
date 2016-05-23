using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graph;

[RequireComponent(typeof(SphereCollider))]
public class ProviderAIWheels : MonoBehaviour, IControllerAI
{
    public float NodesStoppingDistance = 5f;
    public string DEBUG_CurrentState = string.Empty;

    public AIGraph graph;

    private ControllerWheels receiver;
    private Rigidbody m_hRigidbody;
    private SphereCollider sphereCollider;
    private List<Collider> receiverColliders;

    private StateIdle idle;
    private StatePatrol patrol;
    private StateOnAir onAir;
    private StateWait wait;

    private IState currentState;

    private void Awake()
    {
        sphereCollider = this.GetComponent<SphereCollider>();
        Debug.LogWarning("HARDCODED");
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 20f;

        receiverColliders = GetComponents<Collider>().ToList();
        receiverColliders = GetComponentsInChildren<Collider>().ToList();
        receiverColliders.ForEach(hC => Physics.IgnoreCollision(sphereCollider, hC));

        m_hRigidbody = this.GetComponent<Rigidbody>();

        //FSM
        idle = new StateIdle(this);
        patrol = new StatePatrol(this);
        onAir = new StateOnAir(this);
        wait = new StateWait(this);

        patrol.Idle = idle;
        patrol.OnAir = onAir;
        onAir.Wait = wait;
        wait.Patrol = patrol;

        currentState = idle;
        currentState.OnStateEnter();
    }

    public void Start()
    {
        graph = GameObject.FindObjectOfType<AIGraph>();
    }

    private void Update()
    {
        currentState = currentState.Update();
        DEBUG_CurrentState = currentState.ToString();
    }

    #region IControllerAI

    public GameObject target;

    public GameObject Target { get { return target; } set { target = value; } }

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
        private ProviderAIWheels owner;

        public StateIdle(ProviderAIWheels owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            owner.receiver.EndForward();
        }

        public IState Update()
        {
            Debug.LogWarning("HARDCODED");
            if (owner.m_hRigidbody.velocity.magnitude > 1f)
            {
                float sign = Mathf.Sign(Vector3.Dot(this.owner.transform.forward, this.owner.m_hRigidbody.velocity.normalized));
                if (sign > 0f)
                {
                    owner.receiver.EndForward();
                    owner.receiver.BeginBackward();
                }
                else
                {
                    owner.receiver.EndBackward();
                    owner.receiver.BeginForward();
                }
            }
            else
            {
                owner.receiver.EndBackward();
                owner.receiver.EndForward();
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
        private ProviderAIWheels owner;
        public StateIdle Idle { get; internal set; }
        public StateOnAir OnAir { get; internal set; }

        bool pathComputed;
        Queue<POI> pois;
        private POI poi;

        public StatePatrol(ProviderAIWheels owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
            if (!pathComputed)
            {
                List<POI> roads = owner.graph.Where(hN => hN.Type == POI.NodeType.Road).ToList();

                POI nearestAI = roads.OrderBy(hN => Vector3.Distance(hN.Position, owner.transform.position)).First();
                POI nearestPlayer = roads.OrderBy(hN => Vector3.Distance(hN.Position, owner.Target.transform.position)).First();

                List<POI> list = owner.graph.m_hGraph.Dijkstra(nearestAI, nearestPlayer, roads);

                pois = new Queue<POI>(list);
                pathComputed = true;
            }


            //GET POI FROM QUEUE
            if (pois.Count > 0)
            {
                poi = pois.Dequeue();
                Debug.Log(this.owner.gameObject.name + "DEQUEUED");
            }
            else
            {
                poi = null;
            }
        }

        public IState Update()
        {
            if (owner.Target == null || poi == null)
                return Idle;

            //STEERING
            Vector3 vDestination = poi.Position;
            Vector3 vDistance = vDestination - owner.transform.position;

            float angle = Vector3.Angle(owner.transform.forward, vDistance);
            float dot = (Mathf.Clamp(Vector3.Dot(owner.transform.right, vDistance), -1f, 1f));

            if (!(angle > 0f && angle < owner.receiver.SteerAngle * 0.5))
            {
                if (dot >= 0f)
                    owner.receiver.BeginTurnRight();
                else
                    owner.receiver.BeginTurnLeft();
            }
            else
            {
                if (owner.receiver.m_hRight)
                    owner.receiver.EndTurnRight();
                else if (owner.receiver.m_hLeft)
                    owner.receiver.EndTurnLeft();
            }

            //FORWARD
            owner.receiver.BeginForward();
            float distance = Vector3.Distance(owner.transform.position, vDestination);

            float sign = Mathf.Sign(Vector3.Dot(this.owner.transform.forward, this.owner.m_hRigidbody.velocity));
            if (distance > 0f && distance < owner.NodesStoppingDistance && sign > 0f)
            {
                if (pois.Count > 0)
                {
                    this.OnStateEnter();
                    return this;
                }
                else
                {
                    owner.receiver.EndForward();
                    owner.receiver.BeginBackward();

                    if (owner.m_hRigidbody.velocity.magnitude < 1f)
                    {
                        owner.receiver.EndBackward();

                        owner.target = null;

                        if (owner.receiver.m_hRight)
                            owner.receiver.EndTurnRight();
                        else if (owner.receiver.m_hLeft)
                            owner.receiver.EndTurnLeft();

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
                owner.receiver.EndForward();

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
        private ProviderAIWheels owner;
        public StateWait Wait { get; internal set; }

        public StateOnAir(ProviderAIWheels owner)
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
        private ProviderAIWheels owner;
        private float waitTime;

        public StatePatrol Patrol { get; internal set; }

        public StateWait(ProviderAIWheels owner)
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

    #region OnTriggerEvents

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
