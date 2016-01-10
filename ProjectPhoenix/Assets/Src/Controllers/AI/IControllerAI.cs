using UnityEngine;
using System.Collections;

internal interface IControllerAI
{
    GameObject target { get; set; }
    void Move();

}
