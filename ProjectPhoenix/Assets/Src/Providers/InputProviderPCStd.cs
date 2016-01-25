using UnityEngine;
using System.Collections;

[RequireComponent(typeof(IControllerPlayer))]
internal class InputProviderPCStd : MonoBehaviour
{
    [Header("Configurable Keys")]
    public KeyCode Forward  = KeyCode.W;
    public KeyCode Backward = KeyCode.S;
    public KeyCode Right    = KeyCode.D;
    public KeyCode Left     = KeyCode.A;
    public KeyCode PanRight = KeyCode.E;
    public KeyCode PanLeft  = KeyCode.Q;

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
        if (Input.GetKeyDown(Up))
            m_hTarget.BeginUp();
        if (Input.GetKeyUp(Up))
            m_hTarget.EndUp();

        if (Input.GetKeyDown(Down))
            m_hTarget.BeginDown();
        if (Input.GetKeyUp(Down))
            m_hTarget.EndDown();

        //MOVING
        if (Input.GetKeyDown(Forward))
            m_hTarget.BeginForward();
        if (Input.GetKeyUp(Forward))
            m_hTarget.EndForward();

        if (Input.GetKeyDown(Backward))
            m_hTarget.BeginBackward();
        if (Input.GetKeyUp(Backward))
            m_hTarget.EndBackward();

        //TURNING
        if (Input.GetKeyDown(Left))
            m_hTarget.BeginTurnLeft();
        if (Input.GetKeyUp(Left))
            m_hTarget.EndTurnLeft();

        if (Input.GetKeyDown(Right))
            m_hTarget.BeginTurnRight();
        if (Input.GetKeyUp(Right))
            m_hTarget.EndTurnRight();

        //PANNING
        if (Input.GetKeyDown(PanLeft))
            m_hTarget.BeginPanLeft();
        if (Input.GetKeyUp(PanLeft))
            m_hTarget.EndPanLeft();

        if (Input.GetKeyDown(PanRight))
            m_hTarget.BeginPanRight();
        if (Input.GetKeyUp(PanRight))
            m_hTarget.EndPanRight();

        //MOUSEPOSITION
        m_hTarget.MousePosition(Input.mousePosition);
    }
}
