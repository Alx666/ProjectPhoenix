using UnityEngine;
using System.Collections;

public class ControllerPlayerHeli2 : MonoBehaviour, IControllerPlayer
{
    public GameObject OwnBody;
    public float MaxHeight;
    public float MaxVelocityMagnitude;
    public float VelocityForce;
    public float VelocityRotation;         // < 5

    [HideInInspector]
    public bool isGrounded { get; private set; }

    Plane playerPlane;
    Rigidbody heliRigidbody;
    Vector3 mousePos;

    float UpdatedMaxHeight;
    float mass;
    float engineForce;
    float forwardForce;
    float strafeForce;
    float currentForwardSlope;
    float currentStrafeSlope;

    void Awake()
    {
        this.UpdatedMaxHeight = this.MaxHeight;
        this.heliRigidbody = GetComponent<Rigidbody>();
        this.mass = heliRigidbody.mass;
        this.isGrounded = true;
        this.playerPlane = new Plane(Vector3.up, this.transform.position);
    }
    void FixedUpdate()
    {
        LiftProcess();
        if (!isGrounded && this.transform.position.y >= MaxHeight * 4 / 5)
        {
            Move();
        }
    }
    void Update()
    {
        if (!isGrounded && this.transform.position.y >= MaxHeight * 4 / 5)
        {
            Rotation();
            Inclination();
        }
    }
    void LateUpdate()
    {
        CheckHeight();
    }

    private void Move()
    {
        this.heliRigidbody.AddForce(heliRigidbody.transform.forward * VelocityForce * forwardForce);
        this.heliRigidbody.AddForce(heliRigidbody.transform.right * VelocityForce * strafeForce);
        this.heliRigidbody.velocity = Vector3.ClampMagnitude(this.heliRigidbody.velocity, MaxVelocityMagnitude);
    }
    public void LiftProcess()
    {
        var upForce = 1f - Mathf.Clamp01(this.transform.position.y / MaxHeight);
        upForce = Mathf.Lerp(0f, engineForce, upForce) * mass;
        this.heliRigidbody.AddForce(Vector3.up * upForce);
    }
    private void CheckHeight()
    {
        RaycastHit vHit;

        if (Physics.Raycast(this.transform.position, Vector3.down, out vHit))  // Aggiungere: int layerMask 
        {
            var changeForce = 1f - Mathf.Clamp01((this.transform.position.y - vHit.distance) / UpdatedMaxHeight);
            changeForce = Mathf.Lerp(0f, engineForce, changeForce) * mass;

            if (vHit.distance < MaxHeight)
            {
                this.heliRigidbody.AddForce(Vector3.up * changeForce);
                UpdatedMaxHeight += (UpdatedMaxHeight - vHit.distance);

            }
            if (vHit.distance >= MaxHeight)
            {
                this.heliRigidbody.AddForce(Vector3.down * changeForce);
                UpdatedMaxHeight -= (UpdatedMaxHeight + vHit.distance);
            }
        }
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
        Debug.Log("Pew");
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
        this.UpdatedMaxHeight = this.MaxHeight;
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
