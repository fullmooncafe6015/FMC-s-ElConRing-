using UnityEngine;

// Goes on the flying enemy.
public class FlyEnemy : MonoBehaviour
{
    [SerializeField] private FlyEnemyStats traits;

    [Header("Circular Movement")]
    [SerializeField] private float circleRadius = 2f;
    [SerializeField] private float circleSpeed = 2f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;

    [Header("Death Effect")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip deathClip;

    [Header("Drop Item")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private float launchForce = 3f;
    [SerializeField] private float sideForce = 1.5f;
    [SerializeField] private float dropLifetime = 8f;

    private int currentHealth;
    private Vector3 homeSpot;
    private float angle;
    private bool isDead;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (traits != null && sr != null)
            sr.color = traits.tint;

        homeSpot = transform.position;
        currentHealth = maxHealth;

        angle = Random.Range(0f, 360f);
    }

    private void Update()
    {
        if (isDead)
            return;

        angle += circleSpeed * Time.deltaTime;

        float x = Mathf.Cos(angle) * circleRadius;
        float y = Mathf.Sin(angle) * circleRadius;

        transform.position = homeSpot + new Vector3(x, y, 0f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;

        Debug.Log("Fly Enemy HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Spawn explosion.
        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Play death sound.
        if (deathClip != null)
            AudioSource.PlayClipAtPoint(
                deathClip,
                transform.position
            );

        // Spawn physical, non-collectible item.
        if (dropPrefab != null)
        {
            GameObject drop = Instantiate(
                dropPrefab,
                transform.position,
                Quaternion.identity
            );

            Rigidbody2D dropRb = drop.GetComponent<Rigidbody2D>();

            if (dropRb != null)
            {
                float randomSide = Random.Range(
                    -sideForce,
                    sideForce
                );

                dropRb.linearVelocity = new Vector2(
                    randomSide,
                    launchForce
                );
            }

            Destroy(drop, dropLifetime);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Keep existing player collision behavior.
        }
    }
}
