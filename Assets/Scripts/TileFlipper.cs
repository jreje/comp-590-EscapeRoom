using System.Collections;
using UnityEngine;


[RequireComponent(typeof(GridTile))]
public class TileFlipper : MonoBehaviour
{
    public float flipDuration = 0.3f;

    public GridTile Tile { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; set; }
    public bool IsAnimating { get; private set; }

    float angle;

    void Awake()
{
    Tile = GetComponent<GridTile>();
}

    // Called when the player selects the tile
    public void OnSelected()
    {
        var controller = Tile.Grid != null ? Tile.Grid.GetComponent<MemoryGameController>() : null;
        if (controller != null) controller.HandleSelect(this);
    }

    public void Flip(bool faceUp)
    {
        IsFaceUp = faceUp;
        if (faceUp && Tile.audioSource != null && Tile.Sound != null) Tile.audioSource.PlayOneShot(Tile.Sound);
        StopAllCoroutines();
        StartCoroutine(Animate(faceUp ? 180f : 0f));
    }

    IEnumerator Animate(float target)
    {
        IsAnimating = true;
        float start = angle, t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(flipDuration, 0.01f);
            angle = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            Tile.visual.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }
        angle = target;
        Tile.visual.localRotation = Quaternion.Euler(0f, angle, 0f);
        IsAnimating = false;
    }

    [ContextMenu("Test Select")] void TestSelect() => OnSelected();
}