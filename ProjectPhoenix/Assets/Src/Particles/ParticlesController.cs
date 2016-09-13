using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ParticlesController : MonoBehaviour
{
    public List<GameObject> MuzzleVfxs;
    public List<GameObject> HitVfxs;
    public List<GameObject> SideHitVfxs;
    public List<GameObject> MissVfxs;
    public List<GameObject> SideMissVfxs;
    public float MuzzleScaleCoef;
    public float HitScaleCoef;
    public float MissScaleCoef;

    internal void PlayMuzzleVfx(Vector3 vPosition, Vector3 vDirection)
    {
        IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(MuzzleVfxs[Random.Range(0, MuzzleVfxs.Count)]);
        effectParticle.PlayEffect(vPosition, vDirection, MuzzleScaleCoef, false);
    }
    internal void PlayHitVfx(Vector3 vPosition, Vector3 vNormal)
    {
        if (CheckIsSide(vNormal))
        {
            IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(SideHitVfxs[Random.Range(0, SideHitVfxs.Count)]);
            effectParticle.PlayEffect(vPosition, vNormal, HitScaleCoef, true);
        }
        else
        {
            IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(HitVfxs[Random.Range(0, HitVfxs.Count)]);
            effectParticle.PlayEffect(vPosition, vNormal, HitScaleCoef, false);
        }
    }
    internal void PlayMissVfx(Vector3 vPosition, Vector3 vNormal)
    {
        if(CheckIsSide(vNormal))
        {
            IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(SideMissVfxs[Random.Range(0, SideMissVfxs.Count)]);
            effectParticle.PlayEffect(vPosition, vNormal, MissScaleCoef, true);
        }
        else
        {
            IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(MissVfxs[Random.Range(0, MissVfxs.Count)]);
            effectParticle.PlayEffect(vPosition, vNormal, MissScaleCoef, false);
        }
    }


    internal static bool CheckIsSide(Vector3 hitNormal)
    {
        return hitNormal.y == 0;
    }
}
