using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

internal class ControllerPlayerHeli : NetworkBehaviour, IControllerPlayer
{
    public GameObject OwnBody;
    public GameObject MainWeapon;
    public float MaxHeight;
    public float MaxVelocityMagnitude;
    public float HorizontalVelocityForce;
    public float VerticalVelocityForce;
    public float VelocityRotation;
    public List<GameObject> rotors;
    public float MaxRotarSpeed = 15f;
    public Rigidbody HeliRigidBody { get; set; }


    Plane playerPlane;
    Vector3 mousePos;
    GameObject weapon;
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
        if (!this.isLocalPlayer)
        {
            GameObject.Destroy(this.GetComponent<InputProviderPCStd>());
            GameObject.Destroy(this);
        }

        if (MainWeapon != null)
        {
            weapon = MainWeapon;
        }
    }
    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        if (!isGrounded)
        {
            Rotation();
            Inclination();
        }

        if (rotors != null)
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
        this.HeliRigidBody.AddForce(HeliRigidBody.transform.forward * HorizontalVelocityForce * forwardForce);
        this.HeliRigidBody.AddForce(HeliRigidBody.transform.right * HorizontalVelocityForce * strafeForce);
        this.HeliRigidBody.velocity = Vector3.ClampMagnitude(this.HeliRigidBody.velocity, MaxVelocityMagnitude);
    }
    private void Inclination()
    {
        this.currentForwardSlope = forwardForce * 20f;
        this.currentStrafeSlope = strafeForce * 20f;
        this.OwnBody.transform.localRotation = Quaternion.Slerp(this.OwnBody.transform.localRotation, Quaternion.Euler(currentForwardSlope, 
                                               OwnBody.transform.localEulerAngles.y, -currentStrafeSlope), Time.deltaTime);
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
            weapon.transform.rotation = targetRotation;            
        }
    }
    
    void RotateRotor()
    {
        if (!isGrounded)
            rotarSpeed += Time.deltaTime * 10f;
        else
            rotarSpeed -= Time.deltaTime * 3f;

        rotarSpeed = Mathf.Clamp(rotarSpeed, 0f, MaxRotarSpeed);
        rotors.ToList().ForEach(hR => hR.transform.Rotate(new Vector3(0f, rotarSpeed, 0f)));
    }

    public void BeginBackward()
    {
        this.forwardForce = -1f;
    }

    public void StopVehicle()
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
        this.engineForce = VerticalVelocityForce;
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
