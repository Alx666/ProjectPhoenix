using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class TEST_CAMERA_WEAPON : MonoBehaviour
{
    public GameObject TurretHub;
    public GameObject TurretGuns;

    private List<IWeapon>   m_hWeapons;
    private IWeapon         m_hCurrent;

    

    void Awake()
    {
        m_hWeapons = this.GetComponents<IWeapon>().ToList();
        m_hCurrent = m_hWeapons.First();
    }

    void Update()
    {
        Ray vRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit vHit;
        if (Physics.Raycast(vRay, out vHit))
        {
            Vector3 vDirecton = vHit.point - this.transform.position;
            vDirecton.y = 0;
            vDirecton.Normalize();
            TurretHub.transform.forward = vDirecton;

            //Vector3 vHeght = vHit.point - TurretGuns.transform.position;
            //vHeght.x = 0;
            //vHeght.z = 0;
            //vHeght.Normalize();

            TurretGuns.transform.LookAt(vHit.point);
        }

        
        
        for (int i = 48; i < 58; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                int iIndex = i - 48;
                if (m_hWeapons.Count > iIndex)
                {
                    m_hCurrent = m_hWeapons[iIndex];
                }
            }
        }

        m_hCurrent.Direction = vRay.direction;

        if (Input.GetMouseButtonDown(0))
            m_hCurrent.Press();
        if(Input.GetMouseButtonUp(0))
            m_hCurrent.Release();
    }
}
