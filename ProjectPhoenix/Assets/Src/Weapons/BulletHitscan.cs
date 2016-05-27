using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

/// <summary>
/// Handles instant-hit cases with a single raycast and resolve graphics in a couple of frame (probably with line renderer)
/// sniper, vulcan, etc
/// </summary>
public class BulletHitscan : NetworkBehaviour, IBullet, IPoolable
{
    public void Shoot(Vector3 vPosition, Vector3 vDirection, Vector3 vWDirection)
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
