using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public class FlameThrower : MonoBehaviour, IBeam
{
    private List<ParticleSystem> m_hParticleSystems;
    void Awake()
    {
        Debug.Log("Awake ft");
        m_hParticleSystems = this.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    public void Enable(Vector3 vPos, Vector3 vDir)
    {
        this.gameObject.transform.position = vPos;
        this.gameObject.transform.forward = vDir;
        m_hParticleSystems.ForEach(hP => hP.Play());
    }

    public void Disable()
    {
        m_hParticleSystems.ForEach(hP => hP.Stop());
    }
}