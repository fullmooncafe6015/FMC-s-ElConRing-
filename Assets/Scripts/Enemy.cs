using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Goes on the Enemy. Patrols left and right using numbers from its EnemyStats asset.
public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyStats stats;
    [SerializeField] float patrolDistance = 2f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private Slider healthBar;

    [Header("Destroy Effect")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip deathClip;

    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;

    private int currentHealth;
    private Vector3 startPosition;
    private float direction = 1f;
    private bool isDead = false;

    private void Start()
    {
        GetComponent<SpriteRenderer>().color = stats.color;

        startPosition = transform.position;
        currentHealth = maxHealth;

        UpdateHealthBar();
    }

    private void Update()
    {
        if (isDead)
            return;

        transform.Translate(
            Vector3.right * direction * stats.moveSpeed * Time.deltaTime,
            Space.World
        );

        if (transform.position.x > startPosition.x + patrolDistance)
            direction = -1f;
        else if (transform.position.x < startPosition.x - patrolDistance)
            direction = 1f;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Enemy took damage! HP: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position);
        }

        if (dropPrefab != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            float dropY = sceneName == "Game1"
                ? 1.69f
                : sceneName == "Room"
                    ? -2f
                    : transform.position.y;

            Vector3 dropPosition = new Vector3(
                transform.position.x,
                dropY,
                transform.position.z
            );

            Instantiate(
                dropPrefab,
                dropPosition,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
                player.Respawn();
        }
    }
}
