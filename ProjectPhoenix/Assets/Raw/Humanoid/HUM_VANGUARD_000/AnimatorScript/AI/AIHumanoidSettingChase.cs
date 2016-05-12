using UnityEngine;
using System.Collections;

public class AIHumanoidSettingChase : StateMachineBehaviour
{
    ControllerAIHumanoid controller;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.firstSetting = false;
        }
        controller.agent.Resume();
        controller.agent.speed = 1f;
        controller.agent.stoppingDistance = 3f;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
