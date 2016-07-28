using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MadMaxCarAudio1 : MonoBehaviour
{
    public List<Gear> Gears;

    public AudioClip Clip;
    public float MaxRolloffDistance = 500;
    public float DEBUG_GEARINDEX;

    private AudioSource m_hAudioSource;

    private ControllerWheels m_hWheelCtrl;

    private void Awake()
    {
        m_hWheelCtrl = this.GetComponent<ControllerWheels>();
        m_hAudioSource = SetUpEngineAudioSource(Clip);

        Gears = Gears.OrderBy(hG => hG.MinChangeSpeed).ToList();
    }

    private void Update()
    {
        Gear hGear = Gears.Where(hG => m_hWheelCtrl.CurrentSpeed > hG.MinChangeSpeed && m_hWheelCtrl.CurrentSpeed < hG.MaxChangeSpeed).FirstOrDefault();
        if (hGear == null)
            hGear = Gears.Last();

        DEBUG_GEARINDEX = hGear.MaxChangeSpeed;

        if(!m_hWheelCtrl.IsFlying && !m_hWheelCtrl.m_hRight && !m_hWheelCtrl.m_hLeft)
            m_hAudioSource.pitch = Revs(m_hWheelCtrl.CurrentSpeed, hGear.MaxChangeSpeed, hGear.Amplitude, hGear.Addendum);
    }

    private AudioSource SetUpEngineAudioSource(AudioClip clip)
    {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 1;
        source.loop = true;

        // start the clip from a random point
        source.time = UnityEngine.Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = MaxRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }

    private static float Revs(float fSpeed, float fPeriod, float fAmplitude, float fAddendum)
    {
        return fAmplitude * (fSpeed / fPeriod - Mathf.Floor(fSpeed / fPeriod)) + fAddendum;
    }

    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    [Serializable]
    public class Gear
    {
        public float MinChangeSpeed;
        public float MaxChangeSpeed;
        public float Addendum;
        public float Amplitude;
    }
}