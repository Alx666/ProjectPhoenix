using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ParticlesController
{
    public List<GameObject> MuzzleVfxs;
    public List<GameObject> HitVfxs;
    public List<GameObject> SideHitVfxs;
    public List<GameObject> MissVfxs;
    public List<GameObject> SideMissVfxs;
    public float MuzzleScaleCoef;
    public float HitScaleCoef;
    public float MissScaleCoef;

    public void PlayMuzzleVfx(Vector3 vPosition, Vector3 vDirection)
    {
        IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(MuzzleVfxs[Random.Range(0, MuzzleVfxs.Count)]);
        effectParticle.PlayEffect(vPosition, vDirection, MuzzleScaleCoef, false);
    }

    public void PlayCollisionVfx(RaycastHit hit)
    {
        IDamageable hHitted = hit.collider.gameObject.GetComponent<IDamageable>();

        if (hit.normal == hit.transform.right || hit.normal == -hit.transform.right)
        {
            if(hHitted != null)
            {
                IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(SideHitVfxs[Random.Range(0, SideHitVfxs.Count)]);
                effectParticle.PlayEffect(hit.point, hit.normal, HitScaleCoef, true);
            }
            else
            {
                IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(SideMissVfxs[Random.Range(0, SideMissVfxs.Count)]);
                effectParticle.PlayEffect(hit.point, hit.normal, MissScaleCoef, true);
            }
        }
        else
        {
            if (hHitted != null)
            {
                IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(HitVfxs[Random.Range(0, HitVfxs.Count)]);
                effectParticle.PlayEffect(hit.point, hit.normal, HitScaleCoef, false);
            }
            else
            {
                IVisualEffect effectParticle = GlobalFactory.GetInstance<IVisualEffect>(MissVfxs[Random.Range(0, MissVfxs.Count)]);
                effectParticle.PlayEffect(hit.point, hit.normal, MissScaleCoef, false);
            }
        }
    }
}
