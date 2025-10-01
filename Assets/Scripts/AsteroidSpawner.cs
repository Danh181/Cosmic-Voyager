using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public float baseSpawnRate = 5.0f;      // spawn ban đầu
    public int baseSpawnAmount = 2;          // số thiên thạch ban đầu
    public float minSpawnRate = 2.0f;        // spawnRate thấp nhất
    public int maxSpawnAmount = 10;          // số thiên thạch cao nhất


    public Asteroid asteroidPrefab; //prefab thiên thạch
    public float spawnDistance = 15.0f; // khoảng cách spawn tính từ vị trí của spawner.
    public float trajectoryVariance = 15.0f; // độ lệch hướng bay ban đầu của thiên thạch.

    public GameManager gameManager;

    private float currentSpawnRate;
    private float spawnTimer;

    private void Start()
    {
        currentSpawnRate = baseSpawnRate;
        spawnTimer = currentSpawnRate;
    }


    private void Update()
    {
        // thời gian sống
        float timeAlive = gameManager.ElapsedTime;

        // giảm spawnRate dần: mỗi 20s giảm 1s nhưng không nhỏ hơn minSpawnRate
        currentSpawnRate = Mathf.Max(minSpawnRate, baseSpawnRate - timeAlive / 40f);

        // tăng số lượng thiên thạch: mỗi 20s +1 không quá maxSpawnAmount
        int spawnAmount = Mathf.Min(maxSpawnAmount, baseSpawnAmount + Mathf.FloorToInt(timeAlive / 40f));

        // đếm ngược spawn
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            Spawn(spawnAmount);
            spawnTimer = currentSpawnRate;
        }
    }

    private void Spawn(int spawnAmount)
    {
        for(int i = 0; i < spawnAmount; i++)
        {
            Vector3 spawnDirection = Random.insideUnitCircle.normalized * spawnDistance;

            Vector3 spawnPoint = transform.position + spawnDirection;

            float variance = Random.Range(-trajectoryVariance, trajectoryVariance);

            Quaternion rotation = Quaternion.AngleAxis(variance, Vector3.forward);

            Asteroid asteroid = Instantiate(asteroidPrefab, spawnPoint, rotation);

            asteroid.size = Random.Range(asteroid.minSize, asteroid.maxSize);

            asteroid.SetTrajectory(rotation * -spawnDirection);
        }
    }
}
