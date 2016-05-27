﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

internal class ControllerPlayerHeli : NetworkBehaviour, IControllerPlayer
{
    public GameObject OwnBody;
    public float MaxHeight;
    public float MaxVelocityMagnitude;
    public float VelocityForce;
    public float VelocityRotation;
    public GameObject rotor;
    public Rigidbody HeliRigidBody { get; set; }


    Plane playerPlane;
    Vector3 mousePos;
    bool isGrounded;
    float mass;
    float engineForce;
    float forwardForce;
    float strafeForce;
    float currentForwardSlope;
    float currentStrafeSlope;
    float targetHeight;
    IWeapon m_hCurrentWeapon;
    float rotarSpeed;


    void Awake()
    {
        this.HeliRigidBody = GetComponent<Rigidbody>();
        this.mass = HeliRigidBody.mass;
        this.isGrounded = true;
        this.playerPlane = new Plane(Vector3.up, this.transform.position);
        m_hCurrentWeapon = GetComponentInChildren<IWeapon>();
    }

    void Start()
    {
        //if (!this.isLocalPlayer)
        //{
        //    GameObject.Destroy(this.GetComponent<InputProviderPCStd>());
        //    GameObject.Destroy(this);
        //}
    }
    void Update()
    {
        //if (!this.isLocalPlayer)
        //    return;

        if (!isGrounded)
        {
            Rotation();
            Inclination();
        }

        if (rotor != null)
            RotateRotor();
    }

    void FixedUpdate()
    {
        LiftProcess();
        if (!isGrounded)
        {
            Move();
        }
    }

    private void LiftProcess()
    {
        Ray currentHeight = new Ray(this.transform.position, Vector3.down);
        RaycastHit vHit;
        Physics.Raycast(currentHeight, out vHit);
        targetHeight = this.transform.position.y + (MaxHeight - vHit.distance);

        var upForce = 1f - Mathf.Clamp01(this.transform.position.y / targetHeight);
        upForce = Mathf.Lerp(0f, engineForce, upForce) * mass;
        this.HeliRigidBody.AddForce(Vector3.up * upForce);
    }
    private void Move()
    {
        this.HeliRigidBody.AddForce(HeliRigidBody.transform.forward * VelocityForce * forwardForce);
        this.HeliRigidBody.AddForce(HeliRigidBody.transform.right * VelocityForce * strafeForce);
        this.HeliRigidBody.velocity = Vector3.ClampMagnitude(this.HeliRigidBody.velocity, MaxVelocityMagnitude);
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

    //provvisorio
    void RotateRotor()
    {
        if (!isGrounded)
        {
            rotarSpeed += Time.deltaTime * 10f;
            if (rotarSpeed > 15f)
                rotarSpeed = 15f;
        }
        else
        {
            rotarSpeed -= Time.deltaTime * 3f;
            if (rotarSpeed < 0f)
                rotarSpeed = 0f;
        }

        rotor.transform.Rotate(new Vector3(0f, rotarSpeed, 0f));
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
        if (m_hCurrentWeapon != null)
            m_hCurrentWeapon.Press();
    }

    public void EndFire()
    {
        if (m_hCurrentWeapon != null)
            m_hCurrentWeapon.Release();
    }

    public void BeginForward()
    {
        this.forwardForce = 1f;

    }

    public void BeginPanLeft()
    {
    }

    public void BeginPanRight()
    {
    }

    public void BeginTurnLeft()
    {
        this.strafeForce = -1f;
    }

    public void BeginTurnRight()
    {
        this.strafeForce = 1f;
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

    public void EndForward()
    {
        this.forwardForce = 0f;
    }

    public void EndPanLeft()
    {
    }

    public void EndPanRight()
    {
    }

    public void EndTurnLeft()
    {
        this.strafeForce = 0f;
    }

    public void EndTurnRight()
    {
        this.strafeForce = 0f;
    }

    public void EndUp()
    {
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        this.mousePos = vMousePosition;
    }
}
