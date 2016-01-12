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
    public Transform Raycast;
    public float MinRange;

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
        if (IsFindTarget())
        {
            Patrol();
        }
        else
            Idle();
    }

    public void Idle()
    {
       
    }

    public void Patrol()
    {
        float distance = Vector3.Distance(Target.transform.position, this.transform.position);
        if (distance < maxDistance)
        { 

            // Y axis rotate
            Vector3 baseDirection = (Target.transform.position - cannon.transform.position).normalized;
            float rotY = Mathf.Atan2(baseDirection.x, baseDirection.z) * (180 / Mathf.PI);

            baseCannon.transform.rotation = Quaternion.Slerp(baseCannon.transform.rotation, Quaternion.Euler(baseCannon.transform.eulerAngles.x, rotY, baseCannon.transform.eulerAngles.z), Time.deltaTime * maxVelocity);

            //Xaxis rotate
            Vector3 cannonInclination = cannon.transform.rotation.eulerAngles;
            float angle = Vector3.Distance(Target.transform.position, cannon.transform.position) / vRotation;

            float angle2 = Target.transform.position.y - cannon.transform.position.y;
            cannonInclination.y = (-Mathf.Atan(angle2) * (180 / Mathf.PI)) / 4.0F;

            cannonInclination.y = Mathf.Clamp(cannonInclination.y, MinRange ,0F);
            baseCannon.transform.rotation = Quaternion.Slerp(baseCannon.transform.rotation, Quaternion.Euler(baseCannon.transform.eulerAngles.x, rotY, baseCannon.transform.eulerAngles.z), Time.deltaTime * vRotation);
            cannon.transform.rotation = Quaternion.Slerp(cannon.transform.rotation, Quaternion.Euler(cannonInclination.y, baseCannon.transform.eulerAngles.y, cannon.transform.eulerAngles.z), Time.deltaTime * vRotation);
        }
    }

    //se il la distanza del target è minore uguale a quella della torretta lancia un raycast per controllare se è visibile se true la torretta si girera in direzione del nemico altrimenti continuera su idle
    private bool IsFindTarget()
    {
        bool isFindTarget;
        RaycastHit hit;

        Ray ray;

        ray = new Ray(Raycast.transform.position, cannon.transform.forward);

        Physics.Raycast(ray, out hit, maxDistance);


        Debug.DrawRay(ray.origin, ray.direction, Color.red);
        if (hit.transform != null)
        {
            if (hit.transform.tag == Target.transform.tag)
            {
              return  isFindTarget = true;
            }
        }

            return isFindTarget= true;
    }
}