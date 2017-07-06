using UnityEngine;
using System.Collections;
using Pathfinding;

public class AIHumanoidKeepDistanceSM : StateMachineBehaviour
{
    IKControllerHumanoid IKController;
    int toKeepDistanceHash;
    int rollingHash;
    ControllerAIHumanoid controller;
    bool firstSetting = true;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (firstSetting)
        {
            this.IKController = animator.GetComponent<IKControllerHumanoid>();
            this.toKeepDistanceHash = Animator.StringToHash("ToKeepDistance");
            this.rollingHash = Animator.StringToHash("Rolling");
            this.controller = animator.GetComponent<ControllerAIHumanoid>();
            this.firstSetting = false;
        }
        if (!controller.isServer)
            return;

        if (Random.Range(0, 4) == 0)
        {
            animator.SetBool(rollingHash, true);
        }
        IKController.SetDefaultWeaponTarget();
        controller.RVOController.enableRotation = true;
        Vector3 thePointToFleeFrom = controller.CurrentEnemy.transform.position;
        int theGScoreToStopAt = 1000;
        FleePath path = FleePath.Construct(animator.transform.position, thePointToFleeFrom, theGScoreToStopAt);
        path.aimStrength = 10;
        controller.ManuallySetPath(path);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;

        if (Vector3.Distance(animator.transform.position, controller.CurrentEnemy.transform.position) > 8f && !controller.IsInFieldOfView(controller.CurrentEnemy))
            animator.SetBool(toKeepDistanceHash, false);
        controller.ManuallyMoveAgent();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!controller.isServer)
            return;
        controller.ManuallyStopAgent();
        controller.RVOController.enableRotation = false;
    }

}
