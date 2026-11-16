using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Dropped Flower")]
    [SerializeField] private GameObject droppedFlowerPrefab;
    [SerializeField] private float dropForceX = 2f;
    [SerializeField] private float dropForceY = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            return;

        Vector3 dropPosition = player.transform.position;

        player.TakeDamage(damage);

        if (GameSession.Score > 0)
        {
            GameSession.Score--;

            if (droppedFlowerPrefab != null)
            {
                GameObject dropped = Instantiate(
                    droppedFlowerPrefab,
                    dropPosition,
                    Quaternion.identity
                );

                DropFlower dropFlower = dropped.GetComponent<DropFlower>();

                if (dropFlower != null)
                {
                    float horizontalForce = Random.Range(-dropForceX, dropForceX);
                    dropFlower.Launch(
                        player,
                        new Vector2(horizontalForce, dropForceY)
                    );
                }
            }
        }
    }
}