using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public class ControllerAIArtillery : MonoBehaviour, IControllerAI,IDamageable
{
///////////<Inspector>/////////////////
  
    public GameObject AxeYrot;
    public GameObject AxeXrot;
    public float Hp;
    public float maxAngleRotation;
    public float minAngleRotation;

    [Range(0f, 100f)]
    public float LightRadius;
    [Range(0f, 50f)]
    public float RotationSpeed;
 

///////////////////////////////////
    private Weapon           m_hWeapon;
   
    private float            tolerance=+0.5f;
    private List<GameObject> PlayerList;
    //private float            m_fTolerance = 1f;   //Gli dò 1 grado come angolo di tolleranza
    private BulletPhysics    m_hBullet;
//////////////////////////////////////
    public IState CurrentState { get; set; }
    public string DEBUG_STATE;
  
    void Awake()
    {       
        this.m_hWeapon      = this.GetComponent<Weapon>();
        m_hBullet           = m_hWeapon.BulletPrefab.GetComponent<BulletPhysics>();

        ///<StateMachine>
        IdleState   stateIdle       = new IdleState(this);
        PatrolState statepatrol     = new PatrolState(this);
        AimState    stateAim        = new AimState(this);
        AttackState stateAttack     = new AttackState(this);
        //Idle
       
        //Patrol
      
        statepatrol.Attack  = stateAttack;
        statepatrol.Aim     = stateAim;
    
        //Aim

        stateAim.Patrol = statepatrol;
        stateAim.Attack = stateAttack;

        //Attack
        stateAttack.Aim     = stateAim;
        stateAttack.patrol  = statepatrol;

        //Init
        CurrentState = statepatrol;
        CurrentState.OnStateEnter();

    }

    void Start()
    {
        PlayerList  = FindObjectsOfType<GameObject>().Where(GO => GO.GetComponent<IControllerPlayer>() != null).ToList();
        this.Target = this.SetTarget();
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
       
        public IdleState(ControllerAIArtillery owner)
        {
            this.owner = owner;
        }

        public void OnStateEnter()
        {
        }
        public IState OnStateUpdate()
        {
          
            return this;
        }
    }

    class PatrolState : IState
    {
        private ControllerAIArtillery owner;

        public AttackState Attack { get; internal set; }
       
        public AimState Aim { get; internal set; }

        float     yAngle;
        float     xAngle;
        float     max;
        float     min;
        Transform yRot;
        Transform xRot;
       
        public PatrolState(ControllerAIArtillery owner)
        {
            this.owner = owner;

            yRot = this.owner.AxeYrot.transform;
            xRot = this.owner.AxeXrot.transform;

            max = -this.owner.maxAngleRotation; // è in  negativo perche la rotazione  verso l'alto sulle x è in negativo
            min =  this.owner.minAngleRotation;
        }

        public void OnStateEnter()
        {         //Calcolo l'angolo di rotazione
            yAngle = UnityEngine.Random.Range(-180f, 180f);
            xAngle = UnityEngine.Random.Range(max, min);
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

            currentxAngle =   owner.ClampAngle(currentxAngle, max, min);

            //TOIDLE
            if (currentYAngle >= yAngle - owner.tolerance && currentYAngle <= yAngle + owner.tolerance ||
                    currentxAngle >= xAngle - owner.tolerance && currentxAngle <= xAngle + owner.tolerance)
            {
                this.OnStateEnter();
                return this;
            }

            //Aim condition
            if ((Vector3.Distance(owner.gameObject.transform.position, owner.Target.transform.position) <= owner.LightRadius))
            {
                Aim.OnStateEnter();
                return Aim;
            }
                return this;   
        }
    }


    class AimState : IState
    {
        public PatrolState Patrol { get; internal set; }
        public AttackState Attack { get; internal set; }

        private ControllerAIArtillery Owner;
        private Transform Target;
       
        public AimState(ControllerAIArtillery owner)
        {
            Owner = owner;
        }

        public void OnStateEnter()
        {
            this.Target = Owner.target.transform;
        }

        public IState OnStateUpdate()
        {
            ///////<YAXIS>/////////           
            Vector3 vDirection = Target.transform.position - Owner.AxeYrot.transform.position;
            Owner.AxeYrot.transform.localRotation = Quaternion.RotateTowards(Owner.AxeYrot.transform.localRotation, Quaternion.LookRotation(vDirection), Owner.RotationSpeed);
            Owner.AxeYrot.transform.localRotation = Quaternion.Euler(0f, Owner.AxeYrot.transform.localRotation.eulerAngles.y, 0f);

            ///////<X AXIS>/////////
            Owner.m_hBullet         = Owner.m_hWeapon.BulletPrefab.GetComponent<BulletPhysics>();

            float   Distance        = Vector3.Distance(Owner.transform.position, Owner.target.transform.position);
            Vector3 Distancea       = Owner.target.transform.position- Owner.m_hWeapon.ShootLocators[0].transform.position;
            float   gravity         = Mathf.Abs(Physics.gravity.y);
            float   Velocity_Bullet = Owner.m_hBullet.Force;
            float   m_fAngle = 0;
            bool    aim = Owner.aimAngle(Velocity_Bullet, gravity, Distance,Distancea.y, out m_fAngle);

                    Owner.AxeXrot.transform.localRotation = Quaternion.AngleAxis(m_fAngle, Vector3.right);

            float Parallel = Vector3.Dot(Owner.m_hWeapon.ShootLocators[0].transform.forward, Owner.target.transform.forward);

            if ( Parallel <= -0.97f) 
            {
                Debug.Log("Bersaglio agganciato");
                Debug.Log("sto per attaccare");

                Attack.OnStateEnter();
                return Attack;
            }
        
            if (!(Vector3.Distance(Owner.gameObject.transform.position, this.Target.position) <= Owner.LightRadius))
            {
                Patrol.OnStateEnter();
                return Patrol;
            }

            return this;
        }
    }

    class AttackState : IState
    {
        private ControllerAIArtillery Owner;
        private Transform Target;
        private float timer;
       
        public AimState     Aim    { get; internal set; }
        public PatrolState  patrol { get; internal set; }

        public AttackState(ControllerAIArtillery owner)
        {
            this.Owner  = owner;    
        }

        public void OnStateEnter()
        {
            this.Target = Owner.target.transform;
            timer = UnityEngine.Random.Range(1f, 3f);
        }

        public IState OnStateUpdate()
        {
            if (timer == 0f)
            {
                timer = UnityEngine.Random.Range(1f, 3f);
                Owner.m_hWeapon.Press();
            }
            else
                Owner.m_hWeapon.Release();

            timer = Mathf.Clamp(timer - Time.deltaTime, 0f, timer);
           

            if (!(Vector3.Distance(Owner.gameObject.transform.position, this.Target.position) <= Owner.LightRadius))
            {
                Owner.m_hWeapon.Release();
                patrol.OnStateEnter();
                return patrol;
            }

            float Parallel = Vector3.Dot(Owner.m_hWeapon.ShootLocators[0].transform.forward, Owner.target.transform.forward);
            Debug.Log(Parallel);
            if ( Parallel >-0.97f)
            {
                Owner.m_hWeapon.Release();

                Aim.OnStateEnter();
                return Aim;
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
    internal bool aimAngle(double v, double g, double x, double y, out float angle)
    {
         angle = 0;

        double v2  = Math.Pow(v, 2);
        double v4  = Math.Pow(v, 4);
        double gpart = g * (g * Math.Pow(x, 2) + (2 * y * v2));
        double sqrt = Math.Sqrt(v4 - gpart);
        //    sqrt = traj ? sqrt : -sqrt;
        if (double.IsNaN(sqrt))
            return false;

        double numerator = v2 - sqrt;
        double argument  = numerator / (g * x) ;
        angle     = -(float)(Mathf.Rad2Deg * Math.Atan(argument));

        return true;
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
    public void Aim()
    {

        ////////////// <Y XAXIS> /////////////////

    }
    public void Attack()
    {
   
    }

    public void Damage(float fDmg)
    {
        this.Hp -= fDmg;
    }

    public void Damage(object damage)
    {
       
    }


    #endregion

   
}
