using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponProjectile : MonoBehaviour, IWeapon
{
    public GameObject BulletPrefab;
    public AudioSource FireSFX;
    public GameObject Muzzle;
    public List<GameObject> ShootLocators;

    [Range(0f, 90f)]
    public float Spread = 0f;

    [Range(0f, 3f)]
    public float BulletDelay = 0.3f;

    public float Force = 50f;

    [Range(0.01f, 3f)]
    public float BarrageDelay = 0.3f;

    public int Barrage = 1;

    private Pool m_hPool;
    private IWeaponState m_hStateMachine;
    private WeaponReady m_hTrigger;

    void Awake()
    {
        m_hPool = GlobalFactory.GetPool(BulletPrefab);

        //Automatic State Machine Composition
        //One Ready State
        m_hTrigger = new WeaponReady();

        //N Wait And Shoot for Barrage Implemenation
        IWeaponState hLast = m_hTrigger;

        if (BulletDelay > 0f)
        {
            for (int i = 0; i < Barrage; i++)
            {
                WeaponShoot hShoot = new WeaponShoot(this);
                WeaponWait hWait = new WeaponWait(BulletDelay);

                hLast.Next = hShoot;
                hShoot.Next = hWait;
                hLast = hWait;
            }

        }
        else
        {
            WeaponShoot hShoot = new WeaponShoot(this, Barrage);
            WeaponWait hWait = new WeaponWait(BulletDelay);
            hLast.Next = hShoot;
            hShoot.Next = hWait;
            hLast = hWait;
        }

        (hLast as WeaponWait).Delay = BarrageDelay;
        hLast.Next = m_hTrigger;
        m_hStateMachine = hLast;
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
        m_hStateMachine = m_hStateMachine.Update();
    }

    public void OnFireButtonReleased()
    {
        m_hTrigger.OnButtonReleased();
    }

    public void OnFireButtonPressed()
    {
        m_hTrigger.OnButtonPressed();
    }

    public bool IsFiring
    {
        get { return m_hStateMachine != m_hTrigger; }
    }


    public void OnUpdate()
    {

    }

    #region Internal State Machine

    private interface IWeaponState
    {
        IWeaponState Update();
        IWeaponState Next { get; set; }
    }

    private class WeaponWait : IWeaponState
    {
        public float Delay;
        private float m_fElapsedTime;

        public IWeaponState Next { get; set; }

        public WeaponWait(float fDelay)
        {
            Delay = fDelay;
            m_fElapsedTime = Delay;
        }
        public IWeaponState Update()
        {
            if (m_fElapsedTime <= 0f)
            {
                m_fElapsedTime = Delay;
                return Next;
            }
            else
            {
                m_fElapsedTime -= Time.deltaTime;
                return this;
            }
        }
    }

    private class WeaponShoot : IWeaponState
    {
        private WeaponProjectile m_hOwner;
        private int m_iShootCount;

        public IWeaponState Next { get; set; }


        public WeaponShoot(WeaponProjectile hWeap) : this(hWeap, 1)
        {
            m_hOwner = hWeap;
        }

        public WeaponShoot(WeaponProjectile hWeap, int iCount)
        {
            m_hOwner = hWeap;
            m_iShootCount = iCount;
        }

        public IWeaponState Update()
        {
            for (int i = 0; i < m_iShootCount; i++)
            {
                for (int j = 0; j < m_hOwner.ShootLocators.Count; j++)
                {
                    Vector3 vPosition = m_hOwner.ShootLocators[j].transform.position;

                    Vector3 vDirection;

                    if (m_hOwner.Spread > 0f)
                    {
                        float fRange = UnityEngine.Random.Range(-m_hOwner.Spread, m_hOwner.Spread);
                        vDirection = Quaternion.Euler(0f, fRange, 0f) * m_hOwner.ShootLocators[j].transform.forward;
                        vDirection.Normalize();
                    }
                    else
                    {
                        vDirection = m_hOwner.ShootLocators[j].transform.forward;
                    }

                    IBullet hBullet = GlobalFactory.GetInstance<IBullet>(m_hOwner.BulletPrefab);
                    hBullet.Shoot(vPosition, vDirection, m_hOwner.Force, ForceMode.VelocityChange);
                    GameObject.Instantiate(m_hOwner.Muzzle, vPosition, m_hOwner.ShootLocators[j].transform.rotation);

                    m_hOwner.FireSFX.Play();
                }              
            }
            return Next;
        }
    }

    private class WeaponReady : IWeaponState
    {
        public bool m_bFire;

        public IWeaponState Next { get; set; }


        public IWeaponState Update()
        {
            if (m_bFire)
            {
                return Next;
            }
            else
            {
                return this;
            }
        }

        public void OnButtonPressed()
        {
            m_bFire = true;
        }

        public void OnButtonReleased()
        {
            m_bFire = false;
        }
    }

    #endregion
}

