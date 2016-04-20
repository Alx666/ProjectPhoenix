using UnityEngine;
using System.Collections;

public class POI
{
    public Vector3 Position { get; set; }
    public NodeType Type { get; set; }

    public POI(Vector3 Position, NodeType Type)
    {
        this.Position = Position;
        this.Type = Type;
    }
}
