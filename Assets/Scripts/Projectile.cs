using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 direction, float speed, int bulletDamage)
    {
        damage = bulletDamage;

        // Flip the bullet sprite based on its direction.
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0;

        // Move the bullet in the correct direction.
        rb.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        if (other.GetComponentInParent<Flower>() != null)
            return;

        Pot pot = other.GetComponentInParent<Pot>();

        if (pot != null)
        {
            pot.BreakPot();
            Destroy(gameObject);
            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        FlyEnemy flyEnemy = other.GetComponentInParent<FlyEnemy>();

        if (flyEnemy != null)
        {
            flyEnemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
