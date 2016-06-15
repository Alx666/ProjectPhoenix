using UnityEngine;
using System.Collections;

public interface IBeam : IDamageSource
{
    void Enable(Vector3 vPos, Vector3 vDir);
    void Disable();
}
