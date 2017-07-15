using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVehicleHUD : MonoBehaviour
{
    public List<WheelColliderHUD> WheelColliderHUDs = new List<WheelColliderHUD>();

    void Reset()
    {
        WheelCollider[] colliders = GetComponentsInChildren<WheelCollider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            WheelColliderHUDs.Add(new WheelColliderHUD(colliders[i]));
        }
    }

    void Update()
    {
        for (int i = 0; i < WheelColliderHUDs.Count; i++)
        {
            WheelColliderHUDs[i].Update();
        }
    }
}

[Serializable]
public class WheelColliderHUD
{
    public WheelCollider Collider;
    public FrictionCurve ForwardFrictionCurve;
    public FrictionCurve SidewaysFrictionCurve;

    public WheelColliderHUD()
    {
        this.ForwardFrictionCurve = new FrictionCurve();
        this.SidewaysFrictionCurve = new FrictionCurve();
    }

    public WheelColliderHUD(WheelCollider collider)
    {
        this.Collider = collider;
    }

    internal void Update()
    {
        ForwardFrictionCurve.Update(Collider);
        SidewaysFrictionCurve.Update(Collider);
    }
}

[Serializable]
public class FrictionCurve
{
    public AnimationCurve Curve = new AnimationCurve();

    public FrictionCurve()
    {
        Curve.AddKey(0f, 0f);
        Curve.AddKey(1f, 1f);
        Curve.AddKey(2f, 2f);
    }

    internal void Update(WheelCollider collider)
    {
        if(Curve.length < 3)
        {
            for (int i = 0; i < Curve.length; i++)
            {
                Curve.RemoveKey(i);
            }
            Curve.AddKey(0f, 0f);
            Curve.AddKey(1f, 1f);
            Curve.AddKey(2f, 2f);
        }

        WheelFrictionCurve frictionCurve = collider.forwardFriction;

        float stiffness = frictionCurve.stiffness;

        float xExtremum = frictionCurve.extremumSlip;
        float yExtremum = frictionCurve.extremumValue * stiffness;

        float xAsymptote = frictionCurve.asymptoteSlip;
        float yAsymptote = frictionCurve.asymptoteValue * stiffness;

        Keyframe extremum  = new Keyframe(xExtremum, yExtremum);
        Keyframe asymptote = new Keyframe(xAsymptote, yAsymptote);

        Curve.MoveKey(1, extremum);
        Curve.MoveKey(2, asymptote);
    }
}
