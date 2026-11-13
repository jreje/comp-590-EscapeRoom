# Memory Grid Puzzle: Handoff for Flipping & Rules

**Grid built by:** Lucy · **Next steps:** Lauren

## Game rules (agreed)

- 3×3 board = **4 pairs + 1 red car tile**, all face down at the start.
- Player selects a tile → it flips. Selects a second tile:
  - **Match** → both stay face up.
  - **No match** → both stay visible ~1 s, flip back, **+1 mistake**.
- The **red car tile is locked**: selecting it does nothing until all 4 pairs are matched. Then it flips by itself → **puzzle solved** (unlocks the escape).
- **3 mistakes → puzzle failed** (team decides: reshuffle and retry, or game over).

---

## What's already done

### Files
| File | Where | Purpose |
|---|---|---|
| `Assets/Scripts/MemoryGrid.cs` | on the `MemoryGrid` object | Spawns the 3×3 grid, shuffles pairs, assigns images/sounds |
| `Assets/Scripts/GridTile.cs` | on the Tile prefab | Stores each tile's data (row, column, pair id, image, sound) |
| `Assets/Prefabs/Tile.prefab` | — | The tile spawned 9 times |

### Tile prefab structure
```
Tile          (GridTile, BoxCollider 0.2×0.2×0.03, AudioSource)
 └ Visual     ← ROTATE THIS 180° on Y to flip
     ├ Body   (thin cube, no collider)
     ├ Back   (Quad, z = -0.011)            – visible when face down
     └ Front  (Quad, z = +0.011, rot Y 180) – visible when face up
```
- The player stands on the grid's **−Z side** (yellow gizmo line on MemoryGrid points toward the player).
- Only the root `Tile` has a collider; put your interactable on the root.
- Flip by rotating **`Visual`**, not the root `Tile`.

### MemoryGrid setup (Inspector)
- **Tile Prefab:** Tile · **Rows/Columns:** 3/3 · **Spacing:** 0.25
- **Pair Faces:** 4 entries (christmas, smile, sock, toothbrush)
- **Odd Tile Face:** redcar
- Right-click the component → **Build Grid** / **Clear Grid** to preview in edit mode. It also builds automatically on Play.

---

## API you can use

### `GridTile` (on every tile)
| Member | Meaning |
|---|---|
| `visual` | Transform to rotate when flipping |
| `frontRenderer` | Front quad renderer (image already applied) |
| `audioSource` | 3D AudioSource; `clip` already set to the tile's sound (may be null) |
| `Grid` | The `MemoryGrid` that spawned this tile |
| `Row`, `Column` | Position in the grid (row 0 = top) |
| `FaceId` | Pair id. Two tiles with the same `FaceId` are a pair. `-1` = car tile |
| `IsOddTile` | `true` for the red car tile |
| `Image`, `Sound` | The tile's texture / audio clip |

### `MemoryGrid`
| Member | Meaning |
|---|---|
| `Tiles` | All 9 spawned `GridTile`s |
| `GetTile(row, col)` | Tile at a position |
| `IsMatch(a, b)` | `true` if two different, non-car tiles are a pair |
| `PairCount` | Number of pairs (4) |
| `OddTile` | The car tile |
| `BuildGrid()` | Destroys tiles and builds a new shuffled grid (use for retry) |
| `onGridBuilt` | UnityEvent fired after every build. Reset your game state here |

> ⚠️ `BuildGrid()` **destroys and respawns** all tiles. Don't keep references to old tiles after a rebuild. Any per-tile state should live in a component on the prefab (it resets automatically) or be cleared in `onGridBuilt`.

---

## To do (Lauren)

1. **Selection input.** Add an interactable to the Tile prefab root and call your select method from its event:
   - **XR Interaction Toolkit:** *XR Simple Interactable* → Interactable Events → **Select Entered** → `TileFlipper.OnSelected`
   - **Meta Interaction SDK:** Ray/Poke Interactable + *Pointable Unity Event Wrapper* → **When Select** → `TileFlipper.OnSelected`
2. **Flip animation.** Add a script to the Tile prefab that rotates `visual` 0° ↔ 180° on Y.
3. **Game rules.** A controller on the `MemoryGrid` object that:
   - ignores input while a pair is resolving, on face-up/matched tiles, and on the car tile
   - checks pairs with `grid.IsMatch(a, b)`
   - counts mistakes (fail at 3, editable in Inspector)
   - flips `grid.OddTile` when all pairs are matched, then fires a "solved" event
4. **Events for the team.** Expose UnityEvents (`onMatch`, `onMismatch`, `onSolved`, `onFailed`) so the door, timer, particles and sounds can hook in without code changes.
5. **Optional:** play `tile.audioSource` when a tile flips up (counts toward the "presence" rubric item).

---

## Reference implementation (optional, use or adapt)

**`TileFlipper.cs`** → add to the Tile prefab root
```csharp
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

    void Awake() => Tile = GetComponent<GridTile>();

    // Hook to XR Select Entered / Meta When Select
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
```

**`MemoryGameController.cs`** → add to the `MemoryGrid` object
```csharp
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
```

### Testing without a headset
Enter Play mode, select a spawned tile in the Hierarchy, then right-click **Tile Flipper** → **Test Select**.

### Notes
- 3 mistakes is tough because the first flips are guesses. Playtest and adjust `maxMistakes` in the Inspector if needed.
- If `reshuffleOnFail` is off, wire `onFailed` to the game-over logic instead.
