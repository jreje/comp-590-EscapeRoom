using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Builds a rows x columns grid of tiles, shuffles pairs onto them, and assigns each tile its image/sound.
/// Layout and data only — flipping, input and match rules are handled elsewhere and can use:
///   Tiles, GetTile(row, col), IsMatch(a, b), PairCount, OddTile, onGridBuilt, BuildGrid().
/// </summary>
public class MemoryGrid : MonoBehaviour
{
    [System.Serializable]
    public class TileFace
    {
        public string name;
        public Texture2D image;
        [Tooltip("Optional sound for this face.")]
        public AudioClip sound;
    }

    [Header("Layout")]
    public GridTile tilePrefab;
    [Min(1)] public int rows = 3;
    [Min(1)] public int columns = 3;
    [Tooltip("Distance between tile centers, in meters.")]
    public float spacing = 0.25f;
    public bool buildOnStart = true;

    [Header("Content")]
    [Tooltip("One entry per pair; each face goes on two tiles. Needs at least (rows*columns)/2 entries.")]
    public List<TileFace> pairFaces = new List<TileFace>();
    [Tooltip("Face for the leftover tile when rows*columns is odd (e.g. the escape car).")]
    public TileFace oddTileFace;

    [Header("Events")]
    [Tooltip("Fires after every build/rebuild. Use it to set up flipping on the new tiles.")]
    public UnityEvent onGridBuilt;

    readonly List<GridTile> tiles = new List<GridTile>();
    GridTile[,] tileGrid;

    public IReadOnlyList<GridTile> Tiles => tiles;
    public int PairCount { get; private set; }
    public GridTile OddTile { get; private set; }

    void Start()
    {
        if (buildOnStart) BuildGrid();
    }

    /// <summary>Clears any existing tiles and builds a freshly shuffled grid.</summary>
    [ContextMenu("Build Grid")]
    public void BuildGrid()
    {
        ClearGrid();

        if (tilePrefab == null)
        {
            Debug.LogError($"{name}: no tile prefab assigned.", this);
            return;
        }

        int count = rows * columns;
        PairCount = count / 2;
        if (pairFaces.Count < PairCount)
        {
            Debug.LogError($"{name}: a {rows}x{columns} grid needs {PairCount} pair faces, but only {pairFaces.Count} are assigned.", this);
            return;
        }

        // Face ids: each pair twice, plus -1 for the odd tile if the count is odd.
        var ids = new List<int>(count);
        for (int i = 0; i < PairCount; i++) { ids.Add(i); ids.Add(i); }
        if (count % 2 == 1) ids.Add(-1);
        Shuffle(ids);

        tileGrid = new GridTile[rows, columns];
        int k = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                // Centered on this object, in its local X/Y plane. Row 0 is the top.
                float x = (c - (columns - 1) / 2f) * spacing;
                float y = ((rows - 1) / 2f - r) * spacing;

                GridTile tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector3(x, y, 0f);
                tile.transform.localRotation = Quaternion.identity;
                tile.name = $"Tile_{r}_{c}";

                int id = ids[k++];
                TileFace face = id >= 0 ? pairFaces[id] : oddTileFace;
                tile.Setup(this, r, c, id, face != null ? face.image : null, face != null ? face.sound : null);

                if (tile.IsOddTile) OddTile = tile;
                tiles.Add(tile);
                tileGrid[r, c] = tile;
            }
        }

        onGridBuilt?.Invoke();
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        tiles.Clear();
        tileGrid = null;
        OddTile = null;

        // Remove every spawned tile, including ones built in edit mode for previewing.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<GridTile>() == null) continue;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    public GridTile GetTile(int row, int column)
    {
        if (tileGrid == null || row < 0 || row >= rows || column < 0 || column >= columns) return null;
        return tileGrid[row, column];
    }

    /// <summary>True if two different tiles share a face (the odd tile never matches).</summary>
    public bool IsMatch(GridTile a, GridTile b)
    {
        return a != null && b != null && a != b && !a.IsOddTile && a.FaceId == b.FaceId;
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Grid outline in the Scene view; yellow line points toward where the player stands.
    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(columns * spacing, rows * spacing, 0.02f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, Vector3.back * 0.3f);
    }
}
