using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    #region Main UI Components
    public GameObject failScreen;
    public GameObject homeScreen;
    public GameObject playScreen;
    public GameObject pauseScreen;
    #endregion

    #region Mutable UI Components
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI yourScoreDisplay;
    public Button continueButton;
    public List<TextMeshProUGUI> highScoreDisplays;
    public Toggle bgMusicToggle;
    public Toggle sfxToggle;
    #endregion

    private void Start()
    {
        bgMusicToggle.isOn = PlayerPrefs.GetInt("BGMusic") > 0;
        sfxToggle.isOn = PlayerPrefs.GetInt("SFX") > 0;

        OnToggleBGMusic();
        OnToggleSFX();

        UpdateHighScoreDisplays();

        continueButton.onClick?.AddListener(GameManager.gameInstance.adsManager.ShowRewarded);
    }

    public void UpdateScore()
    {
        scoreDisplay.text = GameManager.score.ToString();
    }

    public void ShowFailScreen(bool newHighScore)
    {
        playScreen.SetActive(false);
        failScreen.SetActive(true);
        yourScoreDisplay.text = GameManager.score.ToString();
        UpdateHighScoreDisplays();
        if (newHighScore)
        {
            failScreen.GetComponent<Animator>().SetTrigger("NewHighScore");
        }
    }

    public void ShowPlayScreen()
    {
        failScreen.SetActive(false);
        homeScreen.SetActive(false);
        pauseScreen.SetActive(false);
        playScreen.SetActive(true);
    }

    public void ShowPauseScreen()
    {
        pauseScreen.SetActive(true);
        failScreen.SetActive(false);
        homeScreen.SetActive(false);
        playScreen.SetActive(false);
    }

    public void UpdateFailScreen(int adsNeeded)
    {
        yourScoreDisplay.text = GameManager.score.ToString();
        UpdateHighScoreDisplays();
        continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue? " + adsNeeded;
        continueButton.gameObject.SetActive(adsNeeded <= 2 && AdsManager.IsRewardedAdReady);
    }

    public void OnFailScreenClicked()
    {
        GameManager.gameInstance.OnReStartGame();

        AudioManager.PlayUIButtonSFX();
    }

    public void OnToggleBGMusic()
    {
        AudioManager.isBgMusicEnabled = bgMusicToggle.isOn;
        PlayerPrefs.SetInt("BGMusic", bgMusicToggle.isOn ? 1 : 0);
    }

    public void OnToggleSFX()
    {
        AudioManager.isSfxEnabled = sfxToggle.isOn;
        PlayerPrefs.SetInt("SFX", sfxToggle.isOn ? 1 : 0);
    }

    private void UpdateHighScoreDisplays()
    {
        foreach (TextMeshProUGUI highScoreDisplay in highScoreDisplays)
        {
            highScoreDisplay.text = PlayerPrefs.GetInt("HighScore").ToString();
        }
    }

    public void OnPauseButtonClicked()
    {
        ShowPauseScreen();
        GameManager.Pause();
    }

    public void OnResumeClicked()
    {
        ShowPlayScreen();
        GameManager.Resume();
    }
}
