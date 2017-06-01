using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
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
#endif

