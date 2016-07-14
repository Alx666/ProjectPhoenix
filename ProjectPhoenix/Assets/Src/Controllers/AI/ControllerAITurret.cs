﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Weapon))]
internal class ControllerAITurret : NetworkBehaviour
{
    public GameObject     AxeYrot;
    public GameObject     AxeXrot;
    public float          DetectionRange  = 50f;
    public float          BulletForce;
    public AnimInfo       AnimationParams;
    public TurretAimMode  AimMode         = TurretAimMode.Direct;
    public BallisticMode  Trajectory      = BallisticMode.Hi;
    public Actor Owner { get; set; }

    private Weapon        m_hWeapon;
    private StateIdle     m_hIdle;
    private StatePatrol   m_hPatrol;
    private IState        m_hCurrent;
    private GameObject    m_hTarget;

    [SyncVar(hook = "OnTargetChanged")]
    private bool          m_bHasTarget;

    private void OnTargetChanged(bool b)
    {
        m_bHasTarget = b;
    }

    private void Awake()
    {
        m_hWeapon = this.GetComponent<Weapon>();
        Owner = GetComponent<Actor>();
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

        
    }

    private void Update()
    {
    //    if (m_bHasTarget && m_hTarget == null)
    //    {
    //        RpcSetTarget();
    //    }

        m_hCurrent = m_hCurrent.Update();
    }


    //Serve a notificare tutte le copie di questa istanza sulle altre macchine che il bersaglio e' stato scelto
    [ClientRpc]
    private void RpcSetTarget(NetworkInstanceId netID)
    {
        m_hTarget = ClientScene.FindLocalObject(netID);
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

    private interface IState
    {
        IState  Update();
        IState Next { get; set; }
        void    OnStateEnter();
    }

    private class StateIdle : IState
    {
        private ControllerAITurret m_hOwner;

        public IState Next { get; set; }

        public StateIdle(ControllerAITurret hOwner)
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
        private ControllerAITurret m_hOwner;
        private float                   m_fTime;
        private Quaternion              m_vRotY;
        private Quaternion              m_vRotX;

        public IState Next { get; set; }

        public StatePatrol(ControllerAITurret hOwner)
        {
            m_hOwner = hOwner;
        }

        public void OnStateEnter()
        {
            m_fTime = 0f;
        }
        
        public IState Update()
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
                m_hOwner.AxeYrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AxeYrot.transform.localRotation, m_vRotY, m_hOwner.AnimationParams.AnimationSpeed);
                m_hOwner.AxeXrot.transform.localRotation = Quaternion.Slerp(m_hOwner.AxeXrot.transform.localRotation, m_vRotX, m_hOwner.AnimationParams.AnimationSpeed);
                m_fTime -= Time.deltaTime;
            }

            //on server scan for target
            if (m_hOwner.isServer)
            {
                float fNearest = float.MaxValue;//?da sostituire?
                GameObject hTarget = null;

                GameManager.Instance.scores.Keys.ToList().ForEach(hA =>
                {
                    float fDistance = Vector3.Distance(m_hOwner.transform.position, hA.transform.position);

                    if (fDistance < fNearest)
                    {
                        hTarget = hA.gameObject;
                        fNearest = fDistance;
                    }
                });

                if (fNearest < m_hOwner.DetectionRange)
                {
                    m_hOwner.OnTargetChanged(true);
                    m_hOwner.m_hTarget = hTarget;//il server setta il target
                    m_hOwner.RpcSetTarget(hTarget.GetComponent<PhoenixActor>().netId);//chiama il client
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
        private ControllerAITurret m_hOwner;
        private float m_fShootTimer;

        public IState Next { get; set; }


        public StateAimBallistic(ControllerAITurret hOwner)
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
                m_hOwner.AxeYrot.transform.localRotation = Quaternion.Lerp(m_hOwner.AxeYrot.transform.localRotation, vYRot, m_hOwner.AnimationParams.AnimationSpeed);

                Vector3 vLocToTarget = m_hOwner.m_hTarget.transform.localPosition - m_hOwner.m_hWeapon.ShootLocators[0].transform.localPosition;

                float fAngle;
                if (StateAimBallistic.Aim(m_hOwner.BulletForce, Physics.gravity.y, vLocToTarget.magnitude, vLocToTarget.y, (int)m_hOwner.Trajectory, out fAngle))
                {
                    m_hOwner.AxeXrot.transform.localRotation = Quaternion.AngleAxis(fAngle, Vector3.right);//??
                    
                    if (m_hOwner.isServer && m_fShootTimer < 0f)
                    {
                        m_hOwner.m_hWeapon.Press();
                        m_fShootTimer = 2f;
                    }
                }
                else
                {
                    m_hOwner.m_hWeapon.Release();
                    m_hOwner.m_hTarget = null;
                    Next.OnStateEnter();
                    return Next;
                }

                m_fShootTimer -= Time.deltaTime;

                return this;
            }            
        }

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
        private ControllerAITurret Owner;
        private float m_fShootTimer;
        public StateAimDirect(ControllerAITurret networkTurretController)
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
                Vector3 vDirection = Owner.m_hTarget.transform.position - Owner.AxeYrot.transform.position;
                Quaternion vYRot = Quaternion.LookRotation(vDirection);
                Owner.AxeYrot.transform.localRotation = Quaternion.Lerp(Owner.AxeYrot.transform.localRotation, vYRot, Owner.AnimationParams.AnimationSpeed);
   
                //X axes
                vDirection = Owner.m_hTarget.transform.position - Owner.AxeXrot.transform.position;
                Owner.AxeXrot.transform.localRotation = Quaternion.RotateTowards(Owner.AxeXrot.transform.localRotation, Quaternion.LookRotation(vDirection), Owner.AnimationParams.AnimationSpeed);
                Vector3 clampVector = Owner.AxeXrot.transform.localEulerAngles;
                float anglex = clampVector.x;
                anglex = Utility.ClampAngle(anglex, Owner.AnimationParams.AnimationRangeX, 0);

                Owner.AxeXrot.transform.localRotation = Quaternion.Euler(anglex, 0f, 0f);

                if (Owner.isServer && m_fShootTimer < 0)
                {
                    Owner.m_hWeapon.Press();
                    m_fShootTimer = 2f;
                }

                if (!(Vector3.Distance(Owner.gameObject.transform.position, Owner.m_hTarget.transform.position) <= Owner.DetectionRange))
                {
                    Owner.m_hWeapon.Release();
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


