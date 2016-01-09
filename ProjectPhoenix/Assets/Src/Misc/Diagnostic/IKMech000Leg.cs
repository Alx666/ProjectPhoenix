using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using System;

public class IKMech000Leg : MonoBehaviour
{
    public GameObject LegUpLocator;
    public GameObject LegStepFrontRaycaster;
    public GameObject LegStepRearRaycaster;
    public GameObject Foot;

    [Range(1f, 100f)]
    public float StepSpeed = 1.0f;

    private CCDIK       m_hIk;
    private ILegState   m_hState;

	void Awake ()
    {
        m_hIk = this.GetComponent<CCDIK>();

        LegStateOnGround hOnGround      = new LegStateOnGround(Foot);
        LegStateExtends  hRetractFront  = new LegStateExtends(m_hIk, LegUpLocator, StepSpeed, false);
        LegStateExtends  hRetractRear   = new LegStateExtends(m_hIk, LegUpLocator, StepSpeed, false);
        LegStateExtends  hExtendFront   = new LegStateExtends(m_hIk, LegStepFrontRaycaster, StepSpeed, true);
        LegStateExtends  hExtendRear    = new LegStateExtends(m_hIk, LegStepRearRaycaster, StepSpeed, true);
        hOnGround.NextFront             = hRetractFront;
        hRetractFront.Next              = hExtendFront;
        hExtendFront.Next               = hOnGround;
        hOnGround.NextRear              = hRetractRear;
        hRetractRear.Next               = hExtendRear;
        hExtendRear.Next                = hOnGround;
        m_hState                        = hOnGround;
    }
		
	void Update ()
    {
        m_hState = m_hState.Update();
	}

    private interface ILegState
    {
        void OnEnter();
        ILegState Update();
    }

    private class LegStateOnGround : ILegState
    {
        private GameObject m_hFoot;
        public ILegState NextFront { get; set; }
        public ILegState NextRear  { get; set; }

        public LegStateOnGround(GameObject hFoot)
        {
            m_hFoot = hFoot;
        }

        public void OnEnter()
        {
        }

        public ILegState Update()
        {
            float fAngle = Vector3.Angle(-m_hFoot.transform.up, Vector3.down);
            
            if (fAngle > 2f)
            {
                NextFront.OnEnter();
                return NextFront;
            }
            else if (fAngle < -2f)
            {
                NextRear.OnEnter();
                return NextRear;
            }
            else
            {
                return this;
            }
                
        }
    }

    private class LegStateExtends : ILegState
    {
        private CCDIK       m_hIk;
        private Vector3     m_vStartPosition;
        private Vector3     m_vEndPosition;
        private GameObject  m_hRaycaster;
        private float       m_fTime;
        private float       m_fStepSpeed;
        private bool        m_bUseRaycast;
        public ILegState    Next { get; set; }

        public LegStateExtends(CCDIK hIk, GameObject hTarget, float fStepSpeed, bool bUseRaycast)
        {
            m_hIk        = hIk;
            m_hRaycaster = hTarget;
            m_fStepSpeed = fStepSpeed;
            m_bUseRaycast = bUseRaycast;
        }


        public void OnEnter()
        {
            m_vStartPosition = m_hIk.solver.IKPosition;

            RaycastHit vHit;
            if (m_bUseRaycast)
            {
                Physics.Raycast(new Ray(m_hRaycaster.transform.position, Vector3.down), out vHit);
                m_vEndPosition = vHit.point;
            }
            else
            {
                m_vEndPosition = m_hRaycaster.transform.position;
            }

            m_fTime = 0f;
        }

        public ILegState Update()
        {
            m_fTime                    += Time.deltaTime * m_fStepSpeed;
            Vector3 vPos                = Vector3.Lerp(m_vStartPosition, m_vEndPosition, m_fTime);
            m_hIk.solver.IKPosition     = vPos;

            if (Vector3.Distance(vPos, m_vEndPosition) <= 0.1f)
            {
                Next.OnEnter();
                return Next;
            }
            else
            {
                return this;
            }                        
        }
    }


}
