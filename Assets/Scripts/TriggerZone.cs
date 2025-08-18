using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public bool oneShot = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;

    public string triggerTag = "Player";

    public UnityEvent onEnter;
    public UnityEvent onExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyEntered) return;

        if (!string.IsNullOrEmpty(triggerTag) && !collision.CompareTag(triggerTag)) return;

        onEnter?.Invoke();
        

        if (oneShot) alreadyEntered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (alreadyExited) return;

        if (!string.IsNullOrEmpty(triggerTag) && !collision.CompareTag(triggerTag)) return;

        onExit?.Invoke();

        if (oneShot) alreadyExited = true;
    }
}

