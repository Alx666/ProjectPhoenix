using UnityEngine;
using System.Collections;
using Graph;
using System;
using System.Collections.Generic;

public class AIGraph : MonoBehaviour, IEnumerable<POI>
{
    public Graph<POI> m_hNodes;

   
    public void Add(POI hNode)
    {
        m_hNodes.Add(hNode);
    }

    public void Remove(int iId)
    {
        m_hNodes.Remove(iId);
    }

    public void Link(POI hA, POI hB)
    {
        float fDist = Vector3.Distance(hA.Position, hB.Position);
        hA.Link(hB, fDist);
        hB.Link(hA, fDist);
    }

    public IEnumerator<POI> GetEnumerator()
    {
        return m_hNodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_hNodes.GetEnumerator();
    }

    public POI this[int iIndex]
    {
        get
        {
            return m_hNodes[iIndex];
        }
    }
}
