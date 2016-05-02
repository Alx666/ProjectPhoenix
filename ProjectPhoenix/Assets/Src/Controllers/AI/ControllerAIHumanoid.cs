using UnityEngine;
using System.Collections;
using System;

public class ControllerAIHumanoid : MonoBehaviour
{
    public Transform RightHand;
    public Transform WeaponLocator;
    public Transform LeftHandObj = null;
    public Transform LookObj = null;

    public Transform CurrentEnemy { get; private set; }
    float fieldOfView = 110f;
    Vector3 m_GroundNormal;
    float turnValue;
    float forwardValue;
    int forwardHash;
    int turnHash;
    int toChase;
    Animator animator;
    NavMeshAgent agent;
    SphereCollider coll;
    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            Vector3 direction = other.transform.position - this.transform.position;
            float angle = Vector3.Angle(this.transform.forward, direction);
            if(angle <= fieldOfView * 0.5f)
            {
                this.CurrentEnemy = other.transform;
                animator.SetTrigger(toChase);
            }
        }
    }
    void OnAnimatorIK()
    {
        
        if (animator)
        {
            if (LookObj != null)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(LookObj.position);
            }

            if (LeftHandObj != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetLookAtWeight(0);
            }
            
        }
    }
    void Awake()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.animator = GetComponent<Animator>();
        this.coll = GetComponent<SphereCollider>();
        this.forwardHash = Animator.StringToHash("Forward");
        this.turnHash = Animator.StringToHash("Turn");
        this.toChase = Animator.StringToHash("ToChase");

    }
    void Update()
    {

    }
    internal void Move(Vector3 move)
    {
        if (move.magnitude > 1f)
            move.Normalize();
        move = transform.InverseTransformDirection(move);
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
        {
            m_GroundNormal = hitInfo.normal;

        }

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        turnValue = Mathf.Atan2(move.x, move.z);
        forwardValue = move.z;
        SetAnimator(move);
        ApplyExtraTurnRotation();
    }
    void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(180f, 360f, forwardValue);
        transform.Rotate(0f, turnValue * turnSpeed * Time.deltaTime, 0f);
    }
    
    void SetAnimator(Vector3 move)
    {
        animator.SetFloat(forwardHash, forwardValue, 0.1f, Time.deltaTime);
        animator.SetFloat(turnHash, turnValue, 0.1f, Time.deltaTime);
        if (move.magnitude > 0)
        {
            animator.speed = 1;
        }
    }
    internal void ManualSetAnimator(float forward, float turn, bool damp)
    {
        if (damp)
        {
            animator.SetFloat(forwardHash, forward, 0.1f, Time.deltaTime);
            animator.SetFloat(turnHash, turn, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat(forwardHash, forward);
            animator.SetFloat(turnHash, turn);
        }
    }
   
}
