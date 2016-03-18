using UnityEngine;
using System.Collections;

public class DeleteEffect : MonoBehaviour
{
    public float Duration;

    void Start()
    {
        StartCoroutine(Wait(Duration));
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(this.gameObject);
    }
}
