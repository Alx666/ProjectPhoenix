using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public List<AudioClip> Tracks;

    [Range(0f, 1f)]
    public float Volume = 1.0f;

    public float FadeOutTime = 1.0f;

    private LinkedList<AudioClip> m_hTracks;
    private LinkedListNode<AudioClip> m_hCurrent;
    private AudioSource m_hAudioSource;

	
	void Awake()
    {
        Tracks.Shuffle();

        m_hAudioSource          = this.GetComponent<AudioSource>();
        m_hAudioSource.loop     = false;
        m_hAudioSource.volume   = Volume;

        m_hTracks               = new LinkedList<AudioClip>(Tracks);
        m_hCurrent              = m_hTracks.First;
        m_hAudioSource.clip     = m_hCurrent.Value;
    }
	
	
	void Update ()
    {        
        if (!m_hAudioSource.isPlaying)
        {
            m_hCurrent = m_hCurrent.NextOrFirst();

            m_hAudioSource.clip = m_hCurrent.Value;
            m_hAudioSource.Play();
        }
    }

    public void FadeOut()
    {
        LeanTween.value(this.gameObject, OnValueUpdate, Volume, 0f, FadeOutTime);
    }

    private void OnValueUpdate(float fVolume)
    {
        m_hAudioSource.volume = fVolume;
    }
}
