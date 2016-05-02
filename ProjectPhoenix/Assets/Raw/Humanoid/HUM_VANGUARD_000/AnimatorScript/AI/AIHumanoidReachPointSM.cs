using UnityEngine;
using System.Collections.Generic;

public class AIHumanoidReachPointSM : StateMachineBehaviour
{
    public List<Vector3> Node;

    Vector3 target;

    //Vector3 currentDestination;
    NavMeshAgent agent;
    Queue<Vector3> wayPoint;
    int toLookAround;
    bool firstSetting = true;

    ControllerAIHumanoid controller;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();

            this.agent = animator.GetComponent<NavMeshAgent>();
            this.wayPoint = new Queue<Vector3>();
            for (int i = 0; i < Node.Count; i++)
            {
                wayPoint.Enqueue(Node[i]);
            }
            this.toLookAround = Animator.StringToHash("ToLookAround");
            this.target = wayPoint.Dequeue();
            this.agent.SetDestination(target);
            this.firstSetting = false;
        }
        else
        {
            agent.Resume();
            SetDestination();
        }
        this.agent.speed = 0.4f;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            controller.Move(Vector3.zero);
            animator.SetTrigger(toLookAround);
        }
        else
        {
            controller.Move(agent.desiredVelocity);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    //SOSTITUIRE
    void SetDestination()
    {
        this.wayPoint.Enqueue(target);
        this.target = wayPoint.Dequeue();
        this.agent.SetDestination(target);
    }
}
