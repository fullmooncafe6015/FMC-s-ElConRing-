using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Background Music")]
    [SerializeField] private AudioClip titleScreenMusic;
    [SerializeField] private AudioClip roomMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip finishMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private const string MasterParameter = "MasterVolume";
    private const string MusicParameter = "MusicVolume";
    private const string SFXParameter = "SFXVolume";

    private Coroutine musicTransition;

    private void Awake()
    {
        // Prevent duplicate SoundManagers.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find or create AudioSources.
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Stop();

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Load saved volume settings.
        SetMasterVolume(
            PlayerPrefs.GetFloat(MasterVolumeKey, 1f)
        );

        SetMusicVolume(
            PlayerPrefs.GetFloat(MusicVolumeKey, 1f)
        );

        SetSFXVolume(
            PlayerPrefs.GetFloat(SFXVolumeKey, 1f)
        );

        // Wait for Press Any Button before playing title music.
        if (SceneManager.GetActiveScene().name != "TitleScreen")
        {
            PlayMusicForScene(
                SceneManager.GetActiveScene().name
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScreen")
        {
            StopTitleMusic();
            return;
        }

        PlayMusicForScene(scene.name);
    }

    private void StopTitleMusic()
    {
        if (musicTransition != null)
        {
            StopCoroutine(musicTransition);
            musicTransition = null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = 1f;
    }

    public void StartTitleScreenMusic()
    {
        PlayMusicForScene("TitleScreen");
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clip = null;

        switch (sceneName)
        {
            case "TitleScreen":
                clip = titleScreenMusic;
                break;

            case "Room":
                clip = roomMusic;
                break;

            case "Game1":
                clip = gameMusic;
                break;

            case "FinishGame":
                clip = finishMusic;
                break;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                "No background music assigned for scene: " + sceneName
            );
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (musicTransition != null)
            StopCoroutine(musicTransition);

        musicTransition = StartCoroutine(ChangeMusic(clip));
    }

    private IEnumerator ChangeMusic(AudioClip newClip)
    {
        // Fade out current music.
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            musicSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                elapsed / fadeDuration
            );

            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Start new track.
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music.
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            musicSource.volume = Mathf.Lerp(
                0f,
                1f,
                elapsed / fadeDuration
            );

            yield return null;
        }

        musicSource.volume = 0.5f;
        musicTransition = null;
    }

    // Play sound effects.
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    // MASTER VOLUME
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(
                MasterParameter,
                VolumeToDecibels(volume)
            );
        }

        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    // MUSIC VOLUME
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(
                MusicParameter,
                VolumeToDecibels(volume)
            );
        }

        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }

    // SFX VOLUME
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            audioMixer.SetFloat(
                SFXParameter,
                VolumeToDecibels(volume)
            );
        }

        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
        PlayerPrefs.Save();
    }

    private float VolumeToDecibels(float volume)
    {
        if (volume <= 0.0001f)
            return -80f;

        return Mathf.Log10(volume) * 20f;
    }
}