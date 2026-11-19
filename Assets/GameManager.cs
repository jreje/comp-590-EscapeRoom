using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] CanvasGroup winPanel;
    [SerializeField] float fadeDuration = 2f;
    [SerializeField] MonoBehaviour playerController; // optional: movement script to disable

    bool won;

    // Hook this to MemoryGameController's On Solved event
    public void Win()
    {
        if (won) return;
        won = true;
        StartCoroutine(ShowWinScreen());
    }

    IEnumerator ShowWinScreen()
    {
        if (playerController) playerController.enabled = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            winPanel.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return new WaitForSeconds(1.5f);
        }

        winPanel.alpha = 1f;
        winPanel.interactable = true;
        winPanel.blocksRaycasts = true;
    }

    // Hook this to the Play Again button's OnClick
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}