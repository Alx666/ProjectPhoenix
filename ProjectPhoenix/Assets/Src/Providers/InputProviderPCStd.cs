using UnityEngine;
using System.Collections;

[RequireComponent(typeof(IControllerPlayer))]
internal class InputProviderPCStd : MonoBehaviour
{
    [Header("Configurable Keys")]
    public KeyCode Forward = KeyCode.W;
    public KeyCode Backward = KeyCode.S;
    public KeyCode Right = KeyCode.D;
    public KeyCode Left = KeyCode.A;
    public KeyCode PanRight = KeyCode.E;
    public KeyCode PanLeft = KeyCode.Q;

    public KeyCode Up = KeyCode.LeftShift;
    public KeyCode Down = KeyCode.Space;
    public KeyCode Fire = KeyCode.Mouse0;

    private IControllerPlayer m_hTarget;

    void Awake()
    {
        m_hTarget = this.gameObject.GetComponent<IControllerPlayer>();
    }


    void Update()
    {
        //LIFTING
        if (Input.GetKey(Up))
            m_hTarget.BeginUp();
        else if (Input.GetKey(Down))
            m_hTarget.BeginDown();

        if (Input.GetKeyUp(Up))
            m_hTarget.EndUp();
        if (Input.GetKeyUp(Down))
            m_hTarget.EndDown();


        //MOVING
        if (Input.GetKey(Forward))
            m_hTarget.BeginForward();
        else if (Input.GetKey(Backward))
            m_hTarget.BeginBackward();

        if (Input.GetKeyUp(Forward))
            m_hTarget.EndForward();
        if (Input.GetKeyUp(Backward))
            m_hTarget.EndBackward();


        //TURNING
        if (Input.GetKey(Left))
            m_hTarget.BeginTurnLeft();
        else if (Input.GetKey(Right))
            m_hTarget.BeginTurnRight();

        if (Input.GetKeyUp(Left))
            m_hTarget.EndTurnLeft();
        if (Input.GetKeyUp(Right))
            m_hTarget.EndTurnRight();


        //PANNING
        if (Input.GetKey(PanLeft))
            m_hTarget.BeginPanLeft();
        else if (Input.GetKey(PanRight))
            m_hTarget.BeginPanRight();

        if (Input.GetKeyUp(PanLeft))
            m_hTarget.EndPanLeft();
        if (Input.GetKeyUp(PanRight))
            m_hTarget.EndPanRight();

        //MOUSEPOSITION
        m_hTarget.MousePosition(Input.mousePosition);
    }
}
