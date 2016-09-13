using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ParticleVfx : MonoBehaviour, IVisualEffect, IPoolable
{
    public float Duration = 2f;

    public Pool Pool { get; set; }

    private List<ParticleSystem> m_hParticle;
    private AudioSource m_hAudioSource;
    private bool FirstPlay;

    void Awake()
    {
        m_hParticle = this.GetComponentsInChildren<ParticleSystem>().ToList();
        m_hAudioSource = this.GetComponent<AudioSource>();
        FirstPlay = true;
    }


    public void PlayEffect(Vector3 vPosition, Vector3 vDirection, float scaleCoef, bool isSide)
    {
        this.gameObject.transform.position = vPosition;

        if (isSide)
            this.gameObject.transform.right   = vDirection;
        else
            this.gameObject.transform.forward = vDirection;

        if (FirstPlay)
        {
            m_hParticle.ForEach(hP => hP.startSize = hP.startSize * scaleCoef);
            FirstPlay = false;
        }

        m_hParticle.ForEach(hP => hP.Play());
        m_hAudioSource.Play();

        StartCoroutine(Wait(Duration));
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        GlobalFactory.Recycle(this.gameObject);
    }

    public void Enable()
    {
        m_hAudioSource.time = 0f;
        m_hParticle.ForEach(hP => hP.time = 0f);
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        m_hAudioSource.Stop();
        m_hParticle.ForEach(hP => hP.Stop());
        this.gameObject.SetActive(false);
    }
}
