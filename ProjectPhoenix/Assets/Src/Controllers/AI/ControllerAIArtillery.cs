using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public class ControllerAIArtillery : MonoBehaviour, IControllerAI
{

///////////<Inspector>/////////////////
    public GameObject   AxeYrot;
    public GameObject   AxeXrot;
    public float        minRange;
    public float        maxRange;
    public float        LightRadius     = 50f;
    public float        RotationSpeed   = 10f;
    public bool         Mode;
    ///////////////////////////////////
    private Weapon           m_hWeapon;
    private float            m_fForce;
    private List<GameObject> PlayerList;
    //private float            m_fTolerance = 1f;   //Gli dò 1 grado come angolo di tolleranza
    private BulletPhysics    m_rBullet;
/// ///////////////////////////////////
    public IState CurrentState { get; set; }

    public string DEBUG_STATE;
  
    void Awake()
    {
        
        this.m_hWeapon      = this.GetComponent<Weapon>();
        m_fForce            = m_hWeapon.BulletPrefab.GetComponent<BulletPhysics>().Force;
        
    }
    void Start()
    {
        PlayerList  = FindObjectsOfType<GameObject>().Where(GO => GO.GetComponent<IControllerPlayer>() != null).ToList();
        this.Target = this.SetTarget();
        ///<StateMachine>
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
        stateAttack.Idle = stateIdle;

        //Init
        CurrentState = stateIdle;
        CurrentState.OnStateEnter();

    }
    void Update ()
    {
        this.Target  = this.SetTarget();
        DEBUG_STATE  = CurrentState.ToString();
        CurrentState = CurrentState.OnStateUpdate();
    }
  
    #region FMS
    public interface IState
    {
        void   OnStateEnter();
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
            timer  = Mathf.Clamp(timer - Time.deltaTime, 0f, timer);

            if (timer == 0f)
            {
                Patrol.OnStateEnter();
                return Patrol;
            }

            if ((Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius))
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

            if ((Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius))//True:Se la distanza è minore
            {
                Debug.Log("Bersaglio agganciato");
                Attack.OnStateEnter();
                return Attack;
            }
                return this;   
        }
    }
    class AttackState : IState
    {
        private ControllerAIArtillery owner;

        private Transform Target;
        private Transform XRot;
        private Transform Yrot;

        private float   xAngle;
        private float   yAngle;
  
       public IState Idle { get; internal set; }

        public AttackState(ControllerAIArtillery owner)
        {
            this.owner  = owner;    
        }
        public void OnStateEnter()
        {
            this.Target = owner.target.transform;
            owner.m_hWeapon.Press();
            Debug.Log("MI PREPARO ALL'ATTACCO!");
        }

        public IState OnStateUpdate()
        {
            owner.Attack();

            if (!(Vector3.Distance(owner.gameObject.transform.position, this.Target.position) <= owner.LightRadius))
            {
                Idle.OnStateEnter();
                owner.m_hWeapon.Release();
                return Idle;
            }
            return this;
        }
    }
    #endregion
    #region Internal Metod
    internal float ClampAngle(float angle, float max, float min)
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
    internal GameObject SetTarget()
    {
        return PlayerList.OrderBy(go => Vector3.Distance(go.transform.position, this.transform.position)).First();
    }
    internal float Aim(float v, float g, float x, float y, bool traj)
    {
        Debug.Log("Calcolo del'angolo di tiro");
        float speedsqrt  = Mathf.Pow(v, 2);
        float speedsqrt4 = Mathf.Pow(v, 4);
        float ValoreProvisorio = g * (g * Mathf.Pow(x, 2) + (2 * y * speedsqrt));
        float sqrt = Mathf.Sqrt(speedsqrt4 - ValoreProvisorio);
              sqrt = traj ? sqrt : -sqrt;

        float numerator = speedsqrt + sqrt;
        float argument  = numerator / (g * x) ;
        float angle     = (Mathf.Rad2Deg * Mathf.Atan(argument));

        return angle;
    }
    #endregion
    #region IAITurret
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
    //Inutilizzati perche inutili
    public void Idle()
    {

    }
    public void Patrol()
    {

    }
    public void Attack()
    {
        ////////////// <Y XAXIS> /////////////////
        Vector3 vDirection              = Target.transform.position - AxeYrot.transform.position;
        AxeYrot.transform.localRotation = Quaternion.RotateTowards(AxeYrot.transform.localRotation, Quaternion.LookRotation(vDirection), RotationSpeed);
        AxeYrot.transform.localRotation = Quaternion.Euler(0f, AxeYrot.transform.localRotation.eulerAngles.y, 0f);

        ///////<X AXIS>/////////
        m_rBullet                       = m_hWeapon.BulletPrefab.GetComponent<BulletPhysics>();
        float Distance                  = Vector3.Distance(this.transform.position, target.transform.position);
        Vector3 Distancea               = this.transform.position - target.transform.position;
        float gravity                   = Mathf.Abs( Physics.gravity.y);
        float Velocity_Bullet           = m_rBullet.Force;
        float m_fAngle                  = Aim(Velocity_Bullet, gravity, Distance,Mathf.Abs( Distancea.y), Mode);
        Quaternion ANGLE                = Quaternion.Euler(m_fAngle, 0f, 0f);
        Quaternion Rotationx            = Quaternion.Slerp(AxeXrot.transform.localRotation,ANGLE, RotationSpeed);
        AxeXrot.transform.localRotation = Rotationx;
        ////////////////
    }
    #endregion

}
