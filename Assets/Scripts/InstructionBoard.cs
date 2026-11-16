using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionBoard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject instructionPanel;

    [Header("Animation")]
    [SerializeField] private Animator instructionAnimator;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;

    private bool playerNearby = false;
    private bool panelOpen = false;

    private void Start()
    {
        if (instructionPanel == null)
        {
            Debug.LogError("Instruction Panel is not assigned!");
            enabled = false;
            return;
        }

        instructionPanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Keyboard.current == null)
            return;

        // Open or close panel with F.
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (panelOpen)
                ClosePanel();
            else
                OpenPanel();
        }

        // Close panel with Escape.
        if (panelOpen &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
        }
    }

    private void OpenPanel()
    {
        if (instructionPanel == null)
        {
            Debug.LogError("Instruction Panel is missing or destroyed!");
            return;
        }

        panelOpen = true;

        instructionPanel.SetActive(true);

        // Restart GIF animation.
        if (instructionAnimator != null)
        {
            instructionAnimator.enabled = true;
            instructionAnimator.Rebind();
            instructionAnimator.Update(0f);
            instructionAnimator.Play(0, 0, 0f);
        }
        else
        {
            Debug.LogWarning("Instruction Animator is not assigned!");
        }

        Debug.Log("Instruction panel opened!");
    }

    private void ClosePanel()
    {
        panelOpen = false;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        Debug.Log("Instruction panel closed!");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            ClosePanel();

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    private void OnDisable()
    {
        playerNearby = false;
        panelOpen = false;
    }
}
