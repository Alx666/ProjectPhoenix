using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using RootMotion.FinalIK;

public class IKControllerHumanoid : NetworkBehaviour
{
    [SyncVar]
    internal float IKWeight;

    public Transform WeaponLocator;
    public Transform LeftHandObj;
    public Transform RightHandObj;

    AimIK targetIk;
    Animator animator;
    void Start()
    {
        this.animator = GetComponent<Animator>();
        this.targetIk = GetComponent<AimIK>();
        this.targetIk.solver.target = WeaponLocator;
        this.IKWeight = targetIk.solver.IKPositionWeight;
    }

    void Update()
    {
        if (IKWeight != targetIk.solver.IKPositionWeight)
        {
            targetIk.solver.IKPositionWeight = IKWeight;
        }
    }
    internal void SetWeaponTarget(Transform target)
    {
        targetIk.solver.target = target;
        RpcSetWeaponTarget(target.gameObject.GetComponentInParent<NetworkIdentity>().netId);
    }
    internal void SetDefaultWeaponTarget()
    {
        targetIk.solver.target = WeaponLocator;
        RpcSetDefaultWeaponTarget();
    }
    [ClientRpc]
    void RpcSetDefaultWeaponTarget()
    {
        targetIk.solver.target = WeaponLocator;
    }
    [ClientRpc]
    void RpcSetWeaponTarget(NetworkInstanceId target)
    {
        //Prende la transform del veicolo per settare la mira correttamente.  Trovare un altro sistema!!!

        targetIk.solver.target = ClientScene.FindLocalObject(target).transform.GetComponent<ControllerWheels>().CentralPoint.transform;

    }
    void OnAnimatorIK()
    {
        if (animator)
        {

            if (RightHandObj != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandObj.rotation);
            }
            if (LeftHandObj != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetLookAtWeight(0);
            }
        }
    }
}
