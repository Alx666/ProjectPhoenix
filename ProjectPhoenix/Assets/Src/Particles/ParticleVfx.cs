using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ParticleVfx : MonoBehaviour, IVisualEffect, IPoolable
{
    public float Duration;

    public Pool Pool { get; set; }

    private List<ParticleSystem> m_hParticle;

    void Awake()
    {
        m_hParticle = this.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    public void PlayEffect(Vector3 vPosition, Vector3 vDirection, float scaleCoef, bool isSide)
    {
        this.gameObject.transform.position = vPosition;

        if (isSide)
            this.gameObject.transform.right   = vDirection;
        else
            this.gameObject.transform.forward = vDirection;

        this.gameObject.transform.localScale = this.gameObject.transform.localScale * scaleCoef;

        StartCoroutine(Wait(Duration));
        m_hParticle.ForEach(hP => hP.Play());
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        GlobalFactory.Recycle(this.gameObject);
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
        m_hParticle.ForEach(hP => hP.time = 0f);
    }

    public void Disable()
    {
        m_hParticle.ForEach(hP => hP.Stop());
        this.gameObject.SetActive(false);
    }
}
