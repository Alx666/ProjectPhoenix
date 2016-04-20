using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class ControllerAITurret : MonoBehaviour, IControllerAI
{
 /////////////////////////////////////////
    public GameObject AxeYrot;
    public GameObject AxeXrot;
    public float      minRange;
    public float      maxRange;
    [Range(0f,100f)]
    public float  LightRadius;
    [Range(0f,50f)]
    public float  RotationSpeed;
///////////////////////////////////////////////////////////
    internal  Weapon Weapon;
    public    float tolerance;   //Gli dò 1 grado come angolo di tolleranza
    private   List<GameObject> PlayerList;
///////////////////////////////////////////////////
    public IState CurrentState { get; set; }
    public string DEBUG_STATE;
    public string Debug_Target;

    void Awake()
    {
        this.Weapon = this.GetComponent<Weapon>();     
    }

	void Start ()
    {
        PlayerList  = FindObjectsOfType<GameObject>().Where(GO => GO.GetComponent<IControllerPlayer>() != null).ToList();
        this.Target = this.SetTarget();

        IdleState   stateIdle   = new IdleState(this);
        PatrolState statepatrol = new PatrolState(this);
        AttackState stateAttack = new AttackState(this);
        AimState    stateAim    = new AimState(this);
        //Idle

        //Patrol
        statepatrol.Aim   = stateAim;
        //aim
        stateAim.Attack = stateAttack;
        stateAim.Patrol = statepatrol;
        //Attack
        stateAttack.patrol = statepatrol;
        stateAttack.Aim    = stateAim;
        //Init
        CurrentState = statepatrol;
        CurrentState.OnStateEnter();

    }
	void Update ()
    {

        this.Target  = this.SetTarget();
        DEBUG_STATE  = CurrentState.ToString();
        Debug_Target = target.name.ToString();
        CurrentState = CurrentState.OnStateUpdate();
       
    }

    private GameObject SetTarget()
    {
       return PlayerList.OrderBy(go => Vector3.Distance(go.transform.position, this.transform.position)).First();
    }

    #region FMS
    public interface IState
    {
        void    OnStateEnter();
        IState  OnStateUpdate();
    }

    class IdleState : IState
    {
        ControllerAITurret owner;

        public IdleState(ControllerAITurret owner)
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
        private ControllerAITurret owner;

        public AttackState Attack { get; internal set; }

        public AimState Aim { get; internal set; }

        float yAngle;
        float xAngle;
        float max;
        float min;
        Transform yRot;
        Transform xRot;

        public PatrolState(ControllerAITurret owner)
        {
            this.owner = owner;

            yRot = this.owner.AxeYrot.transform;
            xRot = this.owner.AxeXrot.transform;

            max = -this.owner.maxRange; // è in  negativo perche la rotazione  verso l'alto sulle x è in negativo
            min = this.owner.minRange;
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

            currentxAngle = owner.ClampAngle(currentxAngle, max, min);

         
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

        private ControllerAITurret Owner;
        private Transform Target;

        public AimState(ControllerAITurret owner)
        {
            Owner = owner;
        }

        public void OnStateEnter()
        {
            this.Target = Owner.Target.transform;
        }

        public IState OnStateUpdate()
        {
            ///////<YAXIS>/////////           
            ////Y axes
            Vector3 vDirection = Target.transform.position - Owner.AxeYrot.transform.position;
            Owner.AxeYrot.transform.localRotation = Quaternion.RotateTowards(Owner.AxeYrot.transform.localRotation, Quaternion.LookRotation(vDirection), Owner.RotationSpeed);
            Owner.AxeYrot.transform.localRotation = Quaternion.Euler(0f, Owner.AxeYrot.transform.localRotation.eulerAngles.y, 0f);

            //X axes
            vDirection = Target.transform.position - Owner.AxeXrot.transform.position;
            Owner.AxeXrot.transform.localRotation = Quaternion.RotateTowards(Owner.AxeXrot.transform.localRotation, Quaternion.LookRotation(vDirection), Owner.RotationSpeed);
            Vector3 clampVector = Owner.AxeXrot.transform.localEulerAngles;
            float anglex = clampVector.x;
            anglex = Owner.ClampAngle(anglex, Owner.maxRange, Owner.minRange);

            Owner.AxeXrot.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);

            if (Owner.OnLine(Owner.Weapon.ShootLocators[0].transform, Owner.Target.transform) <= Owner.tolerance)
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
        private ControllerAITurret Owner;
        private Transform Target;
        private float timer;

        public AimState Aim { get; internal set; }
        public PatrolState patrol { get; internal set; }

        public AttackState(ControllerAITurret owner)
        {
            this.Owner = owner;
        }

        public void OnStateEnter()
        {
            this.Target = Owner.Target.transform;
            timer = UnityEngine.Random.Range(1f, 3f);
        }

        public IState OnStateUpdate()
        {

            if (timer == 0f)
            {
                timer = UnityEngine.Random.Range(1f, 3f);
                Owner.Weapon.Press();
            }
            else
                Owner.Weapon.Release();

            timer = Mathf.Clamp(timer - Time.deltaTime, 0f, timer);


            if (!(Vector3.Distance(Owner.gameObject.transform.position, this.Target.position) <= Owner.LightRadius))
            {
                Owner.Weapon.Release();
                patrol.OnStateEnter();
                return patrol;
            }

            if (!(Owner.OnLine(Owner.Weapon.ShootLocators[0].transform, Owner.target.transform) <= Owner.tolerance))
            {
                Owner.Weapon.Release();
                Aim.OnStateEnter();
                return Aim;
            }

            return this;
        }
    }


    #endregion
    #region IAITurret
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
    internal float OnLine(Transform transform1, Transform transform2)
    {

        //Locator
        Vector3 VecLoc = transform1.forward;
        VecLoc.y = 0;
        VecLoc = VecLoc.normalized;

        //TargetLoc
        Vector3 Tar2d = transform2.position;
        Tar2d.y = 0;
        Tar2d = Tar2d.normalized;
        //position locator
        Vector3 This2d = transform1.position;
        This2d.y = 0;
        Vector3 Distance2d = (This2d - Tar2d).normalized;
        float OnLine = Vector3.Angle(VecLoc, Distance2d);

        return OnLine;
    }
    private  GameObject target;
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
