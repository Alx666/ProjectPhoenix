using Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGraph : MonoBehaviour, IEnumerable<POI>, ISerializationCallbackReceiver
{
    //Serializzare questo
    public Graph<POI> m_hGraph;

    [SerializeField, HideInInspector]
    private byte[] m_hSaveData;

    
    #region Misc

    public void Add(POI hNode)
    {
        m_hGraph.Add(hNode);        
    }

    public void Remove(int iId)
    {
        m_hGraph.Remove(iId);
    }

    public void Link(POI hA, POI hB)
    {
        float fDist = Vector3.Distance(hA.Position, hB.Position);
        hA.Link(hB, fDist);
        hB.Link(hA, fDist);
    }

    public POI this[int iIndex]
    {
        get
        {
            return m_hGraph[iIndex];
        }
    }

    #endregion

    #region IEnumerable

    public IEnumerator<POI> GetEnumerator()
    {
        return m_hGraph.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_hGraph.GetEnumerator();
    }

    #endregion

    #region ISerializationCallbackReceiver

    public void OnBeforeSerialize()
    {
        m_hSaveData = Graph<POI>.ToBinary(m_hGraph);
    }

    public void OnAfterDeserialize()
    {
        m_hGraph = Graph<POI>.FromBinary<POI>(m_hSaveData);
    }

    #endregion 

}