﻿using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Handles instant-hit cases with a single raycast and resolve graphics in a couple of frame (probably with line renderer)
/// sniper, vulcan, etc
/// </summary>
public class BulletHitscan : MonoBehaviour, IBullet, IPoolable
{
    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        
    }

    #region IPoolable 

    public Pool Pool
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public void Disable()
    {
        throw new NotImplementedException();
    }

    public void Enable()
    {
        throw new NotImplementedException();
    }

    #endregion
}
