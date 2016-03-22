using UnityEngine;
using System.Collections;

public interface IRechargeableWeapon : IWeapon
{
    void RechargeMechanics();

    void Fire();
}
