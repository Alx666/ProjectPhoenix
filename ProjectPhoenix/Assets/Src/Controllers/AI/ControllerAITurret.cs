using UnityEngine;
using System.Collections;
using System;

//To Do: INSERIRE LO SCRIPT GIUSTO ALLA TORRETTA GIUSTA
//        Clampare le x delle torrette
public class ControllerAITurret : MonoBehaviour, IControllerAI
{
    public GameObject AxeYrot;
    public GameObject AxeXrot;
    public float  minRange;
    public float  maxRange;
    public float  LightRadius   = 50f;
    public float  RotationSpeed = 10f;

    
    public IWeapon Weapon;

    private float tolerance   = 1f;   //Gli dò 1 grado come angolo di tolleranza

    public IState CurrentState { get; set; }

    public string DEBUG_STATE;
 
    void Awake()
    {
        this.Weapon = this.GetComponent<WeaponProjectile>();

        IdleState   stateIdle   = new IdleState(this);
        PatrolState statepatrol = new PatrolState(this);
        AttackState stateAttack = new AttackState(this);
        //Idle
        stateIdle.Patrol = statepatrol;
        stateIdle.Attack = stateAttack;
        //Patrol
        statepatrol.Idle = stateIdle;
        statepatrol.Attack = stateAttack;
        //Attack
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

    #region FMS
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
             if (Vector3.Distance(owner.gameObject.transform.position,owner.Target.transform.position)<= owner.LightRadius)
            {
                Attack.OnStateEnter();
                return Attack;
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
        float xAngle;

        float min;
        float max;

        Transform yRot;
        Transform xRot;

        public PatrolState(ControllerAITurret owner)
        {
            this.owner = owner;

            yRot = this.owner.AxeYrot.transform;
            xRot = this.owner.AxeXrot.transform;

            min = this.owner.minRange;
            max = this.owner.maxRange;
        }

        public void OnStateEnter()
        {
            yAngle = UnityEngine.Random.Range(-180f, 180f);

            xAngle = UnityEngine.Random.Range(min, max);

        }

        public IState OnStateUpdate()
        {

            //Yaxis
            float currentYAngle = yRot.transform.localRotation.eulerAngles.y;
            yRot.transform.localRotation = Quaternion.Slerp(yRot.transform.localRotation, Quaternion.Euler(0, yAngle, 0), owner.RotationSpeed * Time.deltaTime);

            if (currentYAngle > 180f)
            {
                currentYAngle = currentYAngle - 360f;
            }


            //Xaxis
            float currentxAngle = xRot.transform.localRotation.eulerAngles.x;
            xRot.transform.localRotation = Quaternion.Slerp(xRot.transform.localRotation, Quaternion.Euler(xAngle, 0, 0), owner.RotationSpeed * Time.deltaTime);


            if (currentxAngle>0)
            {
                currentxAngle = currentxAngle - 360;
            }
            if(currentxAngle<0)
            {
                currentxAngle = currentxAngle + 360;
            }

            //TOIDLE
            if(currentYAngle >= yAngle - owner.tolerance && currentYAngle <= yAngle + owner.tolerance)
            {
                Idle.OnStateEnter();
                return Idle;
            }

            //TOATTACK
            if (Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius)
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
        private Transform Target;
        private Transform XRot;
        private Transform Yrot;

        private float xAngle;
        private float yAngle;
        private float TargetAngle;


        public IState Idle { get; internal set; }

        public AttackState(ControllerAITurret owner)
        {
            this.owner  = owner;
            this.Target = owner.target.transform;
            this.XRot   = owner.AxeXrot.transform;
            this.Yrot   = owner.AxeYrot.transform;
            this.xAngle = XRot.transform.localEulerAngles.x;
            this.yAngle = Yrot.transform.localEulerAngles.y;
            


        }
        public void OnStateEnter()
        {
          
        }

        public IState OnStateUpdate()
        {
            owner.Weapon.OnFireButtonPressed();
            ////Y axes
            Vector3 vDirection = Target.transform.position - Yrot.transform.position;
            Yrot.transform.localRotation = Quaternion.RotateTowards(Yrot.transform.localRotation, Quaternion.LookRotation(vDirection), owner.RotationSpeed);
            Yrot.transform.localRotation = Quaternion.Euler(0f, Yrot.transform.localRotation.eulerAngles.y, 0f);

            //X axes
            vDirection = Target.transform.position - XRot.transform.position;
            XRot.transform.localRotation = Quaternion.RotateTowards(XRot.transform.localRotation, Quaternion.LookRotation(vDirection), owner.RotationSpeed);
            Vector3 clampVector = XRot.transform.localEulerAngles;
            float anglex = clampVector.x;
            anglex= owner.ClampAngle(anglex,owner.maxRange, owner.minRange);
     
            XRot.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);


            if (!(Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius))
            {
                owner.Weapon.OnFireButtonReleased();
                Idle.OnStateEnter();
                return Idle;
            }

            return this;
        }
    }

    internal float  ClampAngle(float angle,float max,float min)
    {
        if (angle < 90 || angle > 270)
            {       // if angle in the critic region...
                if (angle > 180) angle -= 360;  // convert all angles to -180..+180
                if (max > 180) max -= 360;
                if (min > 180) min -= 360;
            }
            angle = Mathf.Clamp(angle, min, max);
            if (angle < 0) angle += 360;  // if angle negative, convert to 0..360
            return angle;
        }
    #endregion
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
