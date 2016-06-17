using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public class FlameThrower : MonoBehaviour, IBeam
{
    public Light FlameLight;
    public Actor Owner { get; set; }

    public float MaxFireLength;
    public float DPS;
    public float DoT;

    private float LightIntensity;
    

    private List<ParticleSystem> m_hParticleSystems;
    void Awake()
    {
        Owner = GetComponent<Actor>();
        m_hParticleSystems = this.GetComponentsInChildren<ParticleSystem>().ToList();
        FlameLight.enabled = false;
        LightIntensity = FlameLight.intensity;
    }

    public void Enable(Vector3 vPos, Vector3 vDir)
    {
        this.gameObject.transform.position = vPos;
        this.gameObject.transform.forward = vDir;
        m_hParticleSystems.ForEach(hP => hP.Play());
        FlameLight.enabled = true;
        FlameLight.intensity = LightIntensity;

        RaycastHit m_hHitPoint = new RaycastHit();
        Ray vRay = new Ray(transform.position, transform.forward);

        if(Physics.Raycast(vRay, out m_hHitPoint, MaxFireLength))
        {
            IDamageable hHit = m_hHitPoint.collider.GetComponent<IDamageable>();
            FlammableObject m_hFlamObj = m_hHitPoint.collider.GetComponent<FlammableObject>();
            
            if(hHit != null)
                hHit.Damage(this);

            if (m_hFlamObj != null)
                m_hFlamObj.SetOnFire();
        }
    }

    public void Disable()
    {
        m_hParticleSystems.ForEach(hP => hP.Stop());
        LeanTween.value(FlameLight.gameObject, LightIntensity, 0.0f, 0.5f).setOnUpdate(TweenLightIntensity);
    }

    private void TweenLightIntensity(float val)
    {
        FlameLight.intensity = val;
    }

    public float GetDamage(ArmorType armor)
    {
        throw new NotImplementedException();
    }
}