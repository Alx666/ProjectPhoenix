using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

internal class ControllerPlayerWheels : MonoBehaviour, IControllerPlayer
{
    [SerializeField]
    internal float SteerAngle = 25;
    [SerializeField]
    internal float BrakeForce = 100f;
    [SerializeField]
    internal float Hp = 100f;

    private new Rigidbody rigidbody;
    //private Transform turretTransform;
    private List<Wheel> wheels;
    private DriveSystem drive;
    private BrakeSystem brake;
    private bool reverse;

    internal void Awake()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        wheels = new List<Wheel>();

        //Initialize wheels
        List<Transform> gfxPos = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<WheelCollider>() == null).ToList();
        this.GetComponentsInChildren<WheelCollider>().ToList().ForEach(hW => wheels.Add(new Wheel(hW, gfxPos.OrderBy(hP => Vector3.Distance(hP.position, hW.transform.position)).First().gameObject)));
        wheels = wheels.OrderByDescending(hW => hW.Collider.transform.position.z).ToList();

        //Initialize Drive/Brake State Machine
        drive = new DriveSystem(Hp, wheels);
        brake = new BrakeSystem(0.4f * BrakeForce, 0.6f * BrakeForce, wheels);
    }

    internal void Update()
    {
        wheels.ForEach(hW => hW.OnUpdate());
    }

    #region IControllerPlayer 

    public void BeginBackward()
    {
        if (Mathf.Approximately(rigidbody.velocity.magnitude, 0f))
        {
            drive.BeginReverse();
            reverse = true;
        }
        else if (!reverse)
            brake.BeginBrake();
    }

    public void BeginDown()
    {
        throw new NotImplementedException();
    }

    public void BeginFire()
    {
        throw new NotImplementedException();
    }

    public void BeginForward()
    {
        drive.BeginAccelerate();
    }

    public void BeginPanLeft()
    {
        throw new NotImplementedException();
    }

    public void BeginPanRight()
    {
        throw new NotImplementedException();
    }

    public void BeginTurnLeft()
    {
        wheels[0].Steer(-this.SteerAngle);
        wheels[1].Steer(-this.SteerAngle);
    }

    public void BeginTurnRight()
    {
        wheels[0].Steer(this.SteerAngle);
        wheels[1].Steer(this.SteerAngle);
    }

    public void BeginUp()
    {
        throw new NotImplementedException();
    }

    public void EndBackward()
    {
        if (!reverse)
            brake.EndBrake();
        else
        {
            drive.EndReverse();
            reverse = false;
        }
    }

    public void EndDown()
    {
        throw new NotImplementedException();
    }

    public void EndFire()
    {
        throw new NotImplementedException();
    }

    public void EndForward()
    {
        drive.EndAccelerate();
    }

    public void EndPanLeft()
    {
        throw new NotImplementedException();
    }

    public void EndPanRight()
    {
        throw new NotImplementedException();
    }

    public void EndTurnLeft()
    {
        wheels[0].Steer(0);
        wheels[1].Steer(0);
    }

    public void EndTurnRight()
    {
        wheels[0].Steer(0);
        wheels[1].Steer(0);
    }

    public void EndUp()
    {
        throw new NotImplementedException();
    }

    public void MousePosition(Vector3 vMousePosition)
    {
        //RaycastHit hit;
        //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        //    this.m_hTurretPosition.LookAt(hit.point);
    }
    #endregion

    #region DriveSystem
    internal class DriveSystem
    {
        private float hp;
        private List<Wheel> wheels;

        internal DriveSystem(float fHp, List<Wheel> hWheels)
        {
            hp = fHp;
            wheels = hWheels;
        }

        internal void BeginAccelerate()
        {
            //AWD
            wheels.ForEach(hW => hW.Collider.motorTorque = hp * 0.25f);

            //FWD
            //wheels.Take(2).ToList().ForEach(hW => hW.Collider.motorTorque = hp * 0.5f); 

            //BWD
            //wheels.Skip(2).ToList().ForEach(hW => hW.Collider.motorTorque = hp * 0.5f);
        }

        internal void EndAccelerate()
        {
            wheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }

        internal void BeginReverse()
        {
            //AWD
            wheels.ForEach(hW => hW.Collider.motorTorque = -(hp * 0.25f));

            //FWD
            //wheels.Take(2).ToList().ForEach(hW => hW.Collider.motorTorque = -(hp * 0.5f)); 

            //BWD
            //wheels.Skip(2).ToList().ForEach(hW => hW.Collider.motorTorque = -(hp * 0.5f));
        }

        internal void EndReverse()
        {
            wheels.ForEach(hW => hW.Collider.motorTorque = 0f);
        }
    }
    #endregion

    #region BrakeSystem
    internal class BrakeSystem
    {
        private float forwardBrake;
        private float backwardBrake;
        private List<Wheel> wheels;

        internal BrakeSystem(float fForwardBrake, float fBackwardBrake, List<Wheel> hWheels)
        {
            forwardBrake = fForwardBrake;
            backwardBrake = fBackwardBrake;
            wheels = hWheels;
        }

        internal void BeginBrake()
        {
            wheels.Take(2).ToList().ForEach(hW => hW.Collider.brakeTorque = forwardBrake);
            wheels.Skip(2).ToList().ForEach(hW => hW.Collider.brakeTorque = backwardBrake);
        }

        internal void EndBrake()
        {
            wheels.ForEach(hW => hW.Collider.brakeTorque = 0f);
        }
    }
    #endregion

    #region Wheel
    internal class Wheel
    {
        internal WheelCollider Collider { get; private set; }
        internal GameObject Gfx { get; private set; }

        internal Wheel(WheelCollider coll, GameObject gfx)
        {
            Collider = coll;
            Gfx = gfx;
        }

        internal void Steer(float fSteer)
        {
            Collider.steerAngle = fSteer;
        }

        internal void OnUpdate()
        {
            Vector3 position;
            Quaternion rotation;
            Collider.GetWorldPose(out position, out rotation);

            Gfx.transform.position = position;
            Gfx.transform.rotation = rotation;
        }
    }
    #endregion

    [System.Serializable]
    internal enum DriveType
    {
        Forward,
        Backward,
        AWD
    }
}


