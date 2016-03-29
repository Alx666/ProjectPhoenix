using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public class FlameThrower : MonoBehaviour, IBeam
{
    public float MaxFireLength;
    public float DPS;
    public float DoT;

    private List<ParticleSystem> m_hParticleSystems;
    void Awake()
    {
        m_hParticleSystems = this.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    public void Enable(Vector3 vPos, Vector3 vDir)
    {
        this.gameObject.transform.position = vPos;
        this.gameObject.transform.forward = vDir;
        m_hParticleSystems.ForEach(hP => hP.Play());

        RaycastHit m_hHitPoint = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);

        if(Physics.Raycast(ray, out m_hHitPoint, MaxFireLength))
        {
            Debug.Log("Obj Hit");
            FlammableObject m_hFlamObj = m_hHitPoint.collider.GetComponent<FlammableObject>();

            if (m_hFlamObj != null)
                m_hFlamObj.SetOnFire();
        }
    }

    public void Disable()
    {
        m_hParticleSystems.ForEach(hP => hP.Stop());
    }
}