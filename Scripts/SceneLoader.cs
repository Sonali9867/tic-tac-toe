using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{    
    public static void LoadMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public static void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
