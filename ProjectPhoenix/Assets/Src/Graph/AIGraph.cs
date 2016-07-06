using Graph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEditor;

public class AIGraph : MonoBehaviour, IEnumerable<POI>, ISerializationCallbackReceiver
{
    //Serializzare questo
    public Graph<POI> m_hGraph;

    [SerializeField, HideInInspector]
    private byte[] m_hSaveData;

    
    public byte[] GetSaveData()
    {
        return m_hSaveData;
    }

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

    public void Save()
    {
        ScriptableObject.CreateInstance<SaveData>();
        SaveData saveData = new SaveData(m_hSaveData);
        AssetDatabase.CreateAsset(saveData, "Assets/AIGraph.asset");
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

    public bool IsEmpty
    {
        get { return m_hGraph == null; }
    }

    public void Initialize()
    {
        m_hGraph = new Graph<POI>();
        POI.Counter = m_hGraph.Nodes.Max(hP => hP.Id);
    }

   

    #endregion

}

public class SaveData : ScriptableObject
{
    byte[] m_hSaveData;

    public SaveData(byte[] data)
    {
        m_hSaveData = data;
    }

    public Graph<POI> GetGraph()
    {
        return Graph<POI>.FromBinary<POI>(m_hSaveData);
    }
}