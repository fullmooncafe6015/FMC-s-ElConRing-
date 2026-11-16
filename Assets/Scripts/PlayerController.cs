using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Double Jump")]
    [SerializeField] private int maxJumps = 2;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Respawn")]
    [SerializeField] private Transform startingPoint;
    [SerializeField] private float fallLimit = -8f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitboxRight;
    [SerializeField] private GameObject attackHitboxLeft;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Combat Sounds")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip shootingSound;

    [Header("GraceSpark")]
    [SerializeField] private ParticleSystem collectBurst;

    private Rigidbody2D rb;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector3 initialPosition;

    private int currentHealth;
    private int jumpsRemaining;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isDead;
    private bool isPaused;

    private bool facingRight = true;
    private bool isAttacking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        currentHealth = maxHealth;
        jumpsRemaining = maxJumps;

        if (startingPoint != null)
            initialPosition = startingPoint.position;
        else
            initialPosition = transform.position;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        DisableAllAttackHitboxes();
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
        controls.Player.Jump.performed += OnJump;
        controls.Player.Attack.performed += OnAttack;
        controls.Player.Shoot.performed += OnShoot;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Jump.performed -= OnJump;
        controls.Player.Attack.performed -= OnAttack;
        controls.Player.Shoot.performed -= OnShoot;

        controls.Player.Disable();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }

    private void Update()
    {
        if (isDead || isPaused)
            return;

        isGrounded = IsGrounded();

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            Debug.Log("Player landed. Jumps restored!");
        }

        wasGrounded = isGrounded;

        UpdateFacingDirection();
        UpdateAnimations();

        if (transform.position.y < fallLimit)
            Respawn();
    }

    private void FixedUpdate()
    {
        if (isDead || isPaused)
            return;

        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead || isPaused)
            return;

        if (jumpsRemaining <= 0)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpsRemaining--;

        wasGrounded = false;
        isGrounded = false;

        UpdateAnimations();

        Debug.Log("Jumps remaining: " + jumpsRemaining);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead || isPaused || isAttacking)
            return;

        if (animator == null)
            return;

        isAttacking = true;

        animator.SetTrigger("IsAttacking");

        Debug.Log("Player attacking!");
    }

    // ATTACK HITBOX

    public void EnableAttackHitbox()
    {
        if (isDead || isPaused)
            return;

        DisableAllAttackHitboxes();

        if (facingRight)
        {
            if (attackHitboxRight != null)
                attackHitboxRight.SetActive(true);
        }
        else
        {
            if (attackHitboxLeft != null)
                attackHitboxLeft.SetActive(true);
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(attackSound);
    }

    public void DisableAttackHitbox()
    {
        DisableAllAttackHitboxes();

        isAttacking = false;
    }

    private void DisableAllAttackHitboxes()
    {
        if (attackHitboxRight != null)
            attackHitboxRight.SetActive(false);

        if (attackHitboxLeft != null)
            attackHitboxLeft.SetActive(false);
    }

    // SHOOTING

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (isDead || isPaused || isAttacking)
            return;

        if (animator == null)
            return;

        animator.SetTrigger("IsShooting");

        Debug.Log("Player shooting!");
    }

    public void FireBullet()
    {
        if (isDead || isPaused)
            return;

        if (bulletPrefab == null || bulletSpawnPoint == null)
        {
            Debug.LogError(
                "Bullet Prefab or Bullet Spawn Point is missing!"
            );
            return;
        }

        Vector2 direction = facingRight
            ? Vector2.right
            : Vector2.left;

        Vector3 spawnPosition = bulletSpawnPoint.position;

        spawnPosition.x = transform.position.x +
            (facingRight
                ? Mathf.Abs(bulletSpawnPoint.localPosition.x)
                : -Mathf.Abs(bulletSpawnPoint.localPosition.x));

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Projectile projectile = bullet.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                bulletSpeed,
                bulletDamage
            );

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(shootingSound);
        }
        else
        {
            Debug.LogError(
                "Projectile script is missing from the bullet prefab!"
            );

            Destroy(bullet);
        }
    }

    // GROUND CHECK

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck is not assigned!");
            return false;
        }

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    // FACING DIRECTION

    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0.01f)
        {
            facingRight = true;

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.01f)
        {
            facingRight = false;

            if (spriteRenderer != null)
                spriteRenderer.flipX = true;
        }
    }

    // ANIMATIONS

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);

        animator.SetBool(
            "IsRunning",
            Mathf.Abs(moveInput.x) > 0.01f && isGrounded
        );
    }

    // HEALTH AND DAMAGE

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        PlayHurtAnimation();
    }

    public void PlayHurtAnimation()
    {
        if (isDead || isPaused || animator == null)
            return;

        animator.ResetTrigger("IsHurt");
        animator.SetTrigger("IsHurt");

        Debug.Log("Hurt animation triggered!");
    }

    // DEATH

    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        moveInput = Vector2.zero;

        DisableAllAttackHitboxes();

        isAttacking = false;

        Debug.Log("Player died!");

        Invoke(nameof(Respawn), 1f);
    }

    // RESPAWN

    public void Respawn()
    {
        CancelInvoke(nameof(Respawn));

        isDead = false;
        currentHealth = maxHealth;
        jumpsRemaining = maxJumps;

        moveInput = Vector2.zero;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.position = new Vector2(
            initialPosition.x,
            initialPosition.y
        );

        wasGrounded = false;
        isGrounded = false;

        facingRight = true;
        isAttacking = false;

        if (spriteRenderer != null)
            spriteRenderer.flipX = false;

        DisableAllAttackHitboxes();

        Physics2D.SyncTransforms();

        if (animator != null)
        {
            animator.SetBool("IsGrounded", false);
            animator.SetBool("IsRunning", false);
            animator.SetFloat("Speed", 0f);

            animator.ResetTrigger("IsAttacking");
            animator.ResetTrigger("IsShooting");
            animator.ResetTrigger("IsHurt");
        }

        Debug.Log("Player respawned!");
    }

    // PAUSE

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;

            DisableAllAttackHitboxes();

            isAttacking = false;

            if (animator != null)
                animator.SetBool("IsRunning", false);
        }
    }

    // PICKUP EFFECT

    public void PlayCollectEffect()
    {
        if (collectBurst != null)
            collectBurst.Play();
    }

    // GETTERS

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetJumpsRemaining()
    {
        return jumpsRemaining;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}
