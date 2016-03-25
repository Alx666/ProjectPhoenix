using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(IWeapon))]
public class TEST_CAMERA_WEAPON : MonoBehaviour
{
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
