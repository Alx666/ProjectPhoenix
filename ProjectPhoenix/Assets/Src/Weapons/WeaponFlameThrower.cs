using UnityEngine;
using System.Collections;
using System;

public class WeaponFlameThrower : MonoBehaviour, IRechargeableWeapon
{
    public float DPS = 50f;
    public float MaxDurationTime = 5f;
    public float RechargeTime = 2f;

    public GameObject ShootLocator;
    public GameObject AudioLaser;

    public GameObject StartEffect;
    public GameObject HitEffect;

    private LineRenderer m_hRenderer;
    private bool m_bFire;
    private bool m_bCompleteDischarge;

    public bool m_bRecharging;

    private float DurationTime;

    public Vector3 Direction { get; set; }

    void Awake()
    {
        m_hRenderer = this.GetComponent<LineRenderer>();
        DurationTime = MaxDurationTime;
    }

    public void Update()
    {
        RechargeMechanics();
        Fire();
    }

    public void Press()
    {
        m_bFire = true;
    }

    public void Release()
    {
        m_bFire = false;
        m_hRenderer.enabled = false;
    }

    public bool IsFiring
    {
        get { return m_bFire; }
    }

    public void RechargeMechanics()
    {
        if (m_bFire && DurationTime > 0f && !m_bCompleteDischarge)
        {
            m_bRecharging = false;
            DurationTime -= Time.deltaTime;
        }

        if (DurationTime <= 0.0f)
        {
            m_bCompleteDischarge = true;
            m_hRenderer.enabled = false;
            m_bRecharging = true;
        }

        if (m_bRecharging)
        {
            if (DurationTime < MaxDurationTime)
            {
                DurationTime += RechargeTime * Time.deltaTime;
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

        if (!m_hRenderer.enabled)
            m_hRenderer.enabled = true;
    }

    //TODO: da implementare, discutere modalita fuoco lanciafiamme
    public void Fire()
    {
        ShootLocator.transform.forward = Direction;

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