using UnityEngine;
using System.Collections;

public class AIHumanoidLookAroundSM : StateMachineBehaviour
{
    ControllerAIHumanoid controller;
    int toWait;
    float targetRotation;
    float[] degrees;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.toWait = Animator.StringToHash("ToWait");
            this.degrees = new float[2] { 90f, 180f };
            this.firstSetting = false;
        }
        if (controller. agent.isActiveAndEnabled)
            this.controller.agent.Stop();

        this.controller.agent.speed = 0.4f;
        int randomDegree = Random.Range(0, 2);
        this.targetRotation = Mathf.Repeat(animator.rootRotation.eulerAngles.y + degrees[randomDegree], 360f);
        controller.ManualSetAnimator(0f, 0.5f, false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.rootRotation.eulerAngles.y <= targetRotation + 6f && animator.rootRotation.eulerAngles.y >= targetRotation - 6f)
        {
            animator.SetBool(toWait, true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
