using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GraphTemp : MonoBehaviour
{
    public List<Node> Nodes = new List<Node>();

	public void ResetGraph()
	{
		Nodes.Clear();
	}
}


[Serializable]
public class Node : ScriptableObject
{
    private static int s_iIndex;
	private int m_iIndex;
    public string Label;

    public string SetNodeId()
    {
		m_iIndex = s_iIndex++;
        return (m_iIndex).ToString();        
    }

	public string GetID()
	{
		return m_iIndex.ToString() ;
	}

	public static void ResetNodeId()
	{
		s_iIndex = 0;
	}


    public Vector3 Position { get; set; }
    public List<Node> Successors = new List<Node>();
	public NodeType nodetype;




    public bool FoldOutOpen { get; set; }
}

public enum NodeType { 
	Road,
	Building,
	Turret,
	Base
}
