using Graph;
using UnityEngine;

public class POI : Graph<POI>.Node
{
    private static int m_hCounter;

    public Vector3  Position    { get; set; }
    public NodeType Type        { get; set; }
    
    public POI(Vector3 Position, NodeType Type) : base(m_hCounter++)
    {
        this.Position = Position;
        this.Type = Type;
    }

    public enum NodeType
    {
        Road = 0,
        Base = 1,
        Turret = 2,
        Building = 3,
    }
}
