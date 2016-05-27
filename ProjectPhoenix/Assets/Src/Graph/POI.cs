using Graph;
using System;
using UnityEngine;

[Serializable]
public class POI : Graph<POI>.Node
{
    public static int Counter { get; set; }

    public Vector3  Position    { get; set; }
    public NodeType Type        { get; set; }


    public POI()
    {
    }
    
    public POI(Vector3 Position, NodeType Type) : base(Counter++)
    {
        this.Position = Position;
        this.Type = Type;
    }

    [Serializable]
    public enum NodeType
    {
        Road = 0,
        Base = 1,
        Turret = 2,
        Building = 3,
    }


}
