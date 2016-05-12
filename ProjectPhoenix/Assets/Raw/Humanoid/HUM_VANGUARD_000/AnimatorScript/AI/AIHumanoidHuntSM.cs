﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class AIHumanoidHuntSM : StateMachineBehaviour
{
    int shootingHash;
    int toChase;
    ControllerAIHumanoid controller;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.shootingHash = Animator.StringToHash("Shooting");
            this.toChase = Animator.StringToHash("ToChase");
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.firstSetting = false;
        }
        animator.SetBool(shootingHash, true);
        controller.agent.SetDestination(controller.CurrentEnemyPos);

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.agent.SetDestination(controller.CurrentEnemyPos);

        if (controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            controller.Move(Vector3.zero);

        }
        else
        {
            controller.Move(controller.agent.desiredVelocity);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
