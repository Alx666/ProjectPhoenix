using UnityEngine;
using System.Collections;
using System;

public class ControllerAIHumanoid : MonoBehaviour
{
    Vector3 m_GroundNormal;
    float turnValue;
    float forwardValue;
    Vector3 target;
    //int run;
    Animator animator;
    NavMeshAgent agent;
    void Awake()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.animator = GetComponent<Animator>();
        //this.run = Animator.StringToHash("Run");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                target = hit.point;
            }

        }
        if (target != null)
            agent.SetDestination(target);

        if (agent.remainingDistance > agent.stoppingDistance)
            Move(agent.desiredVelocity);
        else
        {
            Move(Vector3.zero);
        }
    }
    void Move(Vector3 move)
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
        animator.SetFloat("Forward", forwardValue, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnValue, 0.1f, Time.deltaTime);
        if (move.magnitude > 0)
        {
            animator.speed = 1;
        }
    }
}
