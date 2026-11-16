using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (player == null || healthSlider == null)
        {
            Debug.LogError("PlayerHealthBar references are missing!");
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = player.GetMaxHealth();

        UpdateHealthBar();
    }

    private void Update()
    {
        if (player == null || healthSlider == null)
            return;

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthSlider.value = player.GetCurrentHealth();
    }
}
