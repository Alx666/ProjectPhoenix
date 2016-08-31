using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
public class AIHumanoidReachPointSM : StateMachineBehaviour
{
    public List<Vector3> Node;

    
    Vector3 target;
    int toReachPoint;
    bool firstSetting = true;

    ControllerAIHumanoid controller;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();

            
            this.toReachPoint = Animator.StringToHash("ToReachPoint");
            this.firstSetting = false;
        }

        if (!controller.isServer)
            return;
        controller.AI.enabled = false;
        controller.RVOController.Move(Vector3.zero);
        controller.RVOController.maxSpeed = 1f;
        this.target = Node[Random.Range(0,Node.Count)];
        this.controller.ManuallySetPath(target);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;

        if (controller.ManuallyMoveAgent())
        {
            controller.RVOController.Move(Vector3.zero);
            animator.SetBool(toReachPoint, false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;

    }

}
