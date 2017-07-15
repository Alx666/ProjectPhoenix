using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashToMenu : MonoBehaviour
{
    private void Awake()
    {
        Cursor.visible = false;    
    }

    public void ToLobby()
    {
        SceneManager.LoadScene("DRAIV_Vehicle_Selection_Heavy");
    }
}
