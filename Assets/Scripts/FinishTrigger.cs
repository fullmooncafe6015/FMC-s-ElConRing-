using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField] private HudController hudController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hudController != null)
            hudController.ShowFinishPanel();
        else
            Debug.LogError("HudController reference is missing!");
    }
}
