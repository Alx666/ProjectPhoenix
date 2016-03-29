using UnityEngine;
using System.Collections;

public class FlammableObject : MonoBehaviour
{
    public ParticleSystem Flame;
    
    public void SetOnFire()
    {
        Instantiate(Flame);
        Flame.transform.position = this.transform.position;
        Flame.Play();
    }
}