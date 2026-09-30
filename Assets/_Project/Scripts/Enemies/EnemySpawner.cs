using System.Collections.Generic;
using UnityEngine;

namespace KitchenInvasion {
    public class EnemySpawner : MonoBehaviour {
        [SerializeField] EnemyFactory factory;
        [SerializeField] PotPlacer potPlacer;
        [SerializeField] float minSpawnRadius = 0.25f;
        [SerializeField] float maxSpawnRadius = 0.5f;
        [SerializeField] float pepperChance = 0.35f;
        [SerializeField] int maxEnemies = 8;

        readonly List<Enemy> activeEnemies = new List<Enemy>();
        float spawnInterval = 2f;
        float speedMultiplier = 1f;
        float spawnTimer;
        bool isSpawning;

        void OnEnable() { GameEvents.OnEnemyKilled += HandleEnemyKilled; }

        void OnDisable() { GameEvents.OnEnemyKilled -= HandleEnemyKilled; }

        public void StartSpawning(DifficultySettings difficulty) {
            spawnInterval = difficulty.spawnInterval;
            speedMultiplier = difficulty.enemySpeedMultiplier;
            spawnTimer = 1f;
            isSpawning = true;
        }

        public void StopAndClear() {
            isSpawning = false;
            foreach (Enemy enemy in activeEnemies) {
                if (enemy != null) Destroy(enemy.gameObject);
            }
            activeEnemies.Clear();
        }

        void Update() {
            if (!isSpawning || !potPlacer.HasPot || activeEnemies.Count >= maxEnemies) return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f) {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }

        void SpawnEnemy() {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 position = potPlacer.Pot.position + offset;

            EnemyType type = Random.value < pepperChance ? EnemyType.Pepper : EnemyType.Tomato;
            activeEnemies.Add(factory.CreateEnemy(type, position, speedMultiplier));
            GameEvents.EnemySpawned();
        }

        void HandleEnemyKilled(Enemy enemy) { activeEnemies.Remove(enemy); }
    }
}
