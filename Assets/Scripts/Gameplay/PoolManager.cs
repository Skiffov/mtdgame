using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Queue<Enemy> enemyPool = new Queue<Enemy>(64);
        private readonly Queue<Projectile> projectilePool = new Queue<Projectile>(128);

        private Transform enemyRoot;
        private Transform projectileRoot;
        private Sprite enemySprite;
        private Sprite projectileSprite;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Sprite enemySpriteAsset, Sprite projectileSpriteAsset)
        {
            enemySprite = enemySpriteAsset;
            projectileSprite = projectileSpriteAsset;

            enemyRoot = new GameObject("EnemyPool").transform;
            enemyRoot.SetParent(transform);
            projectileRoot = new GameObject("ProjectilePool").transform;
            projectileRoot.SetParent(transform);

            PrewarmEnemies(60);
            PrewarmProjectiles(120);
        }

        private void PrewarmEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Enemy enemy = CreateEnemy();
                ReturnEnemy(enemy);
            }
        }

        private void PrewarmProjectiles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Projectile projectile = CreateProjectile();
                ReturnProjectile(projectile);
            }
        }

        private Enemy CreateEnemy()
        {
            var go = new GameObject("Enemy");
            go.transform.SetParent(enemyRoot);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = enemySprite;
            renderer.sortingOrder = 12;
            var enemy = go.AddComponent<Enemy>();
            return enemy;
        }

        private Projectile CreateProjectile()
        {
            var go = new GameObject("Projectile");
            go.transform.SetParent(projectileRoot);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = projectileSprite;
            renderer.sortingOrder = 15;
            var projectile = go.AddComponent<Projectile>();
            return projectile;
        }

        public Enemy GetEnemy()
        {
            Enemy enemy = enemyPool.Count > 0 ? enemyPool.Dequeue() : CreateEnemy();
            enemy.gameObject.SetActive(true);
            return enemy;
        }

        public void ReturnEnemy(Enemy enemy)
        {
            if (enemy == null) return;
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(enemyRoot);
            if (!enemyPool.Contains(enemy)) enemyPool.Enqueue(enemy);
        }

        public Projectile GetProjectile()
        {
            Projectile projectile = projectilePool.Count > 0 ? projectilePool.Dequeue() : CreateProjectile();
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public void ReturnProjectile(Projectile projectile)
        {
            if (projectile == null) return;
            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(projectileRoot);
            if (!projectilePool.Contains(projectile)) projectilePool.Enqueue(projectile);
        }
    }
}
