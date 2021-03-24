using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : SingletonMono<AudioManager>
{
    #region Components
    public AudioMixerSnapshot pauseSnapshot;
    public AudioMixerSnapshot unpauseSnapshot;
    public AudioSource uiAudioSource;
    public AudioSource bgMusicSource;
    public AudioSource gamePlayAudioSource;
    public AudioSource ambienceAudioSource;
    #endregion

    #region Audio Clips
    public AudioClip uiPomButttonClip;
    public AudioClip uiNormButtonAudioClip;
    #endregion

    #region Properties
    private static bool _isSfxEnabled;
    private static bool _isBgMusicEnabled;

    public static bool isSfxEnabled
    {
        get => _isSfxEnabled; set
        {
            if (gameInstance.uiAudioSource) gameInstance.uiAudioSource.enabled = value;
            if (gameInstance.gamePlayAudioSource) gameInstance.gamePlayAudioSource.enabled = value;
            _isSfxEnabled = value;
        }
    }
    public static bool isBgMusicEnabled
    {
        get => _isBgMusicEnabled; set
        {
            if(gameInstance.bgMusicSource) gameInstance.bgMusicSource.enabled = value;
            if (gameInstance.ambienceAudioSource) gameInstance.ambienceAudioSource.enabled = value;
            _isBgMusicEnabled = value;
        }
    }
    #endregion

    private void Start()
    {
        CheckAudioSettings();
    }

    public static void EnterPauseSnapshot()
    {
        gameInstance.pauseSnapshot.TransitionTo(.01f);
    }

    public static void EnterUnpauseSnapshot()
    {
        gameInstance.unpauseSnapshot.TransitionTo(.01f);
    }

    public static void PlayUIPomButtonSFX()
    {
        if (!isSfxEnabled) return;

        gameInstance.uiAudioSource.PlayOneShot(gameInstance.uiPomButttonClip);
    }

    public static void PlayUIButtonSFX()
    {
        if (!isSfxEnabled) return;

        gameInstance.uiAudioSource.PlayOneShot(gameInstance.uiNormButtonAudioClip);
    }

    public static void PlayUIClip(AudioClip audioClip)
    {
        if (!isSfxEnabled) return;

        gameInstance.uiAudioSource.PlayOneShot(audioClip);
    }

    public static void PlayUILooping(AudioClip audioClip)
    {
        if (!isSfxEnabled) return;

        gameInstance.uiAudioSource.clip = audioClip;
        gameInstance.uiAudioSource.loop = true;
        gameInstance.uiAudioSource.Play();
    }

    public static void StopUILooping()
    {
        gameInstance.uiAudioSource.Stop();
        gameInstance.uiAudioSource.loop = false;
        gameInstance.uiAudioSource.clip = null;
    }

    public static void PlayGameClip(AudioClip audioClip)
    {
        if (!isSfxEnabled) return;

        gameInstance.gamePlayAudioSource.PlayOneShot(audioClip);
    }

    public static void TurnUpBgMusic()
    {
        if (!isBgMusicEnabled) return;

        gameInstance.StopAllCoroutines();

        gameInstance.StartCoroutine(gameInstance.FadeInAudio(gameInstance.bgMusicSource, 0.4f));
    }

    public static void TurnDownBgMusic()
    {
        if (!isBgMusicEnabled) return;

        gameInstance.StopAllCoroutines();

        gameInstance.StartCoroutine(gameInstance.FadeOutAudio(gameInstance.bgMusicSource, 0.1f));
    }

    #region Utility Methods
    public static void CheckAudioSettings()
    {
        gameInstance.bgMusicSource.enabled = isBgMusicEnabled;

        gameInstance.uiAudioSource.enabled = isSfxEnabled;

        gameInstance.ambienceAudioSource.enabled = isSfxEnabled;

        gameInstance.gamePlayAudioSource.enabled = isSfxEnabled;
    }

    public static void FreezeAudio()
    {
        gameInstance.bgMusicSource.enabled = false;

        gameInstance.uiAudioSource.enabled = false;

        gameInstance.ambienceAudioSource.enabled = false;

        gameInstance.gamePlayAudioSource.enabled = false;
    }

    public static void UnfreezeAudio()
    {
        CheckAudioSettings();
    }

    private IEnumerator FadeInAudio(AudioSource audioSource, float final)
    {
        while(audioSource.volume < final)
        {
            audioSource.volume += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = final;
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float final)
    {
        while (audioSource.volume > final)
        {
            audioSource.volume -= Time.deltaTime;
            yield return null;
        }
        audioSource.volume = final;
    }
    #endregion
}
