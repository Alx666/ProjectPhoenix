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

    public float Damage = 10f;
    [Range(0f, 1f)]
    public float LightArmorDamageRate;
    [Range(0f, 1f)]
    public float MediumArmorDamageRate;
    [Range(0f, 1f)]
    public float HeavyArmorDamageRate;

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
        float damage = 0;

        switch (armor)
        {
            case ArmorType.Light:
                damage = Damage * LightArmorDamageRate;
                break;
            case ArmorType.Medium:
                damage = Damage * MediumArmorDamageRate;
                break;
            case ArmorType.Heavy:
                damage = Damage * HeavyArmorDamageRate;
                break;
            default:
                break;
        }
        return damage;
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }
}