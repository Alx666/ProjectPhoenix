using UnityEngine;
using System.Collections;
using System;

public class ControllerAIArtillery : MonoBehaviour, IControllerAI
{
    public GameObject AxeYrot;
    public GameObject AxeXrot;

    public float MaxDistance; // Distanza massima di visibilita 
    public float RotationSpeed = 10;


    public IState CurrentState { get; set; }

    public string DEBUG_STATE;
 
    void Awake()
    {
        //Initizializazione stati
        IdleState   stateIdle   = new IdleState(this);
        PatrolState statepatrol = new PatrolState(this);
        AttackState stateAttack = new AttackState(this);
        //Idle
        stateIdle.Patrol = statepatrol;
        stateIdle.Attack = stateAttack;
        //Patrol
        statepatrol.Idle    = stateIdle;
        statepatrol.Attack  = stateAttack;
        //Attack
        stateAttack.Idle    = stateIdle;

        CurrentState = stateIdle;
        CurrentState.OnStateEnter();
    }
	void Start ()
    {
        
    }
	void Update ()
    {
        DEBUG_STATE  = CurrentState.ToString();
        CurrentState = CurrentState.OnStateUpdate();
    }
    

    #region FMS
    public interface IState
    {
        void OnStateEnter();
        IState OnStateUpdate();
    }
    class IdleState : IState
    {
        ControllerAIArtillery owner;
        public IState Patrol { get; internal set; }
        public IState Attack { get; internal set; }

        private float timer;

        public IdleState(ControllerAIArtillery owner)
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

            if ((Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.MaxDistance))
            {
                Attack.OnStateEnter();
                return Attack;
            }
            return this;
        }
    }
    class PatrolState : IState
    {
        private ControllerAIArtillery owner;

        public IState Idle { get; internal set; }
        public IState Attack { get; internal set; }

        float yAngle;
        Transform yRot;
        Transform xRot;

        Vector3 vRotation;

        public PatrolState(ControllerAIArtillery owner)
        {
            this.owner = owner;

            yRot = this.owner.AxeYrot.transform;
            xRot = this.owner.AxeXrot.transform;
        }

        public void OnStateEnter()
        {         //Calcolo l'angolo di rotazione
            yAngle    =  UnityEngine.Random.Range(-180f, 180f);
            yAngle   +=  yRot.transform.localRotation.eulerAngles.y;
            vRotation =  Quaternion.Euler(0, yAngle, 0) * yRot.transform.forward;
        }

        public IState OnStateUpdate()
        {
            Quaternion newPosition       = Quaternion.LookRotation(vRotation);
            yRot.transform.localRotation = Quaternion.Slerp(yRot.transform.localRotation, Quaternion.LookRotation(vRotation), owner.RotationSpeed * Time.deltaTime);

            if (Mathf.Approximately ( yRot.transform.localEulerAngles.y , newPosition.eulerAngles.y ))    //True:Se L'angolazione è la stessa
                return Idle;

            if ((Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.MaxDistance))//True:Se la distanza è minore
            {
                Attack.OnStateEnter();
                return Attack;
            }
                return this;   
        }
    }
    class AttackState : IState
    {
        private ControllerAIArtillery owner;

        public IState Idle { get; internal set; }

        public AttackState(ControllerAIArtillery owner)
        {
            this.owner = owner;
        }
        public void OnStateEnter()
        {

        }

        public IState OnStateUpdate()
        {
            // Y axis inclination
            Vector3 baseDirection = (owner.target.transform.position - owner.AxeYrot.transform.position).normalized;
            float   rotY          = Mathf.Atan2(baseDirection.x, baseDirection.z) * (180 / Mathf.PI);

            owner.AxeYrot.transform.rotation = Quaternion.Slerp(owner.AxeYrot.transform.rotation, Quaternion.Euler
                (owner.AxeYrot.transform.eulerAngles.x, rotY, owner.AxeYrot.transform.eulerAngles.z), Time.deltaTime * owner.RotationSpeed);

            // X axis inclination
            Vector3 cannonInclination = owner.AxeXrot.transform.rotation.eulerAngles;
            float   angle       = Vector3.Distance(owner.AxeXrot.transform.position, owner.target.transform.position) /owner.RotationSpeed;
            cannonInclination.z = (-Mathf.Atan(angle) * (180 / Mathf.PI)) / 4.0F;

            if (owner.target.transform.position.y <= owner.transform.position.y)//Serve??
                cannonInclination.z = -cannonInclination.z;

            cannonInclination.z = Mathf.Clamp(cannonInclination.z, owner.minRange, Math.Abs(owner.minRange));
            owner.AxeXrot.transform.rotation = Quaternion.Slerp(owner.AxeXrot.transform.rotation, Quaternion.Euler(
                cannonInclination.z, owner.AxeYrot.transform.eulerAngles.y, owner.AxeXrot.transform.eulerAngles.z), Time.deltaTime * owner.RotationSpeed);


            if (!(Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.MaxDistance))
            {
                Idle.OnStateEnter();
                return Idle;
            }
            return this;
        }
    }
    #endregion

    #region IAITurret
    public GameObject target;
    public  float minRange;

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
    //Inutilizzati perche inutili
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
