using UnityEngine;
using System.Collections;

public class AIHumanoidWaitSM : StateMachineBehaviour
{
    [Range(0f, 10f)]
    public float WaitTime;
    int toReachPointHash;
    float elapsedTime;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.toReachPointHash = Animator.StringToHash("ToReachPoint");
            this.firstSetting = false;
        }
        this.elapsedTime = Time.time;
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Time.time - elapsedTime >= WaitTime)
        {
            animator.SetBool(toReachPointHash, true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
