using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] private int switchId;

    private PuzzleController puzzleController;
    private bool activated = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int id, PuzzleController controller)
    {
        switchId = id;
        puzzleController = controller;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        if (puzzleController == null)
        {
            Debug.LogError("PuzzleController reference is missing!");
            return;
        }

        bool success = puzzleController.ActivateSwitch(switchId);

        if (!success)
            return;

        activated = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.green;

        Debug.Log("Switch " + switchId + " activated!");
    }
}
