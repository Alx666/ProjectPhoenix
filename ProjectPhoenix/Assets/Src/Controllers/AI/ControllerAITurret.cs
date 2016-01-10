using UnityEngine;
using System.Collections;
using System;

internal class ControllerAITurret : MonoBehaviour, IControllerAI
{
    public GameObject Target;
    public float maxDistance;
    public float maxVelocity = 10.0F;
    private float vRotation = 30.0F;
    public Transform cannon;
    public Transform baseCannon;

    public GameObject target
    {
        get
        {
            return target;
        }

        set
        {
            target = Target;
        }
    }

    void Update()
    {
        Move();
    }

    public void Move()
    {
        float distance = Vector3.Distance(Target.transform.position, this.transform.position);
        if (distance < maxDistance)
        {


            // Y axis rotate
            Vector3 baseDirection = (Target.transform.position - cannon.transform.position).normalized;
            float rotY = Mathf.Atan2(baseDirection.x, baseDirection.z) * (180 / Mathf.PI);

            baseCannon.transform.rotation = Quaternion.Slerp(baseCannon.transform.rotation, Quaternion.Euler(baseCannon.transform.eulerAngles.x, rotY, baseCannon.transform.eulerAngles.z), Time.deltaTime * maxVelocity);

        }
    }
}
