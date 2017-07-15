using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Radar : MonoBehaviour
{
    //private Vector3             m_vScreenCenter;
    private List<TargetArrow>   m_hArrows;
    private List<float>         m_hFloats;
    private Plane               m_vPlaneRight;
    private Plane               m_vPlaneUpper;
    private Plane               m_vPlaneLeft;
    private Plane               m_vPlaneBottom;
    private Vector2             m_vScreenOffset;

    public GameObject           Player        { get; set; }
    public List<Image>          GuiArrows;

    [Range(100f, 350f)]
    public float                MaxDistance = 350f;


    void Awake()
    {
        m_hArrows = new List<TargetArrow>();
        m_hFloats = new List<float>();
        GuiArrows.ForEach(x => x.enabled = false);

        //m_vScreenCenter     = new Vector3(0.5f, 0.5f, 0f);
        m_vPlaneRight       = new Plane(Vector3.left,   1f);
        m_vPlaneUpper       = new Plane(Vector3.down,   1f);
        m_vPlaneLeft        = new Plane(Vector3.right,  0f);
        m_vPlaneBottom      = new Plane(Vector3.up,     0f);

        m_vScreenOffset     = new Vector2(20f, 20f);
    }

	
	void LateUpdate ()
    {
        if (Player == null)
            return;

        m_hFloats.Clear();

        Vector3 vPlayer2D = GameManager.Instance.CustomCamera.Camera.WorldToViewportPoint(Player.transform.position);

        for (int i = 0; i < m_hArrows.Count; i++)
        {
            TargetArrow hCurrent    = m_hArrows[i];
            Vector3 vEnemyRelative2D = GameManager.Instance.CustomCamera.Camera.WorldToViewportPoint(hCurrent.Target.transform.position);
            
            //PRENDILA PER VERAAA!!!
            vEnemyRelative2D = vEnemyRelative2D.z < 0f ? -vEnemyRelative2D : vEnemyRelative2D;

            bool bNotInRangeCondition      = Vector3.Distance(hCurrent.Target.transform.position, Player.transform.position) > MaxDistance;
            bool bInScreenCondition        = vEnemyRelative2D.x > 0f && vEnemyRelative2D.x < 1f && vEnemyRelative2D.y > 0f && vEnemyRelative2D.y < 1f;

            if (bInScreenCondition || bNotInRangeCondition)
            {
                hCurrent.Arrow.enabled = false;
                continue;
            }
            else
            {
                hCurrent.Arrow.enabled = true;

                Vector2 vP = vPlayer2D;
                Vector2 vT = vEnemyRelative2D;
                Vector2 vR = (vT - vP).normalized;

                Ray vRay = new Ray(vPlayer2D, vR);

                //m_hFloats = new List<float>();

                float fLeftRes;
                if (m_vPlaneLeft.Raycast(vRay, out fLeftRes))
                    m_hFloats.Add(fLeftRes);

                float fRightRes;
                if (m_vPlaneRight.Raycast(vRay, out fRightRes))
                    m_hFloats.Add(fRightRes);

                float fTopRes;
                if (m_vPlaneUpper.Raycast(vRay, out fTopRes))
                    m_hFloats.Add(fTopRes);

                float fBottomRes;
                if (m_vPlaneBottom.Raycast(vRay, out fBottomRes))
                    m_hFloats.Add(fBottomRes);

                float fMin = m_hFloats.Min();

                Vector3 vBorder = vRay.GetPoint(fMin);
                vBorder.x = Mathf.Clamp(vBorder.x *= Screen.width, m_vScreenOffset.x, Screen.width - m_vScreenOffset.x);
                vBorder.y = Mathf.Clamp(vBorder.y *= Screen.height, m_vScreenOffset.y, Screen.height - m_vScreenOffset.y);

                hCurrent.Arrow.rectTransform.right = -vRay.direction;
                Vector3 vAngles = hCurrent.Arrow.rectTransform.rotation.eulerAngles;
                vAngles.x = 0f;
                vAngles.y = 0f;
                hCurrent.Arrow.rectTransform.rotation = Quaternion.Euler(vAngles);

                hCurrent.Arrow.rectTransform.position = vBorder;

            }                                                            
        }
    }

    public void Add(GameObject hTarget)
    {
        if (GuiArrows.Count == 0)
            throw new System.Exception("Radar Player Limit Reached!");

        TargetArrow vNew = new TargetArrow();
        vNew.Target = hTarget;
        vNew.Arrow = GuiArrows.Last();
        vNew.Arrow.enabled = true;
        GuiArrows.RemoveAt(GuiArrows.Count - 1);
        m_hArrows.Add(vNew);
    }

    public void Remove(GameObject hTarget)
    {
        TargetArrow vToRemove = m_hArrows.Where(x => x.Target == hTarget).Single();
        vToRemove.Arrow.enabled = false;

        if (!m_hArrows.Remove(vToRemove))
            throw new System.Exception("Radar Bad Reference");

        GuiArrows.Add(vToRemove.Arrow);
    }

    public class TargetArrow
    {
        public GameObject   Target;
        public Image        Arrow;
    }
}
