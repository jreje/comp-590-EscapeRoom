using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hides the memory grid inside the chest, then makes it rise out when Reveal() is called
/// (e.g. when the chest is unlocked/opened).
///
/// Setup:
///   GridReveal (empty, this script)   ← place where the grid should END UP (above the chest)
///     └ MemoryGrid (MemoryGrid script) ← set INACTIVE in the Inspector (uncheck the box by its name)
/// Then call GridReveal.Reveal() from the chest's "opened" event.
/// </summary>
public class GridReveal : MonoBehaviour
{
    [Tooltip("The MemoryGrid object (child). Leave it inactive in the scene; it's turned on when revealed.")]
    public GameObject grid;

    [Header("Animation")]
    [Tooltip("How far below its final spot the grid starts (inside the chest), in meters.")]
    public float riseDistance = 0.5f;
    public float riseDuration = 1.5f;
    [Tooltip("Seconds to wait after Reveal() before rising (e.g. let the lid open first).")]
    public float delay = 0.5f;
    [Tooltip("Grow from small to full size while rising.")]
    public bool scaleUp = true;

    [Header("Optional effects")]
    public ParticleSystem revealParticles;   // e.g. sparkles/dust coming out of the chest
    public AudioSource revealSound;          // e.g. magical whoosh

    [Header("Events")]
    public UnityEvent onRevealed;            // fires when the grid finishes rising

    public bool IsRevealed { get; private set; }

    Vector3 finalPosition;
    Vector3 finalScale;

    void Awake()
    {
        finalPosition = transform.localPosition;
        finalScale = transform.localScale;
        if (grid != null) grid.SetActive(false);
    }

    /// <summary>Call this when the chest opens.</summary>
    [ContextMenu("Reveal (test)")]
    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;
        StartCoroutine(RevealRoutine());
    }

    IEnumerator RevealRoutine()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // "Down" in world space, converted to the parent's local space (works even if the parent is rotated).
        Vector3 localDown = transform.parent != null
            ? transform.parent.InverseTransformDirection(Vector3.down)
            : Vector3.down;
        Vector3 startPosition = finalPosition + localDown * riseDistance;

        transform.localPosition = startPosition;
        if (scaleUp) transform.localScale = finalScale * 0.1f;
        if (grid != null) grid.SetActive(true);   // MemoryGrid builds its tiles now

        if (revealParticles != null) revealParticles.Play();
        if (revealSound != null) revealSound.Play();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(riseDuration, 0.01f);
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(startPosition, finalPosition, s);
            if (scaleUp) transform.localScale = Vector3.Lerp(finalScale * 0.1f, finalScale, s);
            yield return null;
        }

        transform.localPosition = finalPosition;
        transform.localScale = finalScale;
        onRevealed?.Invoke();
    }
}
