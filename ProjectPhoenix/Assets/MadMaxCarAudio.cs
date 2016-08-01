using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class MadMaxCarAudio : MonoBehaviour
{
    public AnimationCurve Curve;


    public AudioClip    HighAccelClip;
    public AudioClip    HighDecelClip;
    public AudioClip    LowAccelClip;
    public AudioClip    LowDecelClip;
    public float        PitchMultiplier        = 1f; 
    public float        LowPitchMin            = 1f; 
    public float        LowPitchMax            = 6f;
    public float        HighPitchMultiplier    = 0.25f;      
    public float        MaxRolloffDistance     = 500;         
   

    private AudioSource m_hHighAccel;
    private AudioSource m_hHighDecel;
    private AudioSource m_hLowAccel;
    private AudioSource m_hLowDecel;

    private ControllerWheels m_hWheelCtrl;
    private float       m_fMaxSpeed;
    private float       m_fChange;
    private float       m_fSpeed;
    private float       m_fAcceleration;


    [Range(0f, 100f)]
    public float AccelerationCoeff;

    public float DEBUG_GEARINDEX;

    void Awake ()
    {
        m_hWheelCtrl    = this.GetComponent<ControllerWheels>();
        m_fMaxSpeed     = m_hWheelCtrl.MaxSpeed;
        m_hHighAccel    = SetUpEngineAudioSource(HighAccelClip);
        m_hHighDecel    = SetUpEngineAudioSource(HighDecelClip);
        m_hLowAccel     = SetUpEngineAudioSource(LowAccelClip);
        m_hLowDecel     = SetUpEngineAudioSource(LowDecelClip);              
    }


    //Velocity = rb.velocity.magnitude;
    //    computeGear();
    //int maxSpeed = gears[CurrentGear - 1];//perché partono da 0
    //float speedPerc = Velocity / maxSpeed;
    //audios.pitch = 2 * speedPerc+1;



    void Update ()
    {
        m_fSpeed = Mathf.Lerp(m_fSpeed, m_hWheelCtrl.CurrentSpeed, 0.1f);

        m_fAcceleration = (m_fSpeed - m_hWheelCtrl.CurrentSpeed) * AccelerationCoeff;


        float pitch = Curve.Evaluate(m_fSpeed);


        m_hLowAccel.pitch = pitch * PitchMultiplier;
        m_hLowDecel.pitch = pitch * PitchMultiplier;
        m_hHighAccel.pitch = pitch * HighPitchMultiplier * PitchMultiplier;
        m_hHighDecel.pitch = pitch * HighPitchMultiplier * PitchMultiplier;

        // get values for fading the sounds based on the acceleration
        float accFade = Mathf.Abs(1);
        float decFade = 1 - accFade;

        // get the high fade value based on the cars revs
        float highFade = Mathf.InverseLerp(0.2f, 0.8f, pitch);
        float lowFade = 1 - highFade;

        // adjust the values to be more realistic
        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        accFade = 1 - ((1 - accFade) * (1 - accFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        // adjust the source volumes based on the fade values
        m_hLowAccel.volume = lowFade * accFade;
        m_hLowDecel.volume = lowFade * decFade;
        m_hHighAccel.volume = highFade * accFade;
        m_hHighDecel.volume = highFade * decFade;
    }

    
    private AudioSource SetUpEngineAudioSource(AudioClip clip)
    {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.loop = true;

        // start the clip from a random point
        source.time = UnityEngine.Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = MaxRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }

    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    private static float Revs(float fSpeed, float fPeriod, float fAmplitude, float fAddendum)
    {
        return fAmplitude * (fSpeed / fPeriod - Mathf.Floor(fSpeed / fPeriod)) + fAddendum;
    }
}
