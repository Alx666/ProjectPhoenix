using UnityEngine;
using System.Collections;

public interface IVisualEffect
{
    void PlayEffect(Vector3 vPosition, Vector3 vDirection, float scaleCoef, bool isSide);
}
