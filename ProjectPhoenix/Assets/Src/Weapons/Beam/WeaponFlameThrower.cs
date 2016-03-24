using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class WeaponFlameThrower : MonoBehaviour, IBeam
{
    private List<ParticleSystem> m_hParticleSystems;
    void Awake()
    {
       m_hParticleSystems = this.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    public void Enable()
    {
        m_hParticleSystems.ForEach(hP => hP.Play());
    }

    public void Disable()
    {
        m_hParticleSystems.ForEach(hP => hP.Stop());
    }
}