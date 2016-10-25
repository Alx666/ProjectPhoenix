using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class IPConfig : MonoBehaviour
{
    InputField inputField;
    public Text TextChild;
	void Start ()
    {
        inputField = GetComponent<InputField>();
        string fileText;
        try
        {
            fileText        = File.ReadAllLines(Directory.GetParent((Application.dataPath)) + "/IPConfig.txt")[0].Trim();
            TextChild.text  = fileText;
            inputField.text = fileText;
        }
        catch
        {
            throw new UnityException("IP URL not found");
        }
	}
}
