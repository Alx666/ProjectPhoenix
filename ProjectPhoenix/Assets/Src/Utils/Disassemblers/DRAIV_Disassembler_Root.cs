using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DRAIV_Disassembler_Root : MonoBehaviour
{
    public ArmorType Armor = ArmorType.Light;
    public float Mass = 1;

    public void Start()
    {
        List<GameObject> children = this.gameObject.GetComponentsInChildren<Transform>().Select(hT => hT.gameObject).Where(GO => GO != this.gameObject).ToList();
        children.ForEach(hGO =>
        {
            DRAIV_Disassembler disassembler = hGO.AddComponent<DRAIV_Disassembler>();
            disassembler.Armor = Armor;
            disassembler.SetMass(Mass);
        });
    }
}