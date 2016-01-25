using UnityEngine;
using System.Collections;

internal interface IControllerPlayer
{
    void BeginForward();
    void EndForward();
    void BeginBackward();
    void EndBackward();
    void BeginTurnRight();
    void EndTurnRight();
    void BeginTurnLeft();
    void EndTurnLeft();
    void BeginFire();
    void EndFire();

    void MousePosition(Vector3 vMousePosition);

    void BeginUp();
    void EndUp();
    void BeginDown();
    void EndDown();
    void BeginPanLeft();
    void EndPanLeft();
    void BeginPanRight();
    void EndPanRight();
}
