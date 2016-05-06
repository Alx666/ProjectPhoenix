using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

public class POI
{
    public Vector3 Position { get; set; }

    public NodeType Type { get; set; }
    
    public POI()
    {

    }
    public POI(Vector3 Position, NodeType Type)
    {
        this.Position = Position;
        this.Type = Type;
    }
}
