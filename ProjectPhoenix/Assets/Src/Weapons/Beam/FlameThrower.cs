using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public class FlameThrower : NetworkBehaviour, IBeam, IPoolable
{
    public Light FlameLight;
    public Actor Owner { get; set; }

    public Pool Pool { get; set; }

    public float MaxFireLength;
    public float DPS;
    public float DoT;

    private float LightIntensity;
    private AudioSource FlamesSound;


    private List<ParticleSystem> m_hParticleSystems;
    void Awake()
    {
        Owner = GetComponent<Actor>();
        m_hParticleSystems = this.GetComponentsInChildren<ParticleSystem>().ToList();
        FlameLight.enabled = false;
        LightIntensity = FlameLight.intensity;
        FlamesSound = this.GetComponent<AudioSource>();
    }

    public void Enable(Vector3 vPos, Vector3 vDir)
    {
        this.gameObject.transform.position = vPos;
        this.gameObject.transform.forward = vDir;
        m_hParticleSystems.ForEach(hP => hP.Play());
        FlameLight.enabled = true;
        FlameLight.intensity = LightIntensity;
        //TODO: Lerp Volume!!!
        if (!FlamesSound.isPlaying)
            FlamesSound.Play();

        RaycastHit m_hHitPoint = new RaycastHit();
        Ray vRay = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(vRay, out m_hHitPoint, MaxFireLength))
        {
            IDamageable hHit = m_hHitPoint.collider.GetComponent<IDamageable>();
            FlammableObject m_hFlamObj = m_hHitPoint.collider.GetComponent<FlammableObject>();

            if (hHit != null)
                hHit.Damage(this);

            if (m_hFlamObj != null)
                m_hFlamObj.SetOnFire();
        }
    }

    public void Disable()
    {
        m_hParticleSystems.ForEach(hP => hP.Stop());
        LeanTween.value(FlameLight.gameObject, LightIntensity, 0.0f, 0.5f).setOnUpdate(TweenLightIntensity);
        //TODO: Lerp Volume!!!
        FlamesSound.Stop();
    }

    private void TweenLightIntensity(float val)
    {
        FlameLight.intensity = val;
    }

    public float GetDamage(ArmorType armor)
    {
        throw new NotImplementedException();
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }
}