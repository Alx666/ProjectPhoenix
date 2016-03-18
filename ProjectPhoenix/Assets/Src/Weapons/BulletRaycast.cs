using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// Handles fast straight-line flying projectiles based on raycast logic
/// </summary>
public class BulletRaycast : MonoBehaviour, IBullet, IPoolable
{    
    public float MaxDistance;
    
    
    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {       
    }

    void Update()
    {
    }

    #region IPoolable

    public Pool Pool { get; set; }

    public void Enable()
    {
    }

    public void Disable()
    {
    }

    #endregion
}
