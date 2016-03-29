using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    public GameObject BulletPrefab;
    public List<GameObject> ShootLocators;

    public bool IsShootAlternate;

    [Range(0f, 90f)]
    public float Spread = 0f;

    [Range(0f, 3f)]
    public float BulletDelay = 0.3f;

    [Range(0.01f, 3f)]
    public float BarrageDelay = 0.3f;

    public int Barrage = 1;

    private Pool m_hPool;
    private IWeaponState m_hStateMachine;
    private WeaponReady m_hTrigger;

    public Vector3 Direction { get; set; }

    private void Awake()
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
                WeaponShoot hShoot = new WeaponShoot(this, IsShootAlternate);
                WeaponWait hWait = new WeaponWait(BulletDelay);

                hLast.Next = hShoot;
                hShoot.Next = hWait;
                hLast = hWait;
            }
        }
        else
        {
            WeaponShoot hShoot = new WeaponShoot(this, Barrage, IsShootAlternate);
            WeaponWait hWait = new WeaponWait(BulletDelay);
            hLast.Next = hShoot;
            hShoot.Next = hWait;
            hLast = hWait;
        }
        WeaponShoot.InitializeQueueLocators(ShootLocators);     //Similar to a static ctor, but can pass parameter

        (hLast as WeaponWait).Delay = BarrageDelay;
        hLast.Next = m_hTrigger;
        m_hStateMachine = hLast;
    }

    private void Update()
    {
        m_hStateMachine = m_hStateMachine.Update();
    }

    public void Press()
    {
        m_hTrigger.OnButtonPressed();
    }

    public void Release()
    {
        m_hTrigger.OnButtonReleased();
    }

    public bool IsFiring
    {
        get { return m_hStateMachine != m_hTrigger; }
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
        private static Queue<GameObject> m_hLocators;

        private Weapon m_hOwner;
        private int m_iShootCount;
        private IShootStrategy m_hShootStrategy;

        public IWeaponState Next { get; set; }

        #region IShootStrategy

        private interface IShootStrategy
        {
            void Shoot();
        }

        private void InstantiateBullet(int index)
        {
            GameObject hNextLocator = m_hLocators.Dequeue();

            Vector3 vPosition = m_hOwner.ShootLocators[index].transform.position;
            Vector3 vDirection = m_hOwner.ShootLocators[index].transform.forward;// sistemato perche altrimenti seguiva la direction del oggetto principale

            if (m_hOwner.Spread > 0f)
            {
                float fRangeX = UnityEngine.Random.Range(-m_hOwner.Spread, m_hOwner.Spread);
                float fRangeY = UnityEngine.Random.Range(-m_hOwner.Spread, m_hOwner.Spread);
                float fRangeZ = UnityEngine.Random.Range(-m_hOwner.Spread, m_hOwner.Spread);

                vDirection = Quaternion.Euler(fRangeX, fRangeY, fRangeZ) * vDirection;
                vDirection.Normalize();
            }

            IBullet hBullet = GlobalFactory.GetInstance<IBullet>(m_hOwner.BulletPrefab);
            hBullet.Shoot(vPosition, vDirection);

            m_hLocators.Enqueue(hNextLocator);

        }

        private class ShootSimultaneous : IShootStrategy
        {
            private WeaponShoot m_hOwner;

            public ShootSimultaneous(WeaponShoot owner)
            {
                m_hOwner = owner;
            }

            public void Shoot()
            {
                for (int i = 0; i < m_hOwner.m_iShootCount; i++)
                {
                    for (int j = 0; j < WeaponShoot.m_hLocators.Count; j++)
                    {
                        m_hOwner.InstantiateBullet(j);
                    }
                }
            }
        }

        private class ShootAlternate : IShootStrategy
        {
            private WeaponShoot m_hOwner;

            public ShootAlternate(WeaponShoot owner)
            {
                m_hOwner = owner;
            }

            public void Shoot()
            {
                for (int i = 0; i < m_hOwner.m_iShootCount; i++)
                {
                    m_hOwner.InstantiateBullet(i);
                }
            }
        }

        #endregion IShootStrategy

        public WeaponShoot(Weapon hWeap, bool bShootAlternate) : this(hWeap, 1, bShootAlternate)
        {
        }

        public WeaponShoot(Weapon hWeap, int iCount, bool bShootAlternate)
        {
            m_hOwner = hWeap;
            m_iShootCount = iCount;

            if (bShootAlternate)
            {
                m_hShootStrategy = new ShootAlternate(this);
            }
            else
            {
                m_hShootStrategy = new ShootSimultaneous(this);
            }
        }

        public IWeaponState Update()
        {
            m_hShootStrategy.Shoot();
            return Next;
        }

        internal static void InitializeQueueLocators(List<GameObject> shootLocators)
        {
            m_hLocators = new Queue<GameObject>(shootLocators);
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

    #endregion Internal State Machine
}