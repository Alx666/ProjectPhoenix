using UnityEngine;
using System.Collections;

public class BulletVanguard : BulletRaycast
{
    int vanguardLayerHash;
    void Start()
    {
        this.vanguardLayerHash = 1 << LayerMask.NameToLayer("Vanguard");
        this.vanguardLayerHash = ~vanguardLayerHash;
    }



}
