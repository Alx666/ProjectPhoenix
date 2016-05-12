using UnityEngine;
using System.Collections;

public class HumanoidShootSM : StateMachineBehaviour
{
    Quaternion originalRotation;
    ControllerAIHumanoid controller;
    int shootingHash;
    IWeapon weapon;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.shootingHash = Animator.StringToHash("Shooting");
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.weapon = animator.GetComponentInChildren<IWeapon>();
            this.originalRotation = controller.WeaponLocator.transform.rotation;
            this.firstSetting = false;
        }

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        this.weapon.Press();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        this.weapon.Release();
    }

}
