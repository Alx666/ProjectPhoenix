using UnityEngine;
using System.Collections;
using System;

public class ControllerAITurret : MonoBehaviour, IControllerAI
{
    public GameObject AxeYrot;
    public GameObject AxeXrot;
    public float minRange;
    public float maxRange;
    public  bool Trovato = false;

    public float RotationSpeed = 10;
    public IState CurrentState { get; set; }
    //public bool Trovato = true;
    public string DEBUG_STATE;
 
    void Awake()
    {
        IdleState stateIdle = new IdleState(this);
        PatrolState statepatrol = new PatrolState(this);
        AttackState stateAttack = new AttackState(this);

        stateIdle.Patrol = statepatrol;
        stateIdle.Attack = stateAttack;

        statepatrol.Idle = stateIdle;
        statepatrol.Attack = stateAttack;

        stateAttack.Idle = stateIdle;

        //Init
        CurrentState = stateIdle;
        CurrentState.OnStateEnter();
    }
	void Start ()
    {
        
    }
	void Update ()
    {
        DEBUG_STATE = CurrentState.ToString();
        CurrentState = CurrentState.OnStateUpdate();
    }
    

    public interface IState
    {
        void OnStateEnter();
        IState OnStateUpdate();
    }

    class IdleState : IState
    {
        ControllerAITurret owner;
        public IState Patrol { get; internal set; }
        public IState Attack { get; internal set; }

        private float timer;

        public IdleState(ControllerAITurret owner)
        {
            this.owner = owner;
        }
        public void OnStateEnter()
        {
            timer = UnityEngine.Random.Range(1f, 3f);
        }
        public IState OnStateUpdate()
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0f, timer);
            if(timer == 0f)
            {
                Patrol.OnStateEnter();
                return Patrol;
            }
            return this;
        }
    }
    class PatrolState : IState
    {
        private ControllerAITurret owner;

        public IState Idle { get; internal set; }
        public IState Attack { get; internal set; }

        float yAngle;
        Transform yRot;
        Transform xRot;

        Vector3 vRotation;

        public PatrolState(ControllerAITurret owner)
        {
            this.owner = owner;

            yRot = this.owner.AxeYrot.transform;
            xRot = this.owner.AxeXrot.transform;
        }

        public void OnStateEnter()
        {
          
            yAngle = UnityEngine.Random.Range(-180f, 180f);
       

            yAngle += yRot.transform.localRotation.eulerAngles.y;
           

            vRotation = Quaternion.Euler(0, yAngle, 0) * yRot.transform.forward;
        }

        public IState OnStateUpdate()
        {
            Quaternion newPosition = Quaternion.LookRotation(vRotation);
           
            yRot.transform.localRotation = Quaternion.Slerp(yRot.transform.localRotation, Quaternion.LookRotation(vRotation), owner.RotationSpeed * Time.deltaTime);


            if (Mathf.Approximately ( yRot.transform.localEulerAngles.y , newPosition.eulerAngles.y ))
                
                return Idle;
            else if (owner.Trovato )
            {
                Attack.OnStateEnter();
                return Attack;
            }
                return this;   
        }
    }
    class AttackState : IState
    {
        private ControllerAITurret owner;

        public IState Idle { get; internal set; }

        public AttackState(ControllerAITurret owner)
        {
            this.owner = owner;
        }
        public void OnStateEnter()
        {

        }

        public IState OnStateUpdate()
        {
            Vector3 baseDirection = (owner.target.transform.position - owner.AxeXrot.transform.position).normalized;
            float rotY = Mathf.Atan2(baseDirection.x, baseDirection.z) * (180 / Mathf.PI);

            // Z axis inclination
            Vector3 cannonInclination = owner.AxeXrot.transform.rotation.eulerAngles;
            float angle = Vector3.Distance(owner.target.transform.position, owner.AxeYrot.transform.position) / owner.RotationSpeed;

            float angle2 = owner.target.transform.position.y - owner.AxeYrot.transform.position.y;
            cannonInclination.y = (-Mathf.Atan(angle) * (180 / Mathf.PI)) / 4.0F;

            cannonInclination.x = Mathf.Clamp(Mathf.Abs(cannonInclination.y), owner.minRange, owner.maxRange);

            owner.AxeYrot.transform.rotation = Quaternion.Slerp(owner.AxeYrot.transform.rotation, Quaternion.Euler(owner.AxeYrot.transform.eulerAngles.x, rotY, owner.AxeYrot.transform.eulerAngles.z), Time.deltaTime * owner.RotationSpeed);
            owner.AxeXrot.transform.rotation = Quaternion.Slerp(owner.AxeXrot.transform.rotation, Quaternion.Euler(cannonInclination.x, owner.AxeXrot.transform.transform.eulerAngles.y, owner.AxeXrot.transform.transform.eulerAngles.z), Time.deltaTime * owner.RotationSpeed);
        
            if (!owner.Trovato)
            {
                return Idle;
            }
            return this;
        }
    }

    #region IAITurret
    public  GameObject target;
  

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
    #endregion
}
