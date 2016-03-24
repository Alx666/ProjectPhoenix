using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaser : MonoBehaviour, IBeam
{
    public LayerMask layerMask;

    public Texture[] BeamFrames;    // Animation frame sequence
    public float FrameStep;         // Animation time
    public bool RandomizeFrames;    // Randomization of animation frames

    public int Points;              // How many points should be used to construct the beam

    public float MaxBeamLength;     // Maximum beam length
    public float beamScale;         // Default beam scale to be kept over distance

    public bool AnimateUV;          // UV Animation
    public float UVTime;            // UV Animation speed

    public bool Oscillate;          // Beam oscillation flag
    public float Amplitude;         // Beam amplitude
    public float OscillateTime;     // Beam oscillation rate

    public Transform rayImpact;     // Impact transform
    public Transform rayMuzzle;     // Muzzle flash transform

    private LineRenderer m_hLineRenderer;
    RaycastHit hitPoint;            // Raycast structure

    int frameNo;                    // Frame counter
    int FrameTimerID;               // Frame timer reference
    int OscillateTimerID;           // Beam oscillation timer reference

    float beamLength;               // Current beam length
    float initialBeamOffset;        // Initial UV offset 


    void Awake()
    {
        m_hLineRenderer = this.GetComponent<LineRenderer>();

        // Assign first frame texture
        if (!AnimateUV && BeamFrames.Length > 0)
            m_hLineRenderer.material.mainTexture = BeamFrames[0];

        // Randomize uv offset
        initialBeamOffset = UnityEngine.Random.Range(0f, 5f);
    }

    public void Enable()
    {
        m_hLineRenderer.enabled = true;
    }

    public void Disable()
    {
        m_hLineRenderer.enabled = false;
    }
}
