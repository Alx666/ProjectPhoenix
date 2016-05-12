using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using System;

public class ControllerAIHumanoid : MonoBehaviour
{
    public Transform WeaponLocator;
    public Transform RightHand;
    public Transform LeftHandObj;
    public Transform RightHandObj;
    public Transform LookObj;

    internal float DefaultStoppingDistance { get; private set; }
    internal List<GameObject> SeeEnemies { get; private set; }
    internal Vector3 CurrentEnemyPos { get; private set; }
    internal NavMeshAgent agent { get; private set; }

    Vector3 m_GroundNormal;
    float fieldOfView = 110f;
    float turnValue;
    float forwardValue;
    int forwardHash;
    int turnHash;
    int toChase;
    int shootingHash;

    AimIK aimIk;
    Animator animator;
    SphereCollider coll;

    internal bool IsInFieldOfView(GameObject obj)
    {
        Vector3 direction = obj.transform.position - this.transform.position;
        float angle = Vector3.Angle(this.transform.forward, direction);
        if (angle <= fieldOfView * 0.5f)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, direction.normalized, out hit, coll.radius))
            {
                if (hit.collider.gameObject.tag == "Enemy")
                {
                    return true;
                }
            }
        }
        return false;
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {

            if (IsInFieldOfView(other.gameObject))
            {
                if (!SeeEnemies.Contains(other.gameObject))
                    this.SeeEnemies.Add(other.gameObject);

                if (!animator.GetBool(toChase))
                {
                    animator.SetBool(toChase, true);
                }
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {


        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {

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
            if (RightHandObj != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandObj.rotation);
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
        this.shootingHash = Animator.StringToHash("Shooting");
        this.aimIk = GetComponent<AimIK>();
        this.aimIk.solver.target = WeaponLocator;
        this.SeeEnemies = new List<GameObject>();
        this.agent = GetComponent<NavMeshAgent>();
        this.DefaultStoppingDistance = agent.stoppingDistance;
        this.animator = GetComponent<Animator>();
        this.coll = GetComponent<SphereCollider>();
        this.forwardHash = Animator.StringToHash("Forward");
        this.turnHash = Animator.StringToHash("Turn");
        this.toChase = Animator.StringToHash("ToChase");

    }
    void SetCurrentEnemy()
    {
        try
        {
            for (int i = 0; i < SeeEnemies.Count; i++)
            {
                if (!IsInFieldOfView(SeeEnemies[i]))
                    SeeEnemies.RemoveAt(i);
            }
            SeeEnemies.Sort(delegate (GameObject x, GameObject y)
            {
                return Vector3.Distance(x.transform.position, this.transform.position).CompareTo(Vector3.Distance(y.transform.position, this.transform.position));
            });
            aimIk.solver.target = SeeEnemies[0].transform;
            CurrentEnemyPos = SeeEnemies[0].transform.position;

        }
        catch (ArgumentOutOfRangeException)
        {
            animator.SetBool(shootingHash, false);
            animator.SetBool(toChase, false);
            this.aimIk.solver.target = WeaponLocator;
        }


    }
    void Update()
    {
        if (animator.GetBool(toChase))
        {
            SetCurrentEnemy();
        }
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
