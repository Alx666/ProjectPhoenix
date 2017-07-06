using UnityEngine;
using System.Collections;

public class AIHumanoidStationarySM : StateMachineBehaviour
{
    ControllerAIHumanoid controller;
    int toKeepDistanceHash;
    int toStationaryHash;
    bool firstSetting = true;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (firstSetting)
        {
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.toKeepDistanceHash = Animator.StringToHash("ToKeepDistance");
            this.toStationaryHash = Animator.StringToHash("ToStationary");

        }
        if (!controller.isServer)
            return;
        controller.AI.enabled = false;
        controller.RVOController.Move(Vector3.zero);
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;
        if (Vector3.Distance(animator.transform.position, controller.CurrentEnemy.transform.position) < 7.5f)
        {
            animator.SetBool(toKeepDistanceHash, true);
        }
        else if(Vector3.Distance(animator.transform.position, controller.CurrentEnemy.transform.position) > 10f || !controller.IsInFieldOfView(controller.CurrentEnemy))
        {
            animator.SetBool(toStationaryHash, false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
