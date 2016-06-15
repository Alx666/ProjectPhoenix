using UnityEngine;
using System.Collections;

public interface IWeapon
{
    void Press();
    void Release();
    bool IsFiring { get; }
    Vector3 Direction { get; set; }
    Actor Owner { get; set; }
}
