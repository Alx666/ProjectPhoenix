using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(CCDIK))]
public class IKMech000Foot : MonoBehaviour
{
    private CCDIK m_hIK;

    void Awake()
    {
        m_hIK = this.GetComponent<CCDIK>();
    }
		
	void LateUpdate()
    {
        RaycastHit vHit;
        if (Physics.Raycast(new Ray(this.transform.position, Vector3.down), out vHit))
        {
            m_hIK.solver.IKPosition = vHit.point;
        }

        //this.transform.rotation = Quaternion.LookRotation(Vector3.forward);

    }


}
