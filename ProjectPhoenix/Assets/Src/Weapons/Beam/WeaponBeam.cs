using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class WeaponBeam : MonoBehaviour, IWeapon
{
    public float ActiveTime = 5f;
    public float Cooldown = 2f;

    public List<GameObject> ShootLocators;
    public GameObject Beam;
    public Actor Owner { get; set; }

    private Fire m_hFire;
    private WeaponOff m_hWeaponOff;
    private WeaponOn m_hWeaponOn;
    private StartRecharging m_hStartRecharging;
    private Recharging m_hRecharging;

    private IBeam m_hBeam;

    private bool m_bFire;
    private float m_fDurationTime;
    private bool m_bCompleteDischarge;
    private IRecharge m_hCurrent;
    public bool IsFiring { get { return m_bFire; } }
    public Vector3 Direction { get; set; }

    void Awake()
    {
        Owner = GetComponent<Actor>();
        GameObject tmp = Instantiate(Beam);
        //tmp.transform.parent = this.transform;
        m_hBeam = tmp.GetComponent<IBeam>();
        m_hBeam.Owner = this.GetComponent<Actor>();
        m_fDurationTime = ActiveTime;

        m_hWeaponOff = new WeaponOff(this);
        m_hWeaponOn = new WeaponOn(this);
        m_hFire = new Fire(this);
        m_hStartRecharging = new StartRecharging(this);
        m_hRecharging = new Recharging(this);

        m_hCurrent = m_hWeaponOff;
    }

    void Update()
    {
        m_hCurrent.Action();
    }
    public void Press()
    {
        m_bFire = true;
    }

    public void Release()
    {
        m_hBeam.Disable();
        m_bFire = false;
    }

    #region StateMachine

    private interface IRecharge
    {
        void Action();
    }

    private class Fire : IRecharge
    {
        WeaponBeam m_hWB;
        public Fire(WeaponBeam wB)
        {
            m_hWB = wB;
        }

        public void Action()
        {
            if (m_hWB.m_bFire && m_hWB.m_fDurationTime > 0f && !m_hWB.m_bCompleteDischarge)
            {
                m_hWB.ShootLocators.ForEach(hS => m_hWB.Direction = hS.transform.forward);
                m_hWB.m_hBeam.Enable(m_hWB.ShootLocators.First().transform.position, m_hWB.Direction);
                m_hWB.m_fDurationTime -= Time.deltaTime;
            }
            else
                m_hWB.m_hCurrent = m_hWB.m_hStartRecharging;
        }
    }

    private class StartRecharging : IRecharge
    {
        WeaponBeam m_hWB;
        public StartRecharging(WeaponBeam wB)
        {
            m_hWB = wB;
        }
        public void Action()
        {
            if (m_hWB.m_fDurationTime < 0.0f)
                m_hWB.m_bCompleteDischarge = true;

            m_hWB.m_hBeam.Disable();
            m_hWB.m_hCurrent = m_hWB.m_hRecharging;
        }
    }

    private class Recharging : IRecharge
    {
        WeaponBeam m_hWB;
        public Recharging(WeaponBeam wB)
        {
            m_hWB = wB;
        }
        public void Action()
        {
            if (m_hWB.m_fDurationTime < m_hWB.ActiveTime)
            {
                m_hWB.m_fDurationTime += m_hWB.Cooldown * Time.deltaTime;
                return;
            }
            else
            {
                m_hWB.m_bCompleteDischarge = false;
                m_hWB.m_hCurrent = m_hWB.m_hWeaponOff;
            }
        }
    }

    private class WeaponOff : IRecharge
    {
        WeaponBeam m_hWB;
        public WeaponOff(WeaponBeam wB)
        {
            m_hWB = wB;
        }
        public void Action()
        {
            if (!m_hWB.m_bFire)
            {
                return;
            }
            else
                m_hWB.m_hCurrent = m_hWB.m_hWeaponOn;
        }
    }

    private class WeaponOn : IRecharge
    {
        WeaponBeam m_hWB;
        public WeaponOn(WeaponBeam wB)
        {
            m_hWB = wB;
        }
        public void Action()
        {
            m_hWB.m_hCurrent = m_hWB.m_hFire;
        }
    }

    #endregion
}
