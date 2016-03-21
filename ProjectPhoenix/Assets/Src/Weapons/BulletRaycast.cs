using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// Handles fast straight-line flying projectiles based on raycast logic
/// </summary>
public class BulletRaycast : MonoBehaviour, IBullet, IPoolable
{
    public float MaxDistance = 50f;
    public float Speed       = 1f;
    public float Damage      = 10f;

    private float m_fTotalDistance;

    
    public void Shoot(Vector3 vPosition, Vector3 vDirection)
    {
        this.gameObject.transform.position  = vPosition;
        this.gameObject.transform.forward   = vDirection;
    }

    void Update()
    {
        RaycastHit vRaycast;

        float fSpace = Speed * Time.deltaTime;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out vRaycast, fSpace))
        {
            IDamageable hHit = vRaycast.collider.gameObject.GetComponent<IDamageable>();

            if (hHit != null)
            {
                hHit.Damage(Damage);

                //Todo: parcicle hit
            }
            else
            {
                //Todo: particle miss
            }

            GlobalFactory.Recycle(this.gameObject);

        }
        else
        {
            this.transform.position += this.transform.forward * fSpace;

            m_fTotalDistance += fSpace;

            if (m_fTotalDistance > MaxDistance)
                GlobalFactory.Recycle(this.gameObject);
        }
    }

    #region IPoolable

    public Pool Pool { get; set; }

    public void Enable()
    {
        this.gameObject.SetActive(true);
        m_fTotalDistance = 0f;
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    #endregion
}
