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

    private IBeam m_hBeam;
    public Vector3 Direction { get; set; }

    private bool m_bFire;
    private float durationTime;
    private bool m_bRecharging;
    private bool m_bCompleteDischarge;

    public bool IsFiring { get { return m_bFire; } }

    void Awake()
    {
        GameObject tmp = Instantiate(Beam);
        m_hBeam = tmp.GetComponent<IBeam>();
        durationTime = ActiveTime;
    }

    void Update()
    {
        Recharge();
        Fire();
    }

    private void Recharge()
    {
        if (m_bFire && durationTime > 0f && !m_bCompleteDischarge)
        {
            m_bRecharging = false;
            durationTime -= Time.deltaTime;
        }

        if (durationTime < 0.0f)
        {
            m_bCompleteDischarge = true;
            m_hBeam.Disable();
            m_bRecharging = true;
        }

        if (m_bRecharging)
        {
            if (durationTime < ActiveTime)
            {
                durationTime += Cooldown * Time.deltaTime;
                return;
            }
            else
            {
                m_bCompleteDischarge = false;
                m_bRecharging = false;
            }
        }

        if (!m_bFire)
        {
            m_bRecharging = true;
            return;
        }
        else
            m_hBeam.Enable(ShootLocators.First().transform.position, Direction);
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

    private void Fire()
    {
        ShootLocators.ForEach(hS => hS.transform.forward = Direction);
    }
}
