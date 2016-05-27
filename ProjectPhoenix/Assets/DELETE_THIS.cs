using UnityEngine;
using System.Collections;

public class DELETE_THIS : MonoBehaviour
{
    private Weapon m_hSwarm;
    private bool m_bRelease;

	void Awake()
    {
        m_hSwarm = this.GetComponent<Weapon>();

    }
	
	
	void Update ()
    {
        if (m_bRelease)
        {
            m_hSwarm.Release();
            m_bRelease = false;
        }
            

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit vHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out vHit))
            {
                m_hSwarm.Press();
                m_bRelease = true;
            }
        }
	}
}
