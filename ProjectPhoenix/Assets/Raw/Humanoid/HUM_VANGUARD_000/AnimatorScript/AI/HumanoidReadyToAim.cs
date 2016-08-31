using UnityEngine;
using System.Collections;

public class HumanoidReadyToAim : StateMachineBehaviour
{
    IKControllerHumanoid IKController;
    ControllerAIHumanoid controller;
    int shootingHash;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.shootingHash = Animator.StringToHash("Shooting");
            this.IKController = animator.GetComponent<IKControllerHumanoid>();
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.firstSetting = false;
        }
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;

        if (IKController.IKWeight > 0.02f)
        {
            IKController.IKWeight = Mathf.Lerp(IKController.IKWeight, 0f, Time.deltaTime * 3f);
        }

        if (controller.IsInFieldOfView(controller.CurrentEnemy))
        {
            animator.SetBool(shootingHash, true);
        }

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
