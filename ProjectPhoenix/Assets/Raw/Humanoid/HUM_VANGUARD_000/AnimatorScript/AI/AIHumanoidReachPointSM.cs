using UnityEngine;
using System.Collections.Generic;

public class AIHumanoidReachPointSM : StateMachineBehaviour
{
    public List<Vector3> Node;

    Vector3 target;
    Queue<Vector3> wayPoint;
    int toLookAround;
    bool firstSetting = true;

    ControllerAIHumanoid controller;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();

            this.wayPoint = new Queue<Vector3>();
            for (int i = 0; i < Node.Count; i++)
            {
                wayPoint.Enqueue(Node[i]);
            }
            this.toLookAround = Animator.StringToHash("ToLookAround");
            this.target = wayPoint.Dequeue();
            this.controller.agent.SetDestination(target);
            this.firstSetting = false;
        }
        else
        {
            this.controller.agent.Resume();
            SetDestination();
        }
        this.controller.agent.speed = 0.4f;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            controller.Move(Vector3.zero);
            animator.SetTrigger(toLookAround);
        }
        else
        {
            controller.Move(controller.agent.desiredVelocity);
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
        this.controller.agent.SetDestination(target);
    }
}
