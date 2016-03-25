using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public class ControllerAIArtillery : MonoBehaviour, IControllerAI
{
    public GameObject AxeYrot;
    public GameObject AxeXrot;
    public float minRange;
    public float maxRange;
    public float LightRadius = 50f;
    public float RotationSpeed = 10f;

    private float AngleShot;
    private Weapon m_hWeapon;
    float force;
    private List<GameObject> PlayerList;
    private IWeapon Weapon;
    private float tolerance = 1f;   //Gli dò 1 grado come angolo di tolleranza

    public IState CurrentState { get; set; }

    public string DEBUG_STATE;
  
    void Awake()
    {
        this.Weapon = this.GetComponent<IWeapon>();
        this.m_hWeapon = this.GetComponent<Weapon>();
        force = m_hWeapon.BulletPrefab.GetComponent<BulletPhysics>().Force;
    }
    void Start()
    {
        PlayerList = FindObjectsOfType<GameObject>().Where(GO => GO.GetComponent<IControllerPlayer>() != null).ToList();
        this.Target = this.SetTarget();

        IdleState stateIdle = new IdleState(this);
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
    void Update ()
    {
        this.Target = this.SetTarget();
        DEBUG_STATE  = CurrentState.ToString();
        CurrentState = CurrentState.OnStateUpdate();
    }
    private GameObject SetTarget()
    {
        return PlayerList.OrderBy(go => Vector3.Distance(go.transform.position, this.transform.position)).First();
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

        private float xAngle;
        private float yAngle;
            Vector3 velocity;


       public IState Idle { get; internal set; }


        public AttackState(ControllerAIArtillery owner)
        {
            this.owner  = owner;    
            this.XRot   = owner.AxeXrot.transform;
            this.Yrot   = owner.AxeYrot.transform;
            this.xAngle = XRot.transform.localEulerAngles.x;
            this.yAngle = Yrot.transform.localEulerAngles.y;
        }
        public void OnStateEnter()
        {
            this.Target = owner.target.transform;
        }

        public IState OnStateUpdate()
        {
            // Y axis inclination
            Vector3 vDirection           = Target.transform.position - Yrot.transform.position;
            Yrot.transform.localRotation = Quaternion.RotateTowards(Yrot.transform.localRotation, Quaternion.LookRotation(vDirection), owner.RotationSpeed);
            Yrot.transform.localRotation = Quaternion.Euler(0f, Yrot.transform.localRotation.eulerAngles.y, 0f);

            ///////////////////vecchia implementazone
            float Distance = Vector3.Distance(XRot.transform.position, Target.transform.position);
            float anglex = XRot.transform.rotation.x;
            anglex = owner.ClampAngle(anglex-Distance, owner.maxRange, owner.minRange);
            //XRot.transform.localRotation = Quaternion.Slerp(XRot.transform.localRotation, Quaternion.Euler(angolo, 0f, 0f), owner.RotationSpeed);

            /////////////////2° metodo
            float gravty = Physics.gravity.y;
            float angolo = XRot.transform.rotation.x;
            // float Distance = Vector3.Distance(XRot.transform.position, Target.transform.position);
            float ASIN = Mathf.Clamp01((gravty * Distance) / Mathf.Pow(owner.force, 2f));
            owner.AngleShot = (1f / 2f) * Mathf.Asin( ASIN);
            angolo = (anglex + owner.AngleShot);
            XRot.transform.localRotation =Quaternion.Slerp(XRot.transform.localRotation, Quaternion.Euler(angolo, 0f,0f), owner.RotationSpeed);
            ////////////////

            owner.Weapon.Press();

            if (!(Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius))
            {
                Idle.OnStateEnter();
                owner.Weapon.Release();
                return Idle;
            }
            return this;
        }
    }
    #endregion
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
    }
    #endregion
}
