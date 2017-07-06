using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using Pathfinding;
public class AIHumanoidHuntSM : StateMachineBehaviour
{

    int toKeepDistance;
    int readyToAimHash;
    int toStationaryHash;
    ControllerAIHumanoid controller;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


        if (firstSetting)
        {
            this.toKeepDistance = Animator.StringToHash("ToKeepDistance");

            this.readyToAimHash = Animator.StringToHash("ReadyToAim");
            this.toStationaryHash = Animator.StringToHash("ToStationary");
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.firstSetting = false;
        }
        if (!controller.isServer)
            return;

        animator.SetBool(readyToAimHash, true);
        controller.RVOController.enableRotation = false;
        controller.RVOController.maxSpeed = controller.MaxSpeedRVO;
        controller.AI.enabled = true;
        controller.AI.target = controller.CurrentEnemy.transform;
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;

        if (controller.IsInFieldOfView(controller.CurrentEnemy) && Vector3.Distance(animator.transform.position, controller.CurrentEnemy.transform.position) < 9f)
        {
            animator.SetBool(toStationaryHash, true);
        }

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
