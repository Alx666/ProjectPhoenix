using UnityEngine;
using System.Collections.Generic;
using System;
using RootMotion.FinalIK;

public class ControllerPlayerWheel : ControllerWheels
{
    public List<IKWheel> WheelsConfig;


    protected override void Awake()
    {
        base.Awake();

        m_hWheels.Clear();
        WheelsConfig.ForEach(hW => m_hWheels.Add(hW));
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void LateUpdate()
    {
        m_hWheels.ForEach(hW => (hW as IKWheel).OnLateUpdate());
    }

    public override void BeginTurnRight()
    {
        m_hWheels[2].Steer(-this.SteerAngle);
        m_hWheels[3].Steer(-this.SteerAngle);

        base.BeginTurnRight();
    }

    public override void EndTurnRight()
    {
        base.EndTurnRight();
        
        if (m_hLeft)
        {
            m_hWheels[2].Steer(this.SteerAngle);
            m_hWheels[3].Steer(this.SteerAngle);
        }
        else
        {
            m_hWheels[2].Steer(0);
            m_hWheels[3].Steer(0);
        }
    }

    public override void BeginTurnLeft()
    {        
        m_hWheels[2].Steer(this.SteerAngle);
        m_hWheels[3].Steer(this.SteerAngle);

        base.BeginTurnLeft();
    }

    public override void EndTurnLeft()
    {
        base.EndTurnLeft();

        if (m_hRight)
        {
            m_hWheels[2].Steer(-this.SteerAngle);
            m_hWheels[3].Steer(-this.SteerAngle);
        }
        else
        {
            m_hWheels[2].Steer(0);
            m_hWheels[3].Steer(0);
        }
    }

    [Serializable]
    public class IKWheel : Wheel
    {
        public CCDIK        IK;
        public GameObject   Axle;

        private Vector3 m_vLastPosition;
        private Quaternion m_vLastRotation;

        public override void Steer(float fSteer)
        {
            base.Steer(fSteer);

            Axle.transform.localRotation = Quaternion.Euler(0f, fSteer, 0f);
        }

        public override void OnUpdate()
        {
            Collider.GetWorldPose(out m_vLastPosition, out m_vLastRotation);

            //Gfx.transform.position = m_vLastPosition;
            Gfx.transform.rotation = m_vLastRotation;

            if (Collider.isGrounded)
                IsWheelGrounded = true;
            else
                IsWheelGrounded = false;

            IK.solver.SetIKPosition(m_vLastPosition);
        }

        public void OnLateUpdate()
        {
            
        }
    }
}
