using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DRAIVCustomImportTool : AssetPostprocessor
{

    void OnPreprocessModel()
    {
        if (DRAIVCustomImportMGR.ImportMeshesWithCustomSettings)
        {
            ModelImporter importedModel = assetImporter as ModelImporter;
            
            importedModel.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        }
    }
}
