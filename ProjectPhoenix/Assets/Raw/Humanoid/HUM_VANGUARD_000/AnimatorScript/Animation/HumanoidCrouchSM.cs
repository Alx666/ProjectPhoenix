using UnityEngine;
using System.Collections;

public class HumanoidCrouchSM : StateMachineBehaviour
{
    int shootingLayer;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.shootingLayer = animator.GetLayerIndex("Shoot");
            this.firstSetting = false;
        }
        animator.SetLayerWeight(shootingLayer, 0.4f);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }


}
