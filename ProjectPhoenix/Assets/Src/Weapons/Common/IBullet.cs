using UnityEngine;
using System.Collections;

public interface IDamageSource
{
    Actor Owner { get; set; }
    
}

public interface IBullet : IDamageSource
{
    void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection, Actor hOwner);
}
