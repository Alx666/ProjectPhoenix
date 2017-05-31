using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "CustomImportSettings", menuName = "CustomImportSettings")]
public class DRAIVImportConfiguration : ScriptableObject
{
    //TODO creare tutti i campi più usati per settare l'import delle meshes
    public ModelImporterMaterialName MaterialName;
}
