using UnityEngine;
using System.Collections;
using System;

public class SimpleVfx : MonoBehaviour, IVisualEffect, IPoolable
{
    public float Duration;

    public Pool Pool { get; set; }

    public void PlayEffect(Vector3 vPosition, Vector3 vDirection, float scaleCoef, bool isSide)
    {
        this.gameObject.transform.position = vPosition;

        if (isSide)
            this.gameObject.transform.right = -vDirection;
        else
            this.gameObject.transform.forward = vDirection;

        this.gameObject.transform.localScale = this.gameObject.transform.localScale * scaleCoef;

        StartCoroutine(Wait(Duration));
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        GlobalFactory.Recycle(this.gameObject);
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
}
