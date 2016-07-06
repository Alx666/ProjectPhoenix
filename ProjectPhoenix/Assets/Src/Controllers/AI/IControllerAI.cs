using UnityEngine;
using System.Collections;
using Graph;

internal interface IControllerAI
{
    Graph<POI> Graph { get; set; }
    GameObject Target { get; set; }
    void Idle();

    void Patrol();

    void Attack();



}
