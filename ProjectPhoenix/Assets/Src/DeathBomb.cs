using UnityEngine;
using System.Collections;
using System;

public class DeathBomb : MonoBehaviour
{
    public float BombTimer;
    public float VelocityThreshold;
    public float Frequency = 0.2f;
    public float ActiveTime = 2f;
    public float DeactiveTime = 1.5f;
    public AudioSource Audio;
    private Light m_hLight;

    private Rigidbody m_hRigidbody;
    private float m_fActiveTime;
    private float m_fDeactiveTime;
    private float m_fChangeTime = 0f;
    private float m_fPrevCoeffOn = 50f;
    private float m_fPrevCoeffOff = 50f;
    private float m_fCount;
    private MadMaxActor m_hActor;

    private Active m_hActive;
    private Inactive m_hInactive;
    private Explode m_hExplode;
    private IBomb m_hCurrent;

    void Awake()
    {
        m_hActor = GetComponent<MadMaxActor>();
        m_hRigidbody = GetComponent<Rigidbody>();
        m_fActiveTime = ActiveTime;
        m_fDeactiveTime = DeactiveTime;
        m_hLight = GetComponentInChildren<Light>();

        m_hActive = new Active(this);
        m_hInactive = new Inactive(this);
        m_hExplode = new Explode(this);


        m_hCurrent = m_hInactive;
    }

    void Update()
    {
        m_hCurrent.Action();

        #region test
        //if(m_hRigidbody.velocity.magnitude < VelocityThreshold)
        //{
        //    m_fCount += Time.deltaTime;

        //    if (m_fCount >= BombTimer)
        //    {
        //        m_fCount = 0f;
        //        m_hActor.OnDeathBombTimeout();
        //    }

        //    if (Time.time >= m_fChangeTime)
        //    {
        //        Light.enabled = !Light.enabled;

        //        if (Light.enabled)
        //        {
        //            m_fChangeTime = Time.time + m_fActiveTime;
        //            m_fActiveTime -= Frequency;

        //            //float fCurCoeff = (Frequency / m_hRigidbody.velocity.magnitude);

        //            //Debug.Log("Magnitude: " + m_hRigidbody.velocity.magnitude + " Coeff: " + fCurCoeff);

        //            //if (fCurCoeff <= m_fPrevCoeffOn)
        //            //    m_fActiveTime -= fCurCoeff;
        //            //else
        //            //    m_fActiveTime += fCurCoeff;

        //            //m_fPrevCoeffOn = fCurCoeff;
        //        }
        //        else
        //        {
        //            m_fChangeTime = Time.time + m_fDeactiveTime;
        //            m_fDeactiveTime -= Frequency;
        //            //float fCurCoeff = (Frequency / m_hRigidbody.velocity.magnitude);

        //            //if (fCurCoeff <= m_fPrevCoeffOff)
        //            //    m_fDeactiveTime -= fCurCoeff;
        //            //else
        //            //    m_fDeactiveTime += fCurCoeff;

        //            //m_fPrevCoeffOff = fCurCoeff;
        //        }
        //    }

        //}
        //else
        //{
        //    Light.enabled = false;
        //    Reset();
        //}
        #endregion
    }

    public void Reset()
    {
        m_fCount = 0f;
        m_fChangeTime = 0f;
        m_fActiveTime = ActiveTime;
        m_fDeactiveTime = DeactiveTime;
    }

    private interface IBomb
    {
        void Action();
    }

    public class Active : IBomb
    {
        DeathBomb m_hDB;

        public Active(DeathBomb Db)
        {
            m_hDB = Db;
        }

        public void Action()
        {
            //Check to see if the bomb has to explode
            if (m_hDB.m_fCount >= m_hDB.BombTimer)
                m_hDB.m_hCurrent = m_hDB.m_hExplode;

            //Check to see if we r still mlving slowly
            if (m_hDB.m_hRigidbody.velocity.magnitude < m_hDB.VelocityThreshold)
            {
                m_hDB.m_fCount += Time.deltaTime;

                if (Time.time >= m_hDB.m_fChangeTime)
                {
                    m_hDB.m_hLight.enabled = !m_hDB.m_hLight.enabled;

                    if (m_hDB.m_hLight.enabled)
                    {
                        m_hDB.m_fChangeTime = Time.time + m_hDB.m_fActiveTime;
                        m_hDB.m_fActiveTime -= m_hDB.Frequency;
                    }
                    else
                    {
                        m_hDB.m_fChangeTime = Time.time + m_hDB.m_fDeactiveTime;
                        m_hDB.m_fDeactiveTime -= m_hDB.Frequency;
                    }
                }
            }
            else
                m_hDB.m_hCurrent = m_hDB.m_hInactive;
        }
    }

    public class Inactive : IBomb
    {
        DeathBomb m_hDB;
        private bool Done = false;

        public Inactive(DeathBomb Db)
        {
            m_hDB = Db;
        }

        public void Action()
        {
            if (m_hDB.m_hRigidbody.velocity.magnitude > m_hDB.VelocityThreshold && !Done)
            {
                m_hDB.m_hLight.enabled = false;
                m_hDB.Reset();
                Done = true;
            }
            else
            {
                Done = false;
                m_hDB.m_hCurrent = m_hDB.m_hActive;
            }
        }
    }

    public class Explode : IBomb
    {
        DeathBomb m_hDB;

        public Explode(DeathBomb Db)
        {
            m_hDB = Db;
        }

        public void Action()
        {
            m_hDB.m_fCount = 0f;
            m_hDB.m_hActor.OnDeathBombTimeout();
            m_hDB.m_hCurrent = m_hDB.m_hInactive;
        }
    }
}
