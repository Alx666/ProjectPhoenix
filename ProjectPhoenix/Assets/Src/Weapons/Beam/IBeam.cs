using UnityEngine;
using System.Collections;

public interface IBeam
{
    void Enable(Vector3 vPos, Vector3 vDir);
    void Disable();
}
