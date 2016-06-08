using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkIdentity))]             
//Far Si che il locator punti anche senza la grafica    V
//far si che funzioni anche quando manca un asse        V


internal class ControllerAITurret1 : NetworkBehaviour
{
    public GameObject     AsixYrot;
    public GameObject     AsixXrot;
    public GameObject     ShootLocator;
    public GameObject     Weapon;
    public float          DetectionRange  = 50f;
    public float          BulletForce;
    public bool           IsVeicle;
    public bool           AsixX;
    public bool           AsixY;

    public TurretAimMode  AimMode         = TurretAimMode.Direct;
    public BallisticMode  Trajectory      = BallisticMode.Hi;
    public TargetType     typeTarget      = TargetType.Weel; //non necessario

    public AnimInfo       AnimationParams;


    private Weapon        m_hWeapon; //prop

    private StateIdle     m_hIdle;
    private StatePatrol   m_hPatrol;
    private IState        m_hCurrent;
    private GameObject    m_hTarget;//

    [SyncVar(hook = "OnTargetChanged")]
    private bool          m_bHasTarget;

    private void OnTargetChanged(bool b)
    {
        m_bHasTarget = b;
    }

    private void Awake()
    {
        //inizialize State
        m_hIdle             = new StateIdle(this);
        m_hPatrol           = new StatePatrol(this);

        switch ((int)AimMode)
        {
            case 1:
                m_hPatrol.Next = new StateAimBallistic(this); 
                break;
            case 2:
                m_hPatrol.Next = new StateAimDirect(this); 
                break;
        }


        m_hPatrol.Next.Next = m_hPatrol;

        m_hCurrent          = m_hPatrol;
        m_hCurrent.OnStateEnter();

        //
        m_hWeapon = this.Weapon.GetComponent<Weapon>();

        //Check assix
        if (AsixY == false)
            AsixYrot = new GameObject();
        if (AsixX == false)
            AsixXrot = new GameObject();

    }

    private void Update()
    {
        //Check target
        if (m_bHasTarget)
        {
            RpcSetTarget();
        }

        m_hCurrent = m_hCurrent.Update();
    }


    //NetWork
    [ClientRpc]
    private void RpcSetTarget()
    {
        CmdGetTarget(m_hTarget);
    }

    [Command]
    private void CmdGetTarget(GameObject hTarget)
    {
        m_hTarget = hTarget;
    }

    #region Nested Types

    internal enum TurretAimMode
    {
        Ballistic = 1,
        Direct    = 2,
    }

    internal enum BallisticMode
    {
        Low = -1,
        Hi  =  1,        
    }

    internal enum TargetType
    {
        Weel    = 0,
        Mech    = 1,
        Heli    = 2,

    }

    private interface IState
    {
        IState  Update();
        IState Next { get; set; }
        void    OnStateEnter();
    }

    private class StateIdle : IState
    {
        private ControllerAITurret1 m_hOwner;

        public IState Next { get; set; }

        public StateIdle(ControllerAITurret1 hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnStateEnter()
        {
        }
        public IState Update()
        {
            return this;
        }
    }

    private class StatePatrol : IState
    {
        private ControllerAITurret1     m_hOwner;
        private float                   m_fTime;
        private Quaternion              m_vRotY;
        private Quaternion              m_vRotX;

        public IState Next { get; set; }

        public StatePatrol(ControllerAITurret1 hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnStateEnter()
        {
            m_fTime = 0f;
        }
        
        public IState Update()     
        {
            if (m_hOwner.IsVeicle==false)
            {
                //Animation Handling
                if (m_fTime <= 0f)
                {

                    m_vRotY = Quaternion.Euler(0f, UnityEngine.Random.Range(-m_hOwner.AnimationParams.AnimationRangeY, m_hOwner.AnimationParams.AnimationRangeY), 0f);
                    m_vRotX = Quaternion.Euler(-UnityEngine.Random.Range(0f, m_hOwner.AnimationParams.AnimationRangeX), 0f, 0f);
                    m_fTime = m_hOwner.AnimationParams.AnimationTime;


                }
                else
                {
                    m_hOwner.AsixYrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AsixYrot.transform.localRotation, m_vRotY, m_hOwner.AnimationParams.AnimationSpeed);
                    m_hOwner.AsixXrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AsixXrot.transform.localRotation, m_vRotX, m_hOwner.AnimationParams.AnimationSpeed);
                    m_fTime -= Time.deltaTime;
                }
            }
            else
            {
                m_vRotY = Quaternion.Euler(0f,m_hOwner.transform.forward.z, 0f);
                m_vRotX = Quaternion.Euler(m_hOwner.transform.forward.z, 0f, 0f);

                m_hOwner.AsixYrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AsixYrot.transform.localRotation, m_vRotY, m_hOwner.AnimationParams.AnimationSpeed);
                m_hOwner.AsixXrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AsixXrot.transform.localRotation, m_vRotX, m_hOwner.AnimationParams.AnimationSpeed);

            }

