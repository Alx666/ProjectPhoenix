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

    Plane playerPlane;
    Rigidbody heliRigidbody;
    Vector3 mousePos;
    bool isGrounded;
    float mass;
    float engineForce;
    float forwardForce;
    float strafeForce;
    float currentForwardSlope;
    float currentStrafeSlope;
    void Awake()
    {
        this.heliRigidbody = GetComponent<Rigidbody>();
        this.mass = heliRigidbody.mass;
        this.isGrounded = true;
        this.playerPlane = new Plane(Vector3.up, this.transform.position);

    }
    void FixedUpdate()
    {
        LiftProcess();
        if (!isGrounded)
        {
            Move();
        }
    }
    void Update()
    {
        if (!isGrounded)
        {
            Rotation();
            Inclination();
        }
    }
    private void LiftProcess()
    {
        var upForce = 1f - Mathf.Clamp01(this.transform.position.y / MaxHeight);
        upForce = Mathf.Lerp(0f, engineForce, upForce) * mass;
        this.heliRigidbody.AddForce(Vector3.up * upForce);
    }
    private void Move()
    {
        this.heliRigidbody.AddForce(heliRigidbody.transform.forward * VelocityForce * forwardForce);
        this.heliRigidbody.AddForce(heliRigidbody.transform.right * VelocityForce * strafeForce);
        this.heliRigidbody.velocity = Vector3.ClampMagnitude(this.heliRigidbody.velocity, MaxVelocityMagnitude);
    }
    private void Inclination()
    {
        this.currentForwardSlope = Mathf.Lerp(currentForwardSlope, forwardForce * 20f, Time.fixedDeltaTime);
        this.currentStrafeSlope = Mathf.Lerp(currentStrafeSlope, strafeForce * 20f, Time.fixedDeltaTime);
        this.OwnBody.transform.localRotation = Quaternion.Euler(currentForwardSlope, OwnBody.transform.localEulerAngles.y, -currentStrafeSlope);
    }
    private void Rotation()
    {
        playerPlane.SetNormalAndPosition(Vector3.up, this.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        float hitdist = 0.0f;
        if (playerPlane.Raycast(ray, out hitdist))
        {
            Vector3 targetPoint = ray.GetPoint(hitdist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - this.transform.position);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, VelocityRotation * Time.deltaTime);
        }
    }
    public void BeginBackward()
    {
        this.forwardForce = -1f;
    }

    public void BeginDown()
    {
        this.isGrounded = true;
        this.engineForce = 0f;
    }

    public void BeginFire()
    {
    }

    public void BeginForward()
    {
        this.forwardForce = 1f;

    }

    public void BeginPanLeft()
    {
        this.strafeForce = -1f;
    }

    public void BeginPanRight()
    {
        this.strafeForce = 1f;
    }

    public void BeginTurnLeft()
    {
    }

    public void BeginTurnRight()
    {
    }

    public void BeginUp()
    {
        this.isGrounded = false;
        this.engineForce = 50f;
    }

    public void EndBackward()
    {
        this.forwardForce = 0f;
    }

    public void EndDown()
    {
    }

    public void EndFire()
    {
    }

    public void EndForward()
    {
        this.forwardForce = 0f;
    }

    public void EndPanLeft()
    {
        this.strafeForce = 0f;
    }

    public void EndPanRight()
    {
        this.strafeForce = 0f;
    }

    public void EndTurnLeft()
    {
    }

    public void EndTurnRight()
    {
    }

    public void EndUp()
    {
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        this.mousePos = vMousePosition;
    }
}
