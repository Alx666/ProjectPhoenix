using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DRAIVCustomImportTool : AssetPostprocessor
{

    private ModelImporter importedModel;

    public bool ApplyCustomImportSettings { get; set; }

    void OnPreProcessModel()
    {
        DRAIVCustomImportWindow Wnd = EditorWindow.GetWindow<DRAIVCustomImportWindow>();
        if (ApplyCustomImportSettings)
        {
            importedModel.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        }
    }
}
