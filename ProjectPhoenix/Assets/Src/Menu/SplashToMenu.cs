using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashToMenu : MonoBehaviour
{

    public void ToLobby()
    {
        SceneManager.LoadScene("DRAIV_Vehicle_Selection");
    }
}
