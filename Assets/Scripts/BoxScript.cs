using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BoxScript : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The lid child object. Its own pivot is already at the hinge.")]
    public Transform lid;

    [Header("Settings")]
    public string keyTag = "PuzzleKey";
    public float openAngle = 110f;  
    public float openDuration = 1.0f;
    public bool consumeKey = false;      // hide the key after use

    [Header("Feedback")]
    public AudioSource openSound;        // optional
    public UnityEvent onOpened;          // hook up puzzle logic / particles here

    private bool isOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;

        // Check the collider itself and its Rigidbody root, since the tag may be on the parent
        bool isKey = other.tag == keyTag ||
             (other.attachedRigidbody != null && other.attachedRigidbody.tag == keyTag);

        Debug.Log($"isKey? {isKey}");

        if (!isKey) return;

        Open();

        if (consumeKey)
        {
            GameObject key = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;
            key.SetActive(false);
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        if (openSound != null) openSound.Play();
        StartCoroutine(OpenLid());
    }

    private IEnumerator OpenLid()
    {
        Quaternion start = lid.localRotation;
        // Rotate around the lid's local X axis
        Quaternion end = start * Quaternion.AngleAxis(openAngle, Vector3.right);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            // SmoothStep gives an ease-in/ease-out feel
            lid.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        lid.localRotation = end;
        onOpened.Invoke();
    }
}