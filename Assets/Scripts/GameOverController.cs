using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load when restarting.")]
    public string sceneToLoad = "Grid_scene";
    [Tooltip("Seconds before restarting automatically. 0 = never.")]
    public float autoRestartAfter = 8f;

    void Start()
    {
        if (autoRestartAfter > 0f) Invoke(nameof(Restart), autoRestartAfter);
    }

    public void Restart() => SceneManager.LoadScene(sceneToLoad);
}