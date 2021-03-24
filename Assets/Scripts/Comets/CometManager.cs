using UnityEngine;
using System.Collections.Generic;

public class CometManager : MonoBehaviour
{
    #region Prefabs
    public Comet cometPrefab;
    #endregion

    #region Settings
    public float baseSpawnRate;
    public float extraSpawnRate;
    #endregion

    #region WorkerParameters
    private float timeElapsed;
    private float spawnPeriod;
    private int worldBoundLayerMask;
    public int liveComets { get; private set; }
    #endregion

    private void Start()
    {
        spawnPeriod = 1 / baseSpawnRate;

        worldBoundLayerMask = LayerMask.GetMask("WorldBound");

        StopSpawning();
    }

    public void ResumeSpawning()
    {
        enabled = true;
    }

    public void StopSpawning()
    {
        enabled = false;
    }

    public void DestroyAllComets()
    {
        BroadcastMessage("DestroySelf", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= spawnPeriod)
        {
            int shouldMultiSpawn = Random.Range(0, 6);
            if(shouldMultiSpawn == 0)
            {
                MultiSpawn();
                timeElapsed = -spawnPeriod;
            }
            else
            {
                SpawnComet();
                timeElapsed = 0;
            }
        }
        
        spawnPeriod = 1 / (baseSpawnRate + (extraSpawnRate * LevelManager.currentLevel));
    }

    private void SpawnComet(SpawnInfo spawnInfo = null)
    {
        if(spawnInfo == null)
        {
            Vector2 direction = Random.onUnitSphere;
            Quaternion rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x), Vector3.forward);

            RaycastHit2D worldBoundHit = Physics2D.Raycast(GameManager.centreEarth.transform.position, direction, 200, worldBoundLayerMask);

            if (worldBoundHit.transform != null)
            {
                Comet comet = Instantiate(cometPrefab, worldBoundHit.point, rotation, transform);
                liveComets++;
                int colorId = GameManager.GetOutermostWheel().GetRandomColorId();
                comet.nextColorId = colorId;
                comet.needsAlert = false;
                comet.cometManager = this;
            }
        }
        else
        {
            Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * spawnInfo.angleFromReference), Mathf.Sin(Mathf.Deg2Rad * spawnInfo.angleFromReference));

            Quaternion rotation = Quaternion.AngleAxis(spawnInfo.angleFromReference, Vector3.forward);

            RaycastHit2D worldBoundHit = Physics2D.Raycast(GameManager.centreEarth.transform.position, direction, 200, worldBoundLayerMask);

            if (worldBoundHit.transform != null)
            {
                Comet comet = Instantiate(cometPrefab, worldBoundHit.point, rotation, transform);
                liveComets++;
                int colorId = spawnInfo.colorId;
                comet.nextColorId = colorId;
                comet.needsAlert = true;
                comet.cometManager = this;
            }
        }
    }

    private void MultiSpawn()
    {
        int spawnCount = Random.Range(2, GameManager.GetOutermostWheel().segmentCount);
        List<SpawnInfo> multiSpawnInfo = GameManager.GetOutermostWheel().GetBeatableMultiSpawn(spawnCount);
        foreach(SpawnInfo spawnInfo in multiSpawnInfo)
        {
            SpawnComet(spawnInfo);
        }
    }

    public void OnCometDestroyed()
    {
        liveComets--;
        GameManager.OnCometDestroyed();
    }
}

public class SpawnInfo
{
    public int colorId;
    public float angleFromReference;

    public SpawnInfo(int colorId, float angleFromReference)
    {
        this.colorId = colorId;
        this.angleFromReference = angleFromReference;
    }
}
