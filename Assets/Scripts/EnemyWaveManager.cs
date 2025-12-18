using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private Transform enemy1;
    [SerializeField] private Transform enemy2;
    [SerializeField] private Transform enemy3;
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
        public Vector2 enemy3Range;
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
            currentWave++;
            SpawnWave(waves[currentWave]);

            if (Utils.RandomBool())
                ObjectPoolManager.SpawnObject(GameManager.I.prefabs.newWaveSFX1);
            else
                ObjectPoolManager.SpawnObject(GameManager.I.prefabs.newWaveSFX2);
            
            GameManager.I.popupListHandler.ShowPopup(GameManager.I.popupListHandler.popup_DetectingSignals, true, 0.5f, 8f);
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
                Instantiate(enemy1, spawnPoints[i], Quaternion.identity);
            }
            
            for (int i = 0; i < enemy2Amount; i++)
            {
                Instantiate(enemy2, spawnPoints[enemy1Amount + i], Quaternion.identity);
            }
            
            for (int i = 0; i < enemy3Amount; i++)
            {
                Instantiate(enemy3, spawnPoints[enemy1Amount + enemy2Amount + i], Quaternion.identity);
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
            
            for (int i = 0; i < enemy3Amount; i++)
            {
                spawnPoint += enemySpawnOffset;
                Instantiate(enemy3, spawnPoint, Quaternion.identity);
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
