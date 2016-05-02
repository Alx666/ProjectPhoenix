using UnityEngine;
using System.Collections;

public class HumanoidRollSM : StateMachineBehaviour
{
    NavMeshAgent agent;
    bool firstSetting = true;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.agent = animator.GetComponent<NavMeshAgent>();
            this.firstSetting = false;
        }

        agent.ResetPath();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        

    }

}
