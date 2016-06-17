using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class VehicleSelection : MonoBehaviour
{
    public List<GameObject> Vehicles;
    public Transform SpawnPosition;
    public Transform CurrentPosition;
    public Transform PreviousPosition;
    public Transform NextPosition;
    public Text CarNameText;

    private LinkedListNode<GameObject>  m_hCurrent;
    private LinkedList<GameObject>      m_hList;
    private Dictionary<GameObject, string> m_hCarNames;
    private bool m_bEnable;
    private const float m_fTweenTime = 0.5f;
    private const float m_fRotationSpeed = 20.0f;

    void Awake()
    {
        m_hList = new LinkedList<GameObject>();
        m_hCarNames = new Dictionary<GameObject, string>();
        Vehicles.ForEach(hV =>
        {
            GameObject hNew = GameObject.Instantiate(hV, SpawnPosition.position, Quaternion.identity) as GameObject;
            //hNew.GetComponent<VehiclePrefabMGR>().VehiclePrefab = hV;
            m_hList.AddLast(hNew);
            string m_sVname = hNew.GetComponent<VehiclePrefabMGR>().VehicleName;
            if (m_sVname != string.Empty)
            {
                m_hCarNames.Add(hNew, m_sVname);
            }
            else
            {
                m_hCarNames.Add(hNew, hNew.ToString());
            }
        });
        
    }

	// Use this for initialization
	void Start ()
    {
        m_bEnable = true;
        m_hCurrent = m_hList.First;
        m_hCurrent.Value.SetActive(true);
        m_hCurrent.Value.transform.position = CurrentPosition.position;
        m_hCurrent.Value.transform.rotation = CurrentPosition.rotation;
        CarNameText.text = m_hCarNames[m_hCurrent.Value];
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_bEnable)
        {
            m_hCurrent.Value.transform.Rotate(Vector3.up, -m_fRotationSpeed * Time.deltaTime);
        }
	}

    public void OnButtonStart()
    {
        LobbyManager.Instance.GamePrefab = m_hCurrent.Value.GetComponent<VehiclePrefabMGR>().VehiclePrefab;
    }

    public void OnButtonNext()
    {
        if (!m_bEnable)
            return;

        m_hCurrent.NextOrFirst().Value.transform.position = NextPosition.position;
        m_hCurrent.NextOrFirst().Value.transform.rotation = NextPosition.rotation;
        m_hCurrent.NextOrFirst().Value.SetActive(true);
        LeanTween.move(m_hCurrent.NextOrFirst().Value, CurrentPosition.position, m_fTweenTime).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(m_hCurrent.NextOrFirst().Value, CurrentPosition.rotation.eulerAngles, m_fTweenTime).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(m_hCurrent.Value, PreviousPosition.position, m_fTweenTime).setOnComplete(() => m_bEnable = true).setEase(LeanTweenType.easeInOutQuad);
        m_hCurrent = m_hCurrent.NextOrFirst();
        CarNameText.text = m_hCarNames[m_hCurrent.Value];
        m_bEnable = false;
    }

    public void OnButtonPrevious()
    {
        if (!m_bEnable)
            return;
  
        m_hCurrent.PreviousOrLast().Value.transform.position = PreviousPosition.position;
        m_hCurrent.PreviousOrLast().Value.transform.rotation = PreviousPosition.rotation;
        m_hCurrent.PreviousOrLast().Value.SetActive(true);
        LeanTween.move(m_hCurrent.PreviousOrLast().Value, CurrentPosition.position, m_fTweenTime).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(m_hCurrent.PreviousOrLast().Value, CurrentPosition.rotation.eulerAngles, m_fTweenTime).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(m_hCurrent.Value, NextPosition.position, m_fTweenTime).setOnComplete(() => m_bEnable = true).setEase(LeanTweenType.easeInOutQuad);
        m_hCurrent = m_hCurrent.PreviousOrLast();
        CarNameText.text = m_hCarNames[m_hCurrent.Value];
        m_bEnable = false;
    }

}

internal static class CircularLinkedListExtensions
{
    public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> hCurrent)
    {
        if (hCurrent.Next == null)
        {
            return hCurrent.List.First;
        }
        else
        {
            return hCurrent.Next;
        }
    }

    public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> hCurrent)
    {
        if (hCurrent.Previous == null)
        {
            return hCurrent.List.Last;
        }
        else
        {
            return hCurrent.Previous;
        }
    }
} 
