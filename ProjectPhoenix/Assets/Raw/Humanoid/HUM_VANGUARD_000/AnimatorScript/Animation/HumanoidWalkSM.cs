using UnityEngine;
using System.Collections;

public class HumanoidWalkSM : StateMachineBehaviour
{
    //public Vector3 Target {  get; set; }

    //Vector3 m_GroundNormal;
    //float turnValue;
    //float forwardValue;
    //Animator animator;
    //NavMeshAgent agent;
    int shootingLayer;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            //this.agent = animator.GetComponent<NavMeshAgent>();
            //this.animator = animator;
            this.shootingLayer = animator.GetLayerIndex("Shoot");
            this.firstSetting = false;
        }
        animator.SetLayerWeight(shootingLayer, 1f);
        //this.Target = agent.transform.position;
        //agent.SetDestination(Target);

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (!animator.IsInTransition(0))
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        RaycastHit hit;

        //        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        //        {
        //            target = hit.point;
        //        }

        //    }
        //    if (target != null)
        //        agent.SetDestination(target);

        //    if (agent.remainingDistance > agent.stoppingDistance)
        //        Move(agent.desiredVelocity);
        //    else
        //    {
        //        Move(Vector3.zero);
        //    }
        //}
        //if (Input.GetMouseButtonDown(1))
        //{
        //    animator.SetTrigger("Rolling");
            
        //}
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    
    //internal void Move(Vector3 move)
    //{
    //    if (move.magnitude > 1f)
    //        move.Normalize();
    //    move = animator.transform.InverseTransformDirection(move);
    //    RaycastHit hitInfo;
    //    if (Physics.Raycast(animator.transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
    //    {
    //        m_GroundNormal = hitInfo.normal;

    //    }

    //    move = Vector3.ProjectOnPlane(move, m_GroundNormal);
    //    turnValue = Mathf.Atan2(move.x, move.z);
    //    forwardValue = move.z;
    //    SetAnimator(move);
    //    ApplyExtraTurnRotation();
    //}
    //void ApplyExtraTurnRotation()
    //{
    //    float turnSpeed = Mathf.Lerp(180f, 360f, forwardValue);
    //    animator.transform.Rotate(0f, turnValue * turnSpeed * Time.deltaTime, 0f);
    //}
    //void SetAnimator(Vector3 move)
    //{
    //    animator.SetFloat("Forward", forwardValue, 0.1f, Time.deltaTime);
    //    animator.SetFloat("Turn", turnValue, 0.1f, Time.deltaTime);
    //    if (move.magnitude > 0)
    //    {
    //        animator.speed = 1;
    //    }
    //}
}
