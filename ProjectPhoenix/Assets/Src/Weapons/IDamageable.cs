using UnityEngine;
using System.Collections;

public interface IDamageable
{
    void Damage(float fDmg);
    void Damage(object damage);
}
