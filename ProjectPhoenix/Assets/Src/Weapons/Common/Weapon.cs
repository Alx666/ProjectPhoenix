using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Weapon : NetworkBehaviour, IWeapon
{
    public GameObject BulletPrefab;
    public List<GameObject> ShootLocators;
    private Queue<GameObject> QueueLocators;


    public ShootMode Mode;

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
    public bool IsReady { get; private set; }

    public Vector3 Direction { get; set; }

    private void Awake()
    {
        QueueLocators = new Queue<GameObject>(ShootLocators);

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
                WeaponShoot hShoot = new WeaponShoot(this, Mode);
                WeaponWait hWait = new WeaponWait(BulletDelay);

                hLast.Next = hShoot;
                hShoot.Next = hWait;
                hLast = hWait;
            }
        }
        else
        {
            WeaponShoot hShoot = new WeaponShoot(this, Barrage, Mode);
            WeaponWait hWait = new WeaponWait(BulletDelay);
            hLast.Next = hShoot;
            hShoot.Next = hWait;
            hLast = hWait;
        }


        (hLast as WeaponWait).Delay = BarrageDelay;
        hLast.Next = m_hTrigger;
        m_hStateMachine = hLast;
    }

    private void Update()
    {
        m_hStateMachine = m_hStateMachine.Update();
        IsReady = m_hStateMachine as WeaponReady != null;
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



    #region WeaponShoot
    private class WeaponShoot : IWeaponState
    {
        private Weapon m_hOwner;
        private int m_iShootCount;
        private IShootStrategy m_hShootStrategy;

        public IWeaponState Next { get; set; }


        #region ShootMode
        public class ShootModeFactory
        {
            private Dictionary<ShootMode, IShootStrategy> ShootModes;

            public ShootModeFactory(WeaponShoot owner)
            {
                ShootModes = new Dictionary<ShootMode, IShootStrategy>();
                ShootModes.Add(ShootMode.Simultaneous, new ShootSimultaneous(owner));
                ShootModes.Add(ShootMode.AlternateA,   new ShootAlternateA(owner));
                ShootModes.Add(ShootMode.AlternateB,   new ShootAlternateB());
            }

            internal IShootStrategy GetMode(ShootMode hMode)
            {
                IShootStrategy res;
                if (ShootModes.TryGetValue(hMode, out res))
                    return res;
                else
                    throw new UnityException("Unknown Implementation of ShootMode");
            }
        }

        public class ShootSimultaneous : IShootStrategy
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
                    for (int j = 0; j < m_hOwner.m_hOwner.QueueLocators.Count; j++)
                    {
                        m_hOwner.InstantiateBullet(j);
                    }
                }
            }
        }
        public class ShootAlternateA : IShootStrategy
        {
            private WeaponShoot m_hOwner;

            public ShootAlternateA(WeaponShoot owner)
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
        public class ShootAlternateB : IShootStrategy
        {
            public void Shoot()
            {
                throw new NotImplementedException();
            }
        }
        #endregion


        #region CommonToShootModes
        private void InstantiateBullet(int index)
        {
            GameObject hNextLocator = m_hOwner.QueueLocators.Dequeue();

            Vector3 vPosition   = hNextLocator.transform.position;
            Vector3 vDirection  = hNextLocator.transform.forward;// sistemato perche altrimenti seguiva la direction del oggetto principale

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

            m_hOwner.QueueLocators.Enqueue(hNextLocator);

        }
        #endregion IShootStrategy

        public WeaponShoot(Weapon hWeap, ShootMode hMode) : this(hWeap, 1, hMode)
        {

        }

        public WeaponShoot(Weapon hWeap, int iCount, ShootMode hMode)
        {
            m_hOwner = hWeap;
            m_iShootCount = iCount;

            ShootModeFactory shootFactory = new ShootModeFactory(this);
            m_hShootStrategy = shootFactory.GetMode(hMode);
        }

        public IWeaponState Update()
        {
            m_hShootStrategy.Shoot();
            return Next;
        }
        
    }

    #endregion



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

#region ENUM
public interface IShootStrategy
{
    void Shoot();
}

public enum ShootMode
{
    Simultaneous,
    AlternateA,
    AlternateB
}
#endregion