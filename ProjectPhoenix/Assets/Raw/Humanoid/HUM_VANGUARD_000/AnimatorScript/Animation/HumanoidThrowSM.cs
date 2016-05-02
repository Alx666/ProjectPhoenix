using UnityEngine;
using System.Collections;

public class HumanoidThrowSM : StateMachineBehaviour
{
    ControllerAIHumanoid own;
    bool firstSetting = true;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(firstSetting)
        {
            this.own = animator.GetComponent<ControllerAIHumanoid>();
        }
        own.WeaponLocator.transform.SetParent(null);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        own.WeaponLocator.transform.SetParent(own.RightHand);

    }

}
