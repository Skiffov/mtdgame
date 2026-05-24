using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class WaveSpawner : MonoBehaviour
    {
        private PoolManager poolManager;
        private GridManager gridManager;
        private Coroutine spawnRoutine;

        public bool IsSpawning { get; private set; }

        public void Initialize(PoolManager pool, GridManager grid)
        {
            poolManager = pool;
            gridManager = grid;
        }

        public void StartWave(List<EnemyData> wave, float spawnInterval)
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(SpawnWaveRoutine(wave, spawnInterval));
        }

        private IEnumerator SpawnWaveRoutine(List<EnemyData> wave, float spawnInterval)
        {
            IsSpawning = true;

            foreach (EnemyData enemyData in wave)
            {
                Enemy enemy = poolManager.GetEnemy();
                enemy.Initialize(enemyData, gridManager.Waypoints);
                yield return new WaitForSeconds(spawnInterval);
            }

            IsSpawning = false;
            spawnRoutine = null;
        }

        public void StopWave()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
            IsSpawning = false;
        }
    }
}
