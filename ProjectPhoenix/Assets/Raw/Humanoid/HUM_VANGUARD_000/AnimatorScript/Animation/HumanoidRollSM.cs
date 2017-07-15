using UnityEngine;
using System.Collections;

public class HumanoidRollSM : StateMachineBehaviour
{
    //Animator animator;
    bool firstSetting = true;
    int rollingHash;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            //this.animator = animator;
            this.rollingHash = Animator.StringToHash("Rolling");
            this.firstSetting = false;
        }

    }
    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(rollingHash, false);

    }

}
