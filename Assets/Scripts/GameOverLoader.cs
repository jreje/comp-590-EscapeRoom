using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverLoader : MonoBehaviour
{
    public string gameOverScene = "GameOver";
    public void Load() => SceneManager.LoadScene(gameOverScene);
}