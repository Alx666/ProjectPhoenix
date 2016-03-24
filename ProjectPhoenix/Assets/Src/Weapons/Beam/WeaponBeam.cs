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
    private IBeam m_hBeam;

    public Vector3 Direction { get; set; }

    private bool m_bFire;
    private float durationTime;
    private bool m_bRecharging;
    private bool m_bCompleteDischarge;

    public bool IsFiring { get { return m_bFire; } }

    void Awake()
    {
        m_hBeam = this.GetComponentInChildren<IBeam>();
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
            m_hBeam.Enable();
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

        //RaycastHit vHit;
        //Ray vRay = new Ray(ShootLocators, Direction);

        //if (Physics.Raycast(vRay, out vHit))
        //{


        //}
        //    RaycastHit vHit;
        //    Ray vRay = new Ray(ShootLocator.transform.position, Direction);

        //    if (Physics.Raycast(vRay, out vHit))
        //    {
        //        m_hRenderer.SetPosition(0, ShootLocator.transform.position);
        //        m_hRenderer.SetPosition(1, vHit.point);

        //        //HitEffect.transform.position = vHit.point;


        //        //IDamageable hDamageable = vHit.collider.gameObject.GetComponent<IDamageable>();

        //        //if (hDamageable != null)
        //        //{

        //        //    //lerptime for laser color lerp
        //        //    m_fCurrentLerpTime += Time.deltaTime;

        //        //    if (m_fCurrentLerpTime > TargetLerpTime)
        //        //        m_fCurrentLerpTime = TargetLerpTime;

        //        //    float fPerc = m_fCurrentLerpTime / TargetLerpTime;

        //        //    //lerp smoothing
        //        //    //perc = perc * perc * perc * (perc * (6f * perc - 15f) + 10f);
        //        //    //color lerp
        //        //    m_hRenderer.SetColors(Color.Lerp(StartColorWeak, StartColorStrong, fPerc), Color.Lerp(EndColorWeak, EndColorStrong, fPerc));


        //        //    hDamageable.Damage(DPS * Time.deltaTime);                                                
        //        //}


        //    }
        //    else
        //    {
        //        Vector3 vAway = ShootLocator.transform.position + Direction * 500f;
        //        HitEffect.transform.position = vAway;
        //        m_hRenderer.SetPosition(0, ShootLocator.transform.position);
        //        m_hRenderer.SetPosition(1, vAway);
        //    }
    }
}
