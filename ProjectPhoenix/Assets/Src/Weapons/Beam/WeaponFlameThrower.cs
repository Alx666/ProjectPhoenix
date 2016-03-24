using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class WeaponFlameThrower : MonoBehaviour, IBeam
{
    private ParticleSystem m_hParticleSystem;
    void Awake()
    {
        m_hParticleSystem = this.GetComponent<ParticleSystem>();
    }

    public void Enable()
    {
        m_hParticleSystem.Play();
    }

    public void Disable()
    {
        m_hParticleSystem.Stop();
    }
}