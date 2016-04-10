using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SimpleVfx : MonoBehaviour, IVisualEffect, IPoolable
{
    public float MuzzleDuration = 0.02f;
    public float SoundDuration = 1f;

    public Pool Pool { get; set; }

    private AudioSource m_hAudioSource;
    private Light m_hPointLight;
    private List<MeshRenderer> m_hChildrenQuad;

    void Awake()
    {
        m_hAudioSource = this.GetComponent<AudioSource>();
        m_hPointLight = this.GetComponentInChildren<Light>();
        m_hChildrenQuad = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
    }

    public void PlayEffect(Vector3 vPosition, Vector3 vDirection, float scaleCoef, bool isSide)
    {
        this.gameObject.transform.position = vPosition;

        if (isSide)
            this.gameObject.transform.right = -vDirection;
        else
            this.gameObject.transform.forward = vDirection;

        this.gameObject.transform.localScale = this.gameObject.transform.localScale * scaleCoef;

        m_hAudioSource.Play();

        StartCoroutine(WaitForMuzzle(MuzzleDuration));
        StartCoroutine(WaitForSound(SoundDuration));
    }

    IEnumerator WaitForMuzzle(float duration)
    {
        yield return new WaitForSeconds(duration);
        DisableMuzzle();
    }
    IEnumerator WaitForSound(float duration)
    {
        yield return new WaitForSeconds(duration);
        GlobalFactory.Recycle(this.gameObject);
    }

    public void Enable()
    {
        m_hPointLight.enabled = true;
        m_hChildrenQuad.ForEach(hQ => hQ.enabled = true);
        m_hAudioSource.time = 0f;
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        m_hAudioSource.Stop();
        this.gameObject.SetActive(false);
    }

    private void DisableMuzzle()
    {
        m_hPointLight.enabled = false;
        m_hChildrenQuad.ForEach(hQ => hQ.enabled = false);
    }
}
