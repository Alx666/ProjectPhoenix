using UnityEngine;
using System.Collections;

public class HumanoidShootSM : StateMachineBehaviour
{
    ControllerAIHumanoid controller;
    IKControllerHumanoid IKController;
    int shootingHash;
    Transform PrevEnemy;
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

        if (!controller.isServer)
            return;
        IKController.SetWeaponTarget(controller.CurrentEnemy.transform);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (!controller.isServer)
            return;

        if (IKController.IKWeight < 0.98f)
        {
            IKController.IKWeight = Mathf.Lerp(IKController.IKWeight, 1f, Time.deltaTime * 3f);
        }
        else
        {
            controller.RpcStartShooting();
        }
                
        if (!controller.IsInFieldOfView(controller.CurrentEnemy))
        {
            controller.RpcStopShooting();
            animator.SetBool(shootingHash, false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
