using UnityEngine;

/// <summary>
/// Data holder for one tile in the grid. MemoryGrid fills this in when it spawns the tile.
/// No flipping or input logic lives here — that goes in a separate script on the same prefab
/// (e.g. TileFlipper.cs), which can read everything below.
///
/// Prefab layout:
///   Tile (GridTile, BoxCollider, AudioSource)
///     └ Visual            ← rotate this 180° on Y to flip
///         ├ Body          (thin cube)
///         ├ Back  (Quad)  pos (0,0,-0.011) rot (0,0,0)    – visible face down
///         └ Front (Quad)  pos (0,0, 0.011) rot (0,180,0)  – visible face up
/// The player stands on the grid's -Z side.
/// </summary>
public class GridTile : MonoBehaviour
{
    [Header("References (set on the prefab)")]
    [Tooltip("Child that should rotate when the tile flips.")]
    public Transform visual;
    [Tooltip("Renderer on the Front quad; gets this tile's image.")]
    public Renderer frontRenderer;
    [Tooltip("Optional. For playing this tile's sound.")]
    public AudioSource audioSource;

    // Filled in by MemoryGrid at spawn time
    public MemoryGrid Grid { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    /// <summary>Tiles with the same FaceId are a pair. -1 = the odd tile (escape car).</summary>
    public int FaceId { get; private set; } = -1;
    public bool IsOddTile => FaceId < 0;
    public Texture2D Image { get; private set; }
    public AudioClip Sound { get; private set; }

    static readonly int BaseMapId = Shader.PropertyToID("_BaseMap"); // URP Lit/Unlit
    static readonly int MainTexId = Shader.PropertyToID("_MainTex"); // fallback
    MaterialPropertyBlock block;

    public void Setup(MemoryGrid grid, int row, int column, int faceId, Texture2D image, AudioClip sound)
    {
        Grid = grid;
        Row = row;
        Column = column;
        FaceId = faceId;
        Image = image;
        Sound = sound;

        if (frontRenderer != null && image != null)
        {
            if (block == null) block = new MaterialPropertyBlock();
            frontRenderer.GetPropertyBlock(block);
            block.SetTexture(BaseMapId, image);
            block.SetTexture(MainTexId, image);
            frontRenderer.SetPropertyBlock(block);
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound from the tile's position
            audioSource.clip = sound;
        }
    }
}
