using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DropFlower : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Prevent this dropped Flower from being collected.
        Flower flower = GetComponent<Flower>();

        if (flower != null)
            flower.enabled = false;

        // Make the collider physical so it can bounce on the ground.
        Collider2D[] dropColliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D dropCollider in dropColliders)
        {
            dropCollider.isTrigger = false;
        }
    }

    public void Launch(PlayerController player, Vector2 velocity)
    {
        if (player != null)
        {
            Collider2D[] dropColliders =
                GetComponentsInChildren<Collider2D>();

            Collider2D[] playerColliders =
                player.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D dropCollider in dropColliders)
            {
                foreach (Collider2D playerCollider in playerColliders)
                {
                    Physics2D.IgnoreCollision(
                        dropCollider,
                        playerCollider
                    );
                }
            }
        }

        rb.linearVelocity = velocity;

        Destroy(gameObject, lifetime);
    }
}
