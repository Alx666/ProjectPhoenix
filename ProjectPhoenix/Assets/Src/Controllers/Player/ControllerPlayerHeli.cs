using UnityEngine;
using System.Collections;
using System;

internal class ControllerPlayerHeli : MonoBehaviour, IControllerPlayer
{
    public GameObject OwnBody;
    public float MaxHeight;
    public float MaxVelocityMagnitude;
    public float VelocityForce;
    public float VelocityRotation;

    Rigidbody heliRigidbody;
    bool isFlying;
    void Awake()
    {
        this.heliRigidbody = GetComponent<Rigidbody>();
    }

    public void BeginBackward()
    {
        throw new NotImplementedException();
    }

    public void BeginDown()
    {
        throw new NotImplementedException();
    }

    public void BeginFire()
    {
        throw new NotImplementedException();
    }

    public void BeginForward()
    {
        throw new NotImplementedException();
    }

    public void BeginPanLeft()
    {
        throw new NotImplementedException();
    }

    public void BeginPanRight()
    {
        throw new NotImplementedException();
    }

    public void BeginTurnLeft()
    {
        throw new NotImplementedException();
    }

    public void BeginTurnRight()
    {
        throw new NotImplementedException();
    }

    public void BeginUp()
    {
        if(!isFlying)
        {
            this.heliRigidbody.AddForce(Vector3.up * 70f);
            this.isFlying = true;
        }
    }

    public void EndBackward()
    {
        throw new NotImplementedException();
    }

    public void EndDown()
    {
        throw new NotImplementedException();
    }

    public void EndFire()
    {
        throw new NotImplementedException();
    }

    public void EndForward()
    {
        throw new NotImplementedException();
    }

    public void EndPanLeft()
    {
        throw new NotImplementedException();
    }

    public void EndPanRight()
    {
        throw new NotImplementedException();
    }

    public void EndTurnLeft()
    {
        throw new NotImplementedException();
    }

    public void EndTurnRight()
    {
        throw new NotImplementedException();
    }

    public void EndUp()
    {
        throw new NotImplementedException();
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        throw new NotImplementedException();
    }
}
