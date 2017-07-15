using UnityEngine;
using System.Collections;

public class HumanoidWalkSM : StateMachineBehaviour
{
    //ControllerAIHumanoid controller;
    int shootingLayer;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            //this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.shootingLayer = animator.GetLayerIndex("Shoot");
            this.firstSetting = false;
        }
        animator.SetLayerWeight(shootingLayer, 1f);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.speed = controller.RVOController.velocity.magnitude;
    }
}
