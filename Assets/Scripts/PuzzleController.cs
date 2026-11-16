using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PuzzleController : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredSwitches = 4;

    [Header("UI")]
    [SerializeField] private TMP_Text switchCounterText;
    [SerializeField] private TMP_Text portalNotificationText;
    [SerializeField] private float notificationDuration = 4f;

    [Header("Portal Appearance Sound")]
    [SerializeField] private AudioClip portalAppearSound;

    private HashSet<int> activatedSwitches =
        new HashSet<int>();

    private GameObject portal;
    private bool puzzleCompleted = false;
    private bool portalSoundPlayed = false;
    private bool isGame1Scene;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        isGame1Scene =
            SceneManager.GetActiveScene().name == "Game1";

        if (!isGame1Scene)
            enabled = false;
    }

    private void Start()
    {
        if (!isGame1Scene)
            return;

        UpdateSwitchCounter();

        if (portalNotificationText != null)
            portalNotificationText.gameObject.SetActive(false);
    }

    public void SetRequiredSwitches(int amount)
    {
        requiredSwitches = Mathf.Max(1, amount);
        UpdateSwitchCounter();
    }

    public void ResetPuzzle()
    {
        activatedSwitches.Clear();
        puzzleCompleted = false;
        portalSoundPlayed = false;

        if (portal != null)
        {
            portal.SetActive(false);
            portal = null;
        }

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (portalNotificationText != null)
            portalNotificationText.gameObject.SetActive(false);

        UpdateSwitchCounter();

        Debug.Log("Puzzle reset!");
    }

    public void RegisterPortal(GameObject portalObject)
    {
        portal = portalObject;

        if (portal != null)
        {
            portal.SetActive(puzzleCompleted);

            if (puzzleCompleted)
                PlayPortalSound();
        }
    }

    public bool ActivateSwitch(int switchId)
    {
        if (!isGame1Scene || puzzleCompleted)
            return false;

        if (!activatedSwitches.Add(switchId))
            return false;

        Debug.Log(
            "Switch " + switchId + " activated! (" +
            activatedSwitches.Count + "/" +
            requiredSwitches + ")"
        );

        UpdateSwitchCounter();

        if (activatedSwitches.Count >= requiredSwitches)
        {
            CompletePuzzle();
        }

        return true;
    }

    public bool IsSwitchActivated(int switchId)
    {
        return activatedSwitches.Contains(switchId);
    }

    private void UpdateSwitchCounter()
    {
        if (switchCounterText != null)
        {
            switchCounterText.text =
                "Switches: " +
                activatedSwitches.Count +
                " / " +
                requiredSwitches;
        }
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        Debug.Log("All switches activated! Portal unlocked!");

        if (portal != null)
        {
            portal.SetActive(true);
            PlayPortalSound();
        }
        else
        {
            Debug.Log(
                "Portal will appear when its room is generated."
            );
        }

        ShowPortalNotification();
    }

    private void ShowPortalNotification()
    {
        if (portalNotificationText == null)
            return;

        portalNotificationText.text =
            "Portal Appears Somewhere!";

        portalNotificationText.gameObject.SetActive(true);

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine =
            StartCoroutine(HideNotification());
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (portalNotificationText != null)
            portalNotificationText.gameObject.SetActive(false);

        notificationCoroutine = null;
    }

    private void PlayPortalSound()
    {
        if (portalSoundPlayed)
            return;

        if (portalAppearSound != null &&
            SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(portalAppearSound);
        }

        portalSoundPlayed = true;
    }
}
