using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Graph;
using System.IO;

[CustomEditor(typeof(GraphTemp))]
public class GraphEditor : Editor
{
    private GraphTemp m_hTarget;
    private string Label0 = string.Empty;
    private string Label1 = string.Empty;
	private string Label2 = string.Empty;

    void OnEnable()
    {
        m_hTarget = this.target as GraphTemp; //get a reference to the target component
    }

    //This is method is called by the unity editor when it need to draw the inspector
    public override void OnInspectorGUI()
    {
        //The inherited OnInspectorGUI draws inspector representations of public variables.
        //we don't need it
        //base.OnInspectorGUI(); 

        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
        EditorGUILayout.Separator();


		for ( int i = 0; i < m_hTarget.Nodes.Count; i++)
        {
			Node hNode = m_hTarget.Nodes[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "N° ", hNode.GetID() );
			hNode.nodetype = (NodeType)EditorGUILayout.EnumPopup( hNode.nodetype );
			EditorGUILayout.EndHorizontal();
            hNode.Position = EditorGUILayout.Vector3Field("", hNode.Position);
        }
        EditorGUILayout.Separator();
		EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            Node hNew = ScriptableObject.CreateInstance(typeof(Node)) as Node;

            RaycastHit vHit;
            Ray vRay = new Ray(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward);
            if (Physics.Raycast(vRay, out vHit))
            {
                hNew.Position = vHit.point;
            }

            hNew.Label = hNew.SetNodeId();
            m_hTarget.Nodes.Add(hNew);
        }

		if ( GUILayout.Button( "Add & Connect" ) )
		{
			Node hNew = ScriptableObject.CreateInstance( typeof( Node ) ) as Node;

			RaycastHit vHit;
			Ray vRay = new Ray( SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward );
			if ( Physics.Raycast( vRay, out vHit ) )
			{
				hNew.Position = vHit.point;
			}

			hNew.Label = hNew.SetNodeId();
			m_hTarget.Nodes.Add( hNew );

			if ( m_hTarget.Nodes.Count>1 )
			{
				Node a = m_hTarget.Nodes[m_hTarget.Nodes.Count - 2];
				Node b = m_hTarget.Nodes.Last();
				a.Successors.Add( b );
				b.Successors.Add( a );
			}
		}


        if (GUILayout.Button("Connect"))
        {
            IEnumerable<Node> hNodes = m_hTarget.Nodes.Where(x => x.Label == Label0 || x.Label == Label1);

            if (hNodes.Count() == 2)
            {
                Node a = hNodes.First();
                Node b = hNodes.Last();
                a.Successors.Add(b);
                b.Successors.Add(a);
            }



            Label0 = string.Empty;
            Label1 = string.Empty;
        }

        Label0 = GUILayout.TextField(Label0);
        Label1 = GUILayout.TextField(Label1);

        EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.BeginHorizontal();

		if ( GUILayout.Button("Remove" ))
		{
			Node hNode = m_hTarget.Nodes.Where( x => x.Label == Label2 ).First();
			hNode.Successors.ForEach( hS => hS.Successors.Remove( hNode ) );
			m_hTarget.Nodes.Remove( hNode );
			Label2 = string.Empty;
		}
		Label2 = GUILayout.TextField( Label2 );

		EditorGUILayout.Separator();

		if ( GUILayout.Button("Reset Graph" ))
		{
			m_hTarget.ResetGraph();
			Node.ResetNodeId();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

        if (GUILayout.Button("Build"))
        {
            Graph<POI> graph = new Graph<POI>();
            m_hTarget.Nodes.ForEach(hN =>
           {
               POI p = new POI(hN.Position, hN.nodetype);
               graph.Add(new Node<POI>(p, int.Parse(hN.GetID())));
           });

            m_hTarget.Nodes.ForEach(hN =>
            {
                hN.Successors.ForEach(hS =>
                {
                    graph.m_hNodes[int.Parse(hN.GetID())].Link(graph.m_hNodes[int.Parse(hS.GetID())], Vector3.Distance(hS.Position, hN.Position));
                });
            });

            using(StreamWriter hS = new StreamWriter("Graph.txt"))
            {
                graph.m_hNodes.ForEach(hN =>
                {
                    hS.Write(hN.m_iId  + " => ");
                    hN.neighbours.ForEach(hNeigh =>
                    {
                        hS.Write(hNeigh.Node.m_iId + ", ");
                    });
                    hS.WriteLine();
                });
            }

        }
        SceneView.RepaintAll();
    }


    void OnSceneGUI()
    {
        for (int i = 0; i < m_hTarget.Nodes.Count; i++)
        {
            Node hCurrent = m_hTarget.Nodes[i];

            hCurrent.Position = Handles.PositionHandle(hCurrent.Position, Quaternion.identity);

            Handles.SphereCap(0, hCurrent.Position, Quaternion.identity, 0.5f);
            Handles.Label(hCurrent.Position, hCurrent.Label);

			Color m_hColor;

			for ( int k = 0; k < hCurrent.Successors.Count; k++ )
			{
				if ( hCurrent.nodetype != NodeType.Road || hCurrent.Successors[k].nodetype != NodeType.Road )
					m_hColor = Color.red;
				else
					m_hColor = Color.green;

				DrawConnection( hCurrent, hCurrent.Successors[k], 0.5f, m_hColor );
			}
        }

        this.Repaint();
    }


    private void OnGUI()
    {
        int iWidth = SceneView.lastActiveSceneView.camera.pixelWidth;
        int iHeight = SceneView.lastActiveSceneView.camera.pixelHeight;

        Rect cameraRect = new Rect(0f, 0f, iWidth, iHeight);
        Handles.DrawCamera(cameraRect, SceneView.lastActiveSceneView.camera, DrawCameraMode.Textured);        
    }

    private void DrawConnection(Node hFirst, Node hSecond, float headSize, Color color)
    {
        Vector3 vDirection      = hFirst.Position - hSecond.Position;
        Vector3 dir = vDirection.normalized;
        Vector3 vConePosition   = hSecond.Position - vDirection * -0.95f;


        Color c = Handles.color;
        Handles.color = color;
        Handles.DrawLine(hFirst.Position, hSecond.Position);
        Handles.ConeCap(0, vConePosition, Quaternion.LookRotation(dir != Vector3.zero ? dir : Vector3.right), headSize);
        Handles.color = c;
    }    

}


public class NodeGizmoDrawer
{
    //occhio a rispettare la signature
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawGizmoForMyScript(Node  hNode, GizmoType gizmoType)
    {
        RaycastHit vHit;
        Ray vRay = new Ray(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward);
        if (Physics.Raycast(vRay, out vHit))
        {
            Gizmos.DrawSphere(vHit.point, 1.0f);
        }
    }
}