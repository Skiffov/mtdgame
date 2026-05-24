using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class WaveAI : MonoBehaviour
    {
        private EnemyData[] enemies;
        private const int MaxEnemiesPerWave = 50;

        public void Initialize(EnemyData[] enemyData)
        {
            enemies = enemyData;
        }

        public List<EnemyData> GenerateWave(int round, int budget)
        {
            var wave = new List<EnemyData>(MaxEnemiesPerWave);
            int remainingBudget = budget;
            int safety = 0;

            while (remainingBudget >= CheapestEnemyCost() && wave.Count < MaxEnemiesPerWave && safety < 200)
            {
                safety++;
                EnemyData chosen = PickEnemy(round, remainingBudget);
                if (chosen == null || chosen.attackCost > remainingBudget) break;

                wave.Add(chosen);
                remainingBudget -= chosen.attackCost;
            }

            ShuffleSlightly(wave);
            return wave;
        }

        private int CheapestEnemyCost()
        {
            int cheapest = int.MaxValue;
            foreach (EnemyData enemy in enemies)
            {
                if (enemy.attackCost < cheapest) cheapest = enemy.attackCost;
            }
            return cheapest;
        }

        private EnemyData PickEnemy(int round, int remainingBudget)
        {
            EnemyData goblin = enemies[0];
            EnemyData orc = enemies[1];
            EnemyData ghost = enemies[2];

            float roll = Random.value;

            if (round <= 2)
            {
                return goblin.attackCost <= remainingBudget ? goblin : null;
            }

            if (round <= 5)
            {
                if (roll < 0.65f && goblin.attackCost <= remainingBudget) return goblin;
                if (orc.attackCost <= remainingBudget) return orc;
                return goblin.attackCost <= remainingBudget ? goblin : null;
            }

            if (roll < 0.45f && goblin.attackCost <= remainingBudget) return goblin;
            if (roll < 0.75f && ghost.attackCost <= remainingBudget) return ghost;
            if (orc.attackCost <= remainingBudget) return orc;
            if (ghost.attackCost <= remainingBudget) return ghost;
            return goblin.attackCost <= remainingBudget ? goblin : null;
        }

        private static void ShuffleSlightly(List<EnemyData> wave)
        {
            for (int i = 0; i < wave.Count; i += 3)
            {
                int swapIndex = Random.Range(i, Mathf.Min(i + 3, wave.Count));
                EnemyData temp = wave[i];
                wave[i] = wave[swapIndex];
                wave[swapIndex] = temp;
            }
        }
    }
}
