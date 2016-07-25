using UnityEngine;
using System.Collections;
using Graph;

internal interface IControllerAI
{
    AIGraph AIGraph { get; set; }
    GameObject Target { get; set; }
    void Idle();
    void Patrol();
    void Attack();
}
