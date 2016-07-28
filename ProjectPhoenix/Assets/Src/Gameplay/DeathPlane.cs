﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DeathPlane : MonoBehaviour
{
    private Collider m_hCollider;

    void Awake()
    {
        m_hCollider = this.GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider hColl)
    {
        Actor hActor = hColl.transform.root.GetComponent<Actor>();
        hActor.Die(hActor);
    }
}
