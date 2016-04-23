using UnityEngine;
using System.Collections;
using Graph;
using System.IO;
using System;
using System.Collections.Generic;

public class GraphParser : MonoBehaviour
{
    public static GraphParser Instance { get; private set; }

    static GraphParser()
    {
        if (Instance == null)
        {
            Instance = new GraphParser();
        }
    }

    public Graph<POI> Parse(string fileName)
    {
        Graph<POI> m_hGraph = new Graph<POI>();

        List<Node<POI>> m_hList = new List<Node<POI>>();

        using (StreamReader hSr = new StreamReader(fileName))
        {
            string m_sLine;
            while ((m_sLine = hSr.ReadLine()) != null)
            {
                //Creazione nodo + poi
                if (m_sLine.StartsWith("n"))
                {
                    string[] m_hS = m_sLine.Split(new string[] { "/" }, System.StringSplitOptions.None);

                    float x = float.Parse(m_hS[1].Split(',')[0]);
                    float y = float.Parse(m_hS[1].Split(',')[1]);
                    float z = float.Parse(m_hS[1].Split(',')[2]);

                    NodeType type = (NodeType)Enum.Parse(typeof(NodeType), m_hS[2]);

                    POI m_hP = new POI(new Vector3(x, y, z), type);

                    Node<POI> m_hNode = new Node<POI>(m_hP, int.Parse(m_hS[3]));

                    m_hList.Add(m_hNode);

                    
                }
                //Creazione link
                else if (m_sLine.StartsWith("l"))
                {
                    string[] m_hS = m_sLine.Split(new string[] { "/" }, System.StringSplitOptions.None);

                    m_hList[int.Parse(m_hS[1])].Link(m_hList[int.Parse(m_hS[2])], float.Parse(m_hS[3]));
                }
            }
            m_hList.ForEach(hN => m_hGraph.Add(hN));
        }

        return m_hGraph;
    }
}

