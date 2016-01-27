using UnityEngine;
using System.Collections;

public interface IWeapon
{
    void OnFireButtonPressed();
    void OnFireButtonReleased();
    bool IsFiring { get; }
}
