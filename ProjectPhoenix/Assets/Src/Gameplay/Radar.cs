using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Radar : MonoBehaviour
{
    private Vector3             m_vScreenCenter;
    private List<TargetArrow>   m_hArrows;
    private List<float>         m_hFloats;
    private Plane               m_vPlaneRight;
    private Plane               m_vPlaneUpper;
    private Plane               m_vPlaneLeft;
    private Plane               m_vPlaneBottom;

    public GameObject           Player        { get; set; }
    public List<Image>          GuiArrows;

    [Range(1.5f, 3.0f)]
    public float                ScreenCoeff = 2.2f;

    [Range(100f, 350f)]
    public float                MaxDistance = 350f;


    void Awake()
    {
        m_hArrows = new List<TargetArrow>();
        m_hFloats = new List<float>();
        GuiArrows.ForEach(x => x.enabled = false);

        m_vScreenCenter     = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        m_vPlaneRight       = new Plane(Vector3.left, Screen.width    / ScreenCoeff);
        m_vPlaneUpper       = new Plane(Vector3.down, Screen.height   / ScreenCoeff);
        m_vPlaneLeft        = new Plane(Vector3.left, -Screen.width   / ScreenCoeff);
        m_vPlaneBottom      = new Plane(Vector3.down, -Screen.height  / ScreenCoeff);
    }

	
	void LateUpdate ()
    {
        if (Player == null)
            return;

        m_hFloats.Clear();

        Vector3 vPlayer2D = Camera.main.WorldToScreenPoint(Player.transform.position);

        for (int i = 0; i < m_hArrows.Count; i++)
        {
            TargetArrow hCurrent    = m_hArrows[i];
            Vector3 vEnemyRelative2D = Camera.main.WorldToScreenPoint(hCurrent.Target.transform.position);

            bool bNotInRangeCondition      = Vector3.Distance(hCurrent.Target.transform.position, Player.transform.position) > MaxDistance;
            bool bNotInScreenCondition  = vEnemyRelative2D.x > 0 && vEnemyRelative2D.x < Screen.width && vEnemyRelative2D.y > 0 && vEnemyRelative2D.y < Screen.height;

            if (bNotInScreenCondition || bNotInRangeCondition)
            {
                hCurrent.Arrow.enabled = false;
                continue;
            }
            else
            {
                hCurrent.Arrow.enabled = true;

                vEnemyRelative2D = (vEnemyRelative2D - vPlayer2D).normalized;

                Ray vRay = new Ray(Vector3.zero, vEnemyRelative2D);

                m_hFloats = new List<float>();

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

                vBorder += m_vScreenCenter;

                hCurrent.Arrow.rectTransform.right = -(vBorder - m_vScreenCenter).normalized;
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
