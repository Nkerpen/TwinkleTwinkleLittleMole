using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void NovoJogo()
    {
        SceneManager.LoadScene("F-FogoScene");
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }
}

