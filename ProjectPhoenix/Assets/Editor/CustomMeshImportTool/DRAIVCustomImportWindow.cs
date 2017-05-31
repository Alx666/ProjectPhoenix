using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DRAIVCustomImportWindow : EditorWindow
{
    public DRAIVImportConfiguration Preset;


    private static bool CustomConfigurationToggle = false;

    [MenuItem("DRAIVTools/CustomAssetImporter")]
    private static void OpenWindow()
    {
        DRAIVCustomImportWindow Wnd = EditorWindow.GetWindow<DRAIVCustomImportWindow>();

    }

    private void OnGUI()
    {
        CustomConfigurationToggle = GUILayout.Toggle(CustomConfigurationToggle, "ApplyCustomMeshesImportConfiguration");
        if (CustomConfigurationToggle)
        {
            Preset = (DRAIVImportConfiguration)EditorGUILayout.ObjectField("MeshesImportConfiguration", Preset, typeof(DRAIVImportConfiguration), false);
            DRAIVCustomImportMGR.ImportMeshesWithCustomSettings = true;
        }
        else
        {
            DRAIVCustomImportMGR.ImportMeshesWithCustomSettings = false;
        }
    }
}