using UnityEngine;
using System.Collections;

public class TEST_CAMERA_WEAPON : MonoBehaviour
{
    private IWeapon m_hWeapon;


    void Awake()
    {
        m_hWeapon = this.GetComponent<IWeapon>();
    }

    void Update()
    {

    }
}
