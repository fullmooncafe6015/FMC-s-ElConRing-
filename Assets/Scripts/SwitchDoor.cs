using UnityEngine;

public class SwitchDoor : MonoBehaviour
{
    [SerializeField] private int switchId;
    [SerializeField] private Door door;

    private bool activated = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int id, Door targetDoor)
    {
        switchId = id;
        door = targetDoor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        if (door == null)
        {
            Debug.LogError("Door reference is missing!");
            return;
        }

        bool success = door.ActivateSwitch(switchId);

        if (!success)
        {
            Debug.Log("Wrong order! Activate the correct switch.");
            return;
        }

        activated = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.green;

        Debug.Log("Switch " + switchId + " activated!");
    }
}
