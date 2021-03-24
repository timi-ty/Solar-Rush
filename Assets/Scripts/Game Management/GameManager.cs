using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[ExecuteAlways]
public class GameManager : SingletonMono<GameManager>
{
    #region Developer Settings
    [Header("Developer Settings")]
    [Tooltip("DEVELOPER ONLY! Disable for production build.")]
    public bool disableInterstitialAds;
    [Tooltip("DEVELOPER ONLY! Disable for production build.")]
    public bool rewardsWithoutAds;
    #endregion

    #region Editor
    private static GameManager _editorInstance;
    public static GameManager editorInstance
    {
        get
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("You grabbed the editor reserved instance of " + gameInstance.name + " during playtime.");
            }
            return _editorInstance;
        }
        private set => _editorInstance = value;
    }
    #endregion

    #region Shared Resources
    [Header("Shared Resources")]
    [SerializeReference]
    private ColorPool _colorPool;
    public static ColorPool colorPool
    {
        get
        {
            if (Application.isPlaying)
            {
                if (!gameInstance)
                {
                    Debug.LogWarning("There must be ONE \"GameManager\" in the scene.");
                    return null;
                }
                return gameInstance._colorPool;
            }
            else
            {
                if (!editorInstance)
                {
                    Debug.LogWarning("There must be ONE \"GameManager\" in the scene.");
                    return null;
                }
                return editorInstance._colorPool;
            }
        }
    }
    #endregion

    #region Game Components
    private CentreEarth _centreEarth;
    public static CentreEarth centreEarth { get => gameInstance._centreEarth; private set => gameInstance._centreEarth = value; }
    private CometManager cometManager;
    private LevelManager levelManager;
    private List<ColorWheel> colorWheels = new List<ColorWheel>();
    #endregion

    #region Application Components
    public InputManager inputManager;
    public UIManager uiManager;
    public AdsManager adsManager;
    #endregion

    #region Game State
    private int _score;
    public static int score
    {
        get => gameInstance._score; private set
        {
            gameInstance._score = value;
            gameInstance.uiManager.UpdateScore();
        }
    }
    public static bool isPlaying { get; private set; }
    private int _requiredPointsForContinue;
    private int requiredPointsForContinue
    {
        get => _requiredPointsForContinue; 
        set
        {
            _requiredPointsForContinue = value;
            uiManager.UpdateFailScreen(_requiredPointsForContinue - continuePoints);
        }
    }
    private int continuePoints { get; set; }
    #endregion

    #region Extras
    public AudioClip gameOverAudioClip;
    #endregion

    private void OnEnable()
    {
        editorInstance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Prepare();
    }
    private void Prepare()
    {
        colorWheels.Clear();

        FindComponents();

        inputManager.enabled = false;
    }

    public void OnStartGame()
    {
        inputManager.enabled = true;
        cometManager.DestroyAllComets();
        cometManager.ResumeSpawning();
        levelManager.StartLeveling();

        score = 0;
        continuePoints = 0;
        requiredPointsForContinue = 1;

        isPlaying = true;

        AudioManager.PlayUIPomButtonSFX();
    }

    public void OnReStartGame()
    {
        OnStartGame();

        uiManager.ShowPlayScreen();
    }

    public static void StartLevel(int level)
    {
        foreach (ColorWheel colorWheel in gameInstance.colorWheels)
        {
            colorWheel.segmentCount = 2 + level;
            colorWheel.RefreshWheel(true, gameInstance.OnWheelRefreshed);
        }
    }

    private void OnWheelRefreshed()
    {
        cometManager.ResumeSpawning();
        levelManager.ResumeLeveling();
    }

    public static void PrepareForLevelUp(Action onPrepared)
    {
        gameInstance.StartCoroutine(gameInstance.PrepareForLevelUpCoroutine(onPrepared));
    }

    public IEnumerator PrepareForLevelUpCoroutine(Action onPrepared)
    {
        gameInstance.cometManager.StopSpawning();
        yield return new WaitUntil(() => cometManager.liveComets == 0);
        onPrepared?.Invoke();
    }

    public static void RegisterColorWheel(ColorWheel colorWheel)
    {
        gameInstance.colorWheels.Add(colorWheel);
        gameInstance.colorWheels = gameInstance.colorWheels.OrderBy(x => x.radius).ToList();
        gameInstance.inputManager.RegisterHandler(colorWheel);
    }

    public static ColorWheel GetWheel(int index)
    {
        if (index < 0 || index > gameInstance.colorWheels.Count)
        {
            Debug.LogWarning("You requested for ColorWheel " + index + " which either does not exist or has not been registered");
        }

        index = Mathf.Min(index, gameInstance.colorWheels.Count);

        if (gameInstance.colorWheels.Count < 1) return null;

        return gameInstance.colorWheels[index];
    }

    public static ColorWheel GetEnclosedWheel(ColorWheel colorWheel)
    {
        //Not functional!!!!
        if (!colorWheel) return null;

        RaycastHit2D wheelHit = Physics2D.Raycast((Vector2)colorWheel.transform.position + Vector2.up * colorWheel.radius,
            Vector2.down, 200, LayerMask.GetMask("Wheel"));

        ColorWheel innerWHeel = wheelHit.transform.GetComponentInParent<ColorWheel>();

        return innerWHeel;
    }

    public static ColorWheel GetOutermostWheel()
    {
        if (gameInstance.colorWheels.Count < 1) return null;
        return gameInstance.colorWheels[gameInstance.colorWheels.Count - 1];
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void OnFailGame()
    {
        isPlaying = false;

        bool newHighScore = false;
        if (score > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", score);
            newHighScore = true;
        }

        inputManager.enabled = false;
        cometManager.StopSpawning();
        cometManager.DestroyAllComets();
        levelManager.StopLeveling();
        uiManager.ShowFailScreen(newHighScore);

        AudioManager.PlayGameClip(gameOverAudioClip);
    }

    private void ContinueAfterFailure()
    {
        inputManager.enabled = true;
        cometManager.DestroyAllComets();
        cometManager.ResumeSpawning();
        levelManager.ResumeLeveling();

        uiManager.ShowPlayScreen();

        isPlaying = true;

        AudioManager.PlayUIPomButtonSFX();
    }

    public static void OnCometDestroyed()
    {
        if (!isPlaying) return;

        score++;
    }

    public static void Pause()
    {
        AudioManager.EnterPauseSnapshot();
        Time.timeScale = 0;
    }

    public static void Resume()
    {
        AudioManager.EnterUnpauseSnapshot();
        Time.timeScale = 1;
    }

    private void FindComponents()
    {
        cometManager = FindObjectOfType<CometManager>();
        levelManager = FindObjectOfType<LevelManager>();
        inputManager = FindObjectOfType<InputManager>();
        uiManager = FindObjectOfType<UIManager>();
        adsManager = FindObjectOfType<AdsManager>();
        _centreEarth = FindObjectOfType<CentreEarth>();
    }

    public static void HandleFullScreenAdOpened()
    {
        Time.timeScale = 0;
        AudioManager.FreezeAudio();
    }

    public static void HandleUserEarnedReward()
    {
        gameInstance.continuePoints++;
        gameInstance.uiManager.UpdateFailScreen(gameInstance.requiredPointsForContinue - gameInstance.continuePoints);

        if (gameInstance.continuePoints >= gameInstance.requiredPointsForContinue)
        {
            gameInstance.ContinueAfterFailure();
            gameInstance.continuePoints = 0;
            gameInstance.requiredPointsForContinue++;
        }
    }

    public static void HandleRewardedAdClosed()
    {
        Time.timeScale = 1;
        AudioManager.UnfreezeAudio();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pause)
    {
        PlayerPrefs.Save();
    }
}
