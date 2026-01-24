using UnityEngine;
using UnityEngine.Serialization;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private Transform enemy1;
    [SerializeField] private Transform enemy2;
    [SerializeField] private Transform enemy3;
    [SerializeField] private EnemyWave[] waves;
    [SerializeField] private float enemySpawnMaxDistanceFromCenter;
    [SerializeField] private float enemySphereCheckRadius;
    [SerializeField] private Vector3 enemySpawnOffset;
    
    [Header("Debug")]
    [SerializeField] private bool debug_spawnEnemy1;
    [SerializeField] private bool debug_spawnEnemy2;
    [SerializeField] private bool debug_spawnEnemy3;
    [SerializeField] private bool debug_killEnemies;
    [SerializeField] private bool debug_scrapNearPlayer;
    
    
    private Utils.Timer newWaveTimer;
    private int currentWave = -1;
    private int enemyAmount;
    private bool waveTimerStartedThisWave = true;
    

    GameManager game;
    
    [System.Serializable]
    private class EnemyWave
    {
        public Vector2 enemy1Range;
        public Vector2 enemy2Range;
        public Vector2 enemy3Range;
        [Tooltip("Randomize the 'scattered' bool")]
        public bool randomizeScattered;
        [Tooltip("Should the ships be scattered through the solar system or together")]
        public bool scattered;
        public float startTimer;
    }

    private void Awake()
    {
        game = GameManager.I;
        float startTimer = waves != null && waves.Length >= 1 && waves[0] != null ? waves[0].startTimer : 0;
        newWaveTimer = new Utils.Timer(startTimer);
    }

    private void Update()
    {
        newWaveTimer.Decrement();
        
        if (newWaveTimer.IsDoneOnce())
        {
            waveTimerStartedThisWave = false;
            currentWave++;
            SpawnWave(waves[currentWave]);

            if (Utils.RandomBool())
                ObjectPoolManager.SpawnObject(game.prefabs.newWaveSFX1);
            else
                ObjectPoolManager.SpawnObject(game.prefabs.newWaveSFX2);
            
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_DetectingSignals, true, 0.5f, 8f);
        }

        if (debug_spawnEnemy1 || debug_spawnEnemy2 || debug_spawnEnemy3)
        {
            Vector3 spawnPoint = FindSpawnPoint();

            if (debug_spawnEnemy1)
            {
                SpawnEnemy(enemy1, spawnPoint);
                enemyAmount++;
                debug_spawnEnemy1 = false;
            }
            
            if (debug_spawnEnemy2)
            {
                SpawnEnemy(enemy2, spawnPoint);
                enemyAmount++;
                debug_spawnEnemy2 = false;
            }
            
            if (debug_spawnEnemy3)
            {
                SpawnEnemy(enemy3, spawnPoint);
                enemyAmount++;
                debug_spawnEnemy3 = false;
            }
        }

        if (debug_killEnemies)
        {
            Transform parent = game.hierarchyManager.folder_enemies;

            foreach (var sc in parent.GetComponentsInChildren<SpaceshipPartManager>())
            {
                sc.Debug_KillShip(debug_scrapNearPlayer);
            }
            
            debug_killEnemies = false;
        }
    }

    private void SpawnWave(EnemyWave wave)
    {
        int enemy1Amount = (int)Random.Range(wave.enemy1Range.x, wave.enemy1Range.y + 1);
        int enemy2Amount = (int)Random.Range(wave.enemy2Range.x, wave.enemy2Range.y + 1);
        int enemy3Amount = (int)Random.Range(wave.enemy3Range.x, wave.enemy3Range.y + 1);
        enemyAmount = enemy1Amount + enemy2Amount + enemy3Amount;
        
        bool scattered = wave.randomizeScattered ? Utils.RandomBool() : wave.scattered;

        if (scattered)
        {
            Vector3[] spawnPoints = new Vector3[enemyAmount];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnPoints[i] = FindSpawnPoint();
            }

            for (int i = 0; i < enemy1Amount; i++)
            {
                SpawnEnemy(enemy1, spawnPoints[i]);
            }
            
            for (int i = 0; i < enemy2Amount; i++)
            {
                SpawnEnemy(enemy2, spawnPoints[enemy1Amount + i]);
            }
            
            for (int i = 0; i < enemy3Amount; i++)
            {
                SpawnEnemy(enemy3, spawnPoints[enemy1Amount + enemy2Amount + i]);
            }
        }
        else
        {
            Vector3 spawnPoint = FindSpawnPoint(enemyAmount, enemySpawnOffset * (enemyAmount / 2f));
            
            for (int i = 0; i < enemy1Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                SpawnEnemy(enemy1, spawnPoint);
            }
            
            for (int i = 0; i < enemy2Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                SpawnEnemy(enemy2, spawnPoint);
            }
            
            for (int i = 0; i < enemy3Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                SpawnEnemy(enemy3, spawnPoint);
            }
        }
    }

    private Vector3 FindSpawnPoint(int checkSphereRadiusMultiplier = 1, Vector3 checkSphereOffset = default)
    {
        Vector3 spawnPoint = Utils.RandomPointInSphere(Vector3.zero, enemySpawnMaxDistanceFromCenter);

        bool clear = !Physics.CheckSphere(spawnPoint + checkSphereOffset, enemySphereCheckRadius * checkSphereRadiusMultiplier);

        if (clear)
            return spawnPoint;
        else
            return FindSpawnPoint(checkSphereRadiusMultiplier);
    }

    public void EnemyDefeated()
    {
        enemyAmount--;

        if (enemyAmount <= 0 && Utils.IsInArrayRange(currentWave + 1, waves) && !waveTimerStartedThisWave)
        {
            Debug.Log("Starting wave timer");
            newWaveTimer.Reset(waves[currentWave + 1].startTimer);
            waveTimerStartedThisWave = true;
        }
    }

    private void SpawnEnemy(Transform prefab, Vector3 pos)
    {
        Transform enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.parent = game.hierarchyManager.folder_enemies;
    }
}
