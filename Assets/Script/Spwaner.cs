using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spwaner : MonoBehaviour
{
    public bool developerMode;
    public Wave[] waves;
    public Enermy enermy;

    LivingEntity playerEntity;
    Transform playerT;

    int remainingToSpwan;
    int remainingAlive;
    int currentWaveNumber=-1;
    float nextSpwanTime;

    MapGenerator map;

    float timeBetweenCampingChecks=2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool playerDeath;

    public event System.Action<int> OnNewWave;
    
    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update()
    {
        if (!playerDeath)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }
            if ((remainingToSpwan > 0||waves[currentWaveNumber].infinite) && Time.time > nextSpwanTime)
            {
                remainingToSpwan--;
                nextSpwanTime = Time.time + waves[currentWaveNumber].timeBetweenSpawns;
                StartCoroutine("SpwanEnemy");
            }
        }
        if (developerMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpwanEnemy");
                foreach(Enermy enermy in FindObjectsOfType<Enermy>())
                {
                    GameObject.Destroy(enermy.gameObject);
                }
                NextWave();
            }
        }
    }
    IEnumerator SpwanEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile=map.Position2Coord(playerT.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
        tileMat.color = initialColor;
        Enermy spwanedEnermy = Instantiate(enermy, spawnTile.position+Vector3.up, Quaternion.identity) as Enermy;
        spwanedEnermy.OnDeath += OnEnermyDeath;
        Wave currentWave = waves[currentWaveNumber];
        spwanedEnermy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }
    void OnPlayerDeath()
    {
        playerDeath = true;
    }
    void OnEnermyDeath()
    {
        //print("enemy died");
        remainingAlive--;
        if (remainingAlive == 0)
        {
            NextWave();
        }
    }
    void ResetPlayerPositon()
    {
        playerT.position = map.Position2Coord(Vector3.zero).position+Vector3.up*3;
    }
    void NextWave()
    {
        currentWaveNumber++;
        if (currentWaveNumber < waves.Length)
        {
            if (currentWaveNumber > 0)
            {
                AudioManager.instance.PlaySound2D("Level Completed");
            }
            remainingToSpwan = waves[currentWaveNumber].enermyCount;
            remainingAlive = remainingToSpwan;
            //Debug.Log(OnNewWave==null);
            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPositon();
        }
    }
    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enermyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
    }
}