            //on server scan for target
            if (m_hOwner.isServer)
            {
                float fNearest = float.MaxValue;//?da sostituire?
                GameObject hTarget = null;
                //for (int i = 0; i < CustomNetworkManager.Players.Count; i++)
                //{

                    GameObject hCurrent = GameObject.Find("player(Clone)");
                    //CustomNetworkManager.Players[i];
                    float fDistance = Vector3.Distance(m_hOwner.transform.position, hCurrent.transform.position);
                    if (fDistance < fNearest)
                    {
                        hTarget = hCurrent;
                        fNearest = fDistance;
                    }
                //}

                if (fNearest < m_hOwner.DetectionRange)
                {
                    m_hOwner.OnTargetChanged(true);
                    m_hOwner.m_hTarget = hTarget;//il server setta il target
                    m_hOwner.RpcSetTarget();//chiama il client
                }
            }

            if (m_hOwner.m_hTarget != null)
            {
                Next.OnStateEnter();
                return Next;
            }
            else
            {
                return this;
            }                                                
        }
    }

    private class StateAimBallistic : IState
    {
        private ControllerAITurret1 m_hOwner;
        private float m_fShootTimer;

        public IState Next { get; set; }


        public StateAimBallistic(ControllerAITurret1 hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnStateEnter()
        {
        }
        public IState Update()
        {
            
            if (m_hOwner.m_hTarget == null)
            {
                Next.OnStateEnter();
                return Next;
            }
            else
            {
                Vector3 vDirection = m_hOwner.m_hTarget.transform.position - m_hOwner.transform.position;
                vDirection.y = 0f;
                vDirection.Normalize();

                Quaternion vYRot = Quaternion.LookRotation(vDirection);
                m_hOwner.AsixYrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AsixYrot.transform.localRotation, vYRot, m_hOwner.AnimationParams.AnimationSpeed);//O LERP

                Vector3 vLocToTarget = m_hOwner.m_hTarget.transform.localPosition - m_hOwner.ShootLocator.transform.localPosition;

                float fAngle;
                if (StateAimBallistic.Aim(m_hOwner.BulletForce, Physics.gravity.y, vLocToTarget.magnitude, vLocToTarget.y, (int)m_hOwner.Trajectory, out fAngle))
                {
                    m_hOwner.AsixXrot.transform.localRotation = Quaternion.AngleAxis(fAngle, Vector3.right);//??

                    //Fire Condition, Only Shoots On Server                    
                    //Tmp Version
                    if (m_hOwner.isServer && m_fShootTimer < 0f)
                    {
                        //TODO:da sostituire
                        BulletPhysics hBullet = (GameObject.Instantiate(m_hOwner.m_hWeapon.BulletPrefab) as GameObject).GetComponent<BulletPhysics>();
                        //hBullet.Shoot(m_hOwner.ShootLocator.transform.position, m_hOwner.ShootLocator.transform.forward);
                        NetworkServer.Spawn(hBullet.gameObject);
                        m_fShootTimer = 2f;
                    }
                }
                else
                {
                    m_hOwner.m_hTarget = null;
                    Next.OnStateEnter();
                    return Next;
                }

                m_fShootTimer -= Time.deltaTime;

                return this;
            }            
        }
        //DA togliere
        private static bool Aim(float fV, float fG, float fX, float fY, int iHigh, out float fAngle)
        {
            fAngle = 0;
            iHigh = Math.Sign(iHigh);
            fG = Mathf.Abs(fG);

            double v2 = Math.Pow(fV, 2);
            double v4 = Math.Pow(fV, 4);
            double gpart = fG * (fG * Math.Pow(fX, 2) + (2 * fY * v2));
            double sqrt = Math.Sqrt(v4 - gpart);
            //    sqrt = traj ? sqrt : -sqrt;
            if (double.IsNaN(sqrt))
                return false;

            double numerator = v2 + iHigh * sqrt;
            double argument = numerator / (fG * fX);
            fAngle = -(float)(Mathf.Rad2Deg * Math.Atan(argument));

            return true;
        }
    }

    private class StateAimDirect : IState
    {
        private ControllerAITurret1 Owner;
        private float m_fShootTimer;
        public StateAimDirect(ControllerAITurret1 networkTurretController)
        {
            this.Owner = networkTurretController;
        }

        public IState Next { get; set; }
        public void OnStateEnter()
        {
           
        }

        public IState Update()
        {
            if (Owner.m_hTarget == null)
            {
                this.Next.OnStateEnter();
                return Next;
            }
            else
            {
                if (Owner.IsVeicle==false)
                {
                    //Y asis
                    Vector3 vDirection  = Owner.m_hTarget.transform.position - Owner.AsixYrot.transform.position;
                    Quaternion vYRot    = Quaternion.LookRotation(vDirection);
                    // Owner.ShootLocator.transform.localRotation = Quaternion.Lerp(Owner.AxeYrot.transform.localRotation, vYRot, Owner.AnimationParams.AnimationSpeed);
                    vYRot.x = 0;
                    vYRot.z = 0;
                    //vYRot=  (Owner.typeTarget==TargetType.Weel)?Quaternion.identity: Quaternion.LookRotation(vDirection);
                    Owner.AsixYrot.transform.localRotation = Quaternion.Lerp(Owner.AsixYrot.transform.localRotation, vYRot, Owner.AnimationParams.AnimationSpeed);

                    //X axes
                    vDirection       = Owner.m_hTarget.transform.position - Owner.AsixXrot.transform.position;
                    Quaternion vXRot = Quaternion.LookRotation(vDirection);
                    Owner.AsixXrot.transform.localRotation = Quaternion.Lerp(Owner.AsixXrot.transform.localRotation, vXRot, Owner.AnimationParams.AnimationSpeed);

                    Vector3 clampVector = Owner.AsixXrot.transform.localEulerAngles;
                    float anglex = clampVector.x;
                    anglex = Utility.ClampAngle(anglex, Owner.AnimationParams.AnimationRangeX, -180f);

                    Owner.AsixXrot.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);
                }
                else
                {
                    //Y Asix
                    Vector3 vDirection  = Owner.m_hTarget.transform.position - Owner.AsixYrot.transform.position;
                    Quaternion vYRot    = Quaternion.LookRotation(vDirection);
                    // Owner.ShootLocator.transform.localRotation = Quaternion.Lerp(Owner.AxeYrot.transform.localRotation, vYRot, Owner.AnimationParams.AnimationSpeed);
                    vYRot.x = 0;
                    vYRot.z = 0;
                    //vYRot=  (Owner.typeTarget==TargetType.Weel)?Quaternion.identity: Quaternion.LookRotation(vDirection);
                    Owner.AsixYrot.transform.localRotation = Quaternion.Lerp(Owner.AsixYrot.transform.localRotation, vYRot, Owner.AnimationParams.AnimationSpeed);

                    //X asix
                    vDirection = Owner.m_hTarget.transform.position - Owner.AsixYrot.transform.position;
                    Owner.ShootLocator.transform.localRotation = Quaternion.Lerp(Owner.ShootLocator.transform.localRotation, Quaternion.LookRotation(vDirection), Owner.AnimationParams.AnimationSpeed);
                    Vector3 clampVector = Owner.ShootLocator.transform.localEulerAngles;
                    float anglex = clampVector.x;
                    anglex = Utility.ClampAngle(anglex, Owner.AnimationParams.AnimationRangeX, -180f);

                    Owner.ShootLocator.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);
                    // Owner.AxeXrot.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);
                }
               

                if (Owner.isServer && m_fShootTimer < 0)
                {
                    //BulletPhysics hBullet = (GameObject.Instantiate(Owner.m_hWeapon.BulletPrefab) as GameObject).GetComponent<BulletPhysics>();
                    //hBullet.Shoot(Owner.ShootLocator.transform.position, Owner.ShootLocator.transform.forward);
                    //NetworkServer.Spawn(hBullet.gameObject);
                  
                    Debug.Log("beng");
                    m_fShootTimer = 2f;
                }

                if (!(Vector3.Distance(Owner.gameObject.transform.position, Owner.m_hTarget.transform.position) <= Owner.DetectionRange))
                {
                    Owner.m_hTarget = null;
                    Next.OnStateEnter();
                    return Next;
                }
                m_fShootTimer -= Time.deltaTime;

            return this;
            }
        }
    }

    [Serializable]
    public class AnimInfo
    {
        public float AnimationSpeed     = 0.1f;
        public float AnimationTime      = 2.0f;
        public float AnimationRangeX    = 45f;
        public float AnimationRangeY    = 180f;
    }


    #endregion

}


