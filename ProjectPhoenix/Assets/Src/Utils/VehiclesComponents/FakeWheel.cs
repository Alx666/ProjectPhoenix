using UnityEngine;
using System.Collections;

public class FakeWheel : MonoBehaviour
{
    internal void OnUpdate(WheelCollider wheel)
    {
        Vector3 position;
        Quaternion rotation;
        wheel.GetWorldPose(out position, out rotation);
        this.transform.rotation = rotation;
    }

}
