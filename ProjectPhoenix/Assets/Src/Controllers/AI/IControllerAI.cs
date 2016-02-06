using UnityEngine;
using System.Collections;

internal interface IControllerAI
{
    GameObject Target { get; set; }
    void Idle();

    void Patrol();

    void Attack();

}
