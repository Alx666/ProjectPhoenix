using UnityEngine;
using System.Collections;
using System;

public class SimpleVfx : MonoBehaviour, IVisualEffect, IPoolable
{
    public float Duration = 0.02f;

    public Pool Pool { get; set; }

    private AudioSource m_hAudioSource;

    void Awake()
    {
        m_hAudioSource = this.GetComponent<AudioSource>();
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
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        m_hAudioSource.Stop();
        this.gameObject.SetActive(false);
    }
}
