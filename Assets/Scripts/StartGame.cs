using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{

    public int sceneInt;

    public void StartPressed()
    {
        SceneManager.LoadScene(sceneInt);
    }

}
