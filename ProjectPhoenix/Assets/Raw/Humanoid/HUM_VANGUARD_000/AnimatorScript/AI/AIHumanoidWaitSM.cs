using UnityEngine;
using System.Collections;

public class AIHumanoidWaitSM : StateMachineBehaviour
{
    [Range(0f, 10f)]
    public float WaitTime;

    float elapsedTime;
    ControllerAIHumanoid controller;
    int[] triggers;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.triggers = new int[2] { Animator.StringToHash("ToLookAround"), Animator.StringToHash("ToReachPoint") };

            this.firstSetting = false;
        }
        this.elapsedTime = Time.time;
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.ManualSetAnimator(0f, 0f, true);
        if (Time.time - elapsedTime >= WaitTime)
        {
            animator.SetTrigger(triggers[Random.Range(0, 2)]);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
