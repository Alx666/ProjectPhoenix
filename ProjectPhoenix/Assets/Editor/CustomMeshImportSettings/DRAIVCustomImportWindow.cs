using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DRAIVCustomImportWindow : EditorWindow
{
    public DRAIVImportConfiguration Preset;

    private DRAIVCustomImportTool ImportTool;

    private static bool CustomConfigurationToggle = false;

    [MenuItem("DRAIVTools/CustomCarMeshImport")]
    private static void OpenWindow()
    {
        DRAIVCustomImportWindow Wnd = EditorWindow.GetWindow<DRAIVCustomImportWindow>();

    }

    private void OnGUI()
    {
        CustomConfigurationToggle = GUILayout.Toggle(CustomConfigurationToggle, "ApplyCustomImpoortConfiguration");
        if (CustomConfigurationToggle)
        {
            Preset = (DRAIVImportConfiguration)EditorGUILayout.ObjectField("ImportConfiguration", Preset, typeof(DRAIVImportConfiguration), false);
            ImportTool.ApplyCustomImportSettings = true;
        }
        else
            ImportTool.ApplyCustomImportSettings = false;
    }
}