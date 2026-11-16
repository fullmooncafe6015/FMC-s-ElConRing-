using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for breakable pot.
        Pot pot = other.GetComponentInParent<Pot>();

        if (pot != null)
        {
            pot.BreakPot();
            Debug.Log("Pot destroyed!");
            return;
        }

        // Check for enemy.
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log("Enemy hit!");
        }
    }
}
