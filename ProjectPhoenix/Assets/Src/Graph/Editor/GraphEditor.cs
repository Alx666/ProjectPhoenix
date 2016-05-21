using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Graph;
using System.IO;
using System.Xml.Serialization;
using System;

[CustomEditor(typeof(AIGraph))]
public class GraphEditor : Editor
{
    private AIGraph m_hTarget;
    private string Label0 = string.Empty;
    private string Label1 = string.Empty;
    private string Label2 = string.Empty;

    private const int NodeIDSpace = 30;
    private const int EnumSpace = 70;
    private const int LabelSize = 59;


    void OnEnable()
    {
        m_hTarget = this.target as AIGraph;
        if (m_hTarget != null && m_hTarget.m_hGraph == null)
            m_hTarget.m_hGraph = new Graph<POI>();
    }

    public override void OnInspectorGUI()
    {

        if (Application.isPlaying)
            return;


        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            RaycastHit vHit;
            Ray vRay = new Ray(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward);
            if (Physics.Raycast(vRay, out vHit))
            {
                m_hTarget.Add(new POI(vHit.point, POI.NodeType.Road));
            }
        }

        if (GUILayout.Button("Connect"))
        {
            try
            {


                IEnumerable<POI> hNodes = m_hTarget.Where(x => x.Id == int.Parse(Label0) || x.Id == int.Parse(Label1));

                if (hNodes.Count() == 2)
                    m_hTarget.Link(hNodes.First(), hNodes.Last());
            }
            catch (Exception)
            {
                //Just reset interface
            }

            Label0 = string.Empty;
            Label1 = string.Empty;
        }

        Label0 = GUILayout.TextField(Label0, 6, GUILayout.Width(LabelSize));
        Label1 = GUILayout.TextField(Label1, 6, GUILayout.Width(LabelSize));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add & Connect"))
        {
            RaycastHit vHit;
            Ray vRay = new Ray(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward);

            if (Physics.Raycast(vRay, out vHit))
                m_hTarget.Add(new POI(vHit.point, POI.NodeType.Road));

            if (m_hTarget.Count() > 1)
            {
                m_hTarget.Link(m_hTarget[m_hTarget.Count() - 2], m_hTarget.Last());
            }
        }

        if (GUILayout.Button("Remove"))
        {
            m_hTarget.Remove(int.Parse(Label2));
            Label2 = string.Empty;
        }

        Label2 = GUILayout.TextField(Label2, 6, GUILayout.Width(LabelSize * 2 + 4));

        EditorGUILayout.EndHorizontal();




        EditorGUILayout.Separator();


        m_hTarget.ToList().ForEach(hN =>
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("N° " + hN.Id, GUILayout.MinWidth(NodeIDSpace), GUILayout.MaxWidth(NodeIDSpace));
            hN.Type = (POI.NodeType)EditorGUILayout.EnumPopup(hN.Type, GUILayout.MinWidth(EnumSpace), GUILayout.MaxWidth(EnumSpace));
            hN.Position = EditorGUILayout.Vector3Field("", hN.Position);
            EditorGUILayout.EndHorizontal();
        });


        EditorGUILayout.EndVertical();


        SceneView.RepaintAll();
    }
    void OnSceneGUI()
    {
        if (Application.isPlaying)
        {
            return;
        }

        for (int i = 0; i < m_hTarget.Count(); i++)
        {
            POI hCurrent = m_hTarget[i];

            hCurrent.Position = Handles.PositionHandle(hCurrent.Position, Quaternion.identity);

            Handles.SphereCap(0, hCurrent.Position, Quaternion.identity, 0.5f);
            Handles.Label(hCurrent.Position, hCurrent.Id.ToString());

            Color m_hColor;

            for (int k = 0; k < hCurrent.Neighbours.Count; k++)
            {
                POI hPoi = hCurrent.Neighbours[k].Node as POI;

                if (hPoi.Type != POI.NodeType.Road || hCurrent.Type != POI.NodeType.Road)
                    m_hColor = Color.red;
                else
                    m_hColor = Color.green;

                DrawConnection(hCurrent, hPoi, 0.5f, m_hColor);
            }
        }

        this.Repaint();
    }


    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            return;
        }
        int iWidth = SceneView.lastActiveSceneView.camera.pixelWidth;
        int iHeight = SceneView.lastActiveSceneView.camera.pixelHeight;

        Rect cameraRect = new Rect(0f, 0f, iWidth, iHeight);
        Handles.DrawCamera(cameraRect, SceneView.lastActiveSceneView.camera, DrawCameraMode.Textured);
    }

    private void DrawConnection(POI hFirst, POI hSecond, float headSize, Color color)
    {
        Vector3 vDirection = hFirst.Position - hSecond.Position;
        Vector3 dir = vDirection.normalized;
        Vector3 vConePosition = hSecond.Position - vDirection * -0.95f;


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
    static void DrawGizmoForMyScript(POI hNode, GizmoType gizmoType)
    {
        RaycastHit vHit;
        Ray vRay = new Ray(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward);
        if (Physics.Raycast(vRay, out vHit))
        {
            Gizmos.DrawSphere(vHit.point, 1.0f);
        }
    }
}