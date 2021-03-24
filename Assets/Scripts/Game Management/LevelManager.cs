using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    #region Global Properties
    public float levelUpInterval;
    public int maxLevel;
    public static int currentLevel;
    #endregion

    #region WorkerParameters
    private float timeElapsed;
    #endregion

    private void Start()
    {
        StopLeveling();
    }

    public void StartLeveling()
    {
        enabled = true;
        currentLevel = 0;
        timeElapsed = 0;
        LevelUp();
    }

    public void ResumeLeveling()
    {
        enabled = true;
    }

    public void StopLeveling()
    {
        enabled = false;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (currentLevel < maxLevel && timeElapsed >= levelUpInterval)
        {
            timeElapsed = 0;
            PrepareForLevelUp();
        }
    }

    private void PrepareForLevelUp()
    {
        StopLeveling();
        GameManager.PrepareForLevelUp(LevelUp);
    }

    private void LevelUp()
    {
        currentLevel++;
        GameManager.StartLevel(currentLevel);
    }
}
