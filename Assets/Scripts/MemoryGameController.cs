using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MemoryGrid))]
public class MemoryGameController : MonoBehaviour
{
    public int maxMistakes = 3;
    public float mismatchRevealTime = 1f;
    [Tooltip("On fail: reshuffle and let the player retry instead of ending.")]
    public bool reshuffleOnFail = true;

    public UnityEvent onMatch, onMismatch, onSolved, onFailed;

    public int Mistakes { get; private set; }
    public bool IsSolved { get; private set; }

    MemoryGrid grid;
    TileFlipper firstPick;
    bool busy;
    int matchedPairs;

    void Awake()
    {
        grid = GetComponent<MemoryGrid>();
        grid.onGridBuilt.AddListener(ResetState);
    }

    void ResetState()
    {
        StopAllCoroutines();
        firstPick = null; busy = false; matchedPairs = 0; Mistakes = 0; IsSolved = false;
    }

    public void HandleSelect(TileFlipper t)
    {
        if (busy || IsSolved || t.IsFaceUp || t.IsMatched || t.IsAnimating) return;
        if (t.Tile.IsOddTile) return; // car is locked until all pairs are found

        t.Flip(true);
        if (firstPick == null) { firstPick = t; return; }

        StartCoroutine(Resolve(firstPick, t));
        firstPick = null;
    }

    IEnumerator Resolve(TileFlipper a, TileFlipper b)
    {
        busy = true;
        yield return new WaitForSeconds(b.flipDuration);

        if (grid.IsMatch(a.Tile, b.Tile))
        {
            a.IsMatched = b.IsMatched = true;
            matchedPairs++;
            onMatch.Invoke();

            if (matchedPairs >= grid.PairCount)
            {
                if (grid.OddTile != null) grid.OddTile.GetComponent<TileFlipper>().Flip(true); // reveal red car
                IsSolved = true;
                onSolved.Invoke();
            }
        }
        else
        {
            Mistakes++;
            onMismatch.Invoke();
            yield return new WaitForSeconds(mismatchRevealTime);
            a.Flip(false); b.Flip(false);

            if (Mistakes >= maxMistakes)
            {
                onFailed.Invoke();
                if (reshuffleOnFail)
                {
                    yield return new WaitForSeconds(a.flipDuration);
                    grid.BuildGrid(); // fires onGridBuilt → ResetState
                    yield break;
                }
            }
        }
        busy = false;
    }
}