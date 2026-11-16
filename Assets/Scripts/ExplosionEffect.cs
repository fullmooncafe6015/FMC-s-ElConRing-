using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f;

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}