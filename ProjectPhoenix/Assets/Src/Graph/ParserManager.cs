using UnityEngine;
using System.Collections;

public class ParserManager : MonoBehaviour
{
    void Start()
    {
        GraphParser.Instance.Parse("Graph.txt");
    }
}
