using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private Transform enemy1;
    [SerializeField] private Transform enemy2;
    [SerializeField] private EnemyWave[] waves;
    [SerializeField] private float enemySpawnMaxDistanceFromCenter;
    [SerializeField] private float enemySphereCheckRadius;
    [SerializeField] private Vector3 enemySpawnOffset;
    
    private Utils.Timer newWaveTimer;
    private int currentWave = -1;
    private int enemyAmount;
    
    [System.Serializable]
    private class EnemyWave
    {
        public Vector2 enemy1Range;
        public Vector2 enemy2Range;
        [Tooltip("Randomize the 'scattered' bool")]
        public bool randomizeScattered;
        [Tooltip("Should the ships be scattered through the solar system or together")]
        public bool scattered;
        public float cooldown;
    }

    private void Awake()
    {
        newWaveTimer = new Utils.Timer(5);
    }

    private void Update()
    {
        newWaveTimer.Decrement();
        
        if (newWaveTimer.IsDoneOnce())
        {
            Debug.Log("next wave");
            currentWave++;
            SpawnWave(waves[currentWave]);
        }
    }

    private void SpawnWave(EnemyWave wave)
    {
        int enemy1Amount = (int)Random.Range(wave.enemy1Range.x, wave.enemy1Range.y + 1);
        int enemy2Amount = (int)Random.Range(wave.enemy2Range.x, wave.enemy2Range.y + 1);
        enemyAmount = enemy1Amount + enemy2Amount;
        
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
                Instantiate(enemy1, spawnPoints[i], Quaternion.identity);
            }
            
            for (int i = 0; i < enemy2Amount; i++)
            {
                Instantiate(enemy2, spawnPoints[enemy1Amount + i], Quaternion.identity);
            }
        }
        else
        {
            Vector3 spawnPoint = FindSpawnPoint(enemyAmount, enemySpawnOffset * (enemyAmount / 2f));
            
            for (int i = 0; i < enemy1Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                Instantiate(enemy1, spawnPoint, Quaternion.identity);
            }
            
            for (int i = 0; i < enemy2Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                Instantiate(enemy2, spawnPoint, Quaternion.identity);
            }
        }
    }

    private Vector3 FindSpawnPoint(int radiusMultiplier = 1, Vector3 checkSphereOffset = default)
    {
        Vector3 spawnPoint = Utils.RandomPointInSphere(Vector3.zero, enemySpawnMaxDistanceFromCenter);

        bool clear = !Physics.CheckSphere(spawnPoint + checkSphereOffset, enemySphereCheckRadius * radiusMultiplier);

        if (clear)
            return spawnPoint;
        else
            return FindSpawnPoint(radiusMultiplier);
    }

    public void EnemyDefeated()
    {
        enemyAmount--;

        if (enemyAmount <= 0)
        {
            newWaveTimer.Reset(waves[currentWave].cooldown);
        }
    }
}
