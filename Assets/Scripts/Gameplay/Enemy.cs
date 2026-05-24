using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> ActiveEnemies = new List<Enemy>(64);

        private EnemyData data;
        private IReadOnlyList<Vector3> waypoints;
        private int waypointIndex;

        private float health;
        private float slowTimer;
        private float slowMultiplier = 1f;
        private float flashTimer;

        private SpriteRenderer spriteRenderer;

        private Transform healthBarRoot;
        private SpriteRenderer healthBarBack;
        private SpriteRenderer healthBarFill;

        public bool IsAlive => gameObject.activeSelf && health > 0f;
        public EnemyData Data => data;

        public float PathProgress
        {
            get
            {
                if (waypoints == null || waypoints.Count == 0)
                    return 0f;

                if (waypointIndex >= waypoints.Count)
                    return waypoints.Count;

                int previousIndex = Mathf.Max(waypointIndex - 1, 0);

                Vector3 from = waypoints[previousIndex];
                Vector3 to = waypoints[waypointIndex];

                float segmentLength = Vector3.Distance(from, to);

                if (segmentLength <= 0.001f)
                    return waypointIndex;

                float remaining = Vector3.Distance(transform.position, to);

                float segmentProgress =
                    Mathf.Clamp01(1f - remaining / segmentLength);

                return waypointIndex + segmentProgress;
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            CreateHealthBar();
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        public void Initialize(
            EnemyData enemyData,
            IReadOnlyList<Vector3> path)
        {
            data = enemyData;
            waypoints = path;

            waypointIndex = 0;

            health = data.maxHealth;

            slowTimer = 0f;
            slowMultiplier = 1f;
            flashTimer = 0f;

            transform.position = waypoints[0];

            transform.localScale =
                Vector3.one * GetVisualScale(data.enemyName);

            spriteRenderer.sprite =
                data.sprite != null
                    ? data.sprite
                    : spriteRenderer.sprite;

            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 11;

            UpdateHealthBar();

            if (!ActiveEnemies.Contains(this))
                ActiveEnemies.Add(this);
        }

        private static float GetVisualScale(string enemyName)
        {
            if (enemyName == "Orc")
                return 1.05f;

            if (enemyName == "Ghost")
                return 0.92f;

            return 0.88f;
        }

        private void Update()
        {
            if (data == null ||
                waypoints == null ||
                waypointIndex >= waypoints.Count)
                return;

            UpdateSlowTimer();
            UpdateFlash();
            MoveAlongPath();

            if (healthBarRoot != null)
            {
                healthBarRoot.position =
                    transform.position + new Vector3(0f, 0.72f, 0f);
            }
        }

        private void UpdateSlowTimer()
        {
            if (slowTimer <= 0f)
                return;

            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0f)
                slowMultiplier = 1f;
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f)
                return;

            flashTimer -= Time.deltaTime;

            spriteRenderer.color =
                flashTimer > 0f
                    ? new Color(1f, 0.72f, 0.72f, 1f)
                    : Color.white;
        }

        private void MoveAlongPath()
        {
            Vector3 target = waypoints[waypointIndex];

            float currentSpeed = data.speed * slowMultiplier;

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                currentSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, target) <= 0.03f)
            {
                waypointIndex++;

                if (waypointIndex >= waypoints.Count)
                {
                    GameManager.Instance.BaseHit(data.baseDamage);

                    PoolManager.Instance.ReturnEnemy(this);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
                return;
        
            health -= damage;
        
            flashTimer = 0.08f;
        
            UpdateHealthBar();
        
            if (health <= 0f)
            {
                Die();
            }
        }

        public void ApplySlow(float multiplier, float duration)
        {
            if (data.enemyName == "Ghost")
                return;

            slowMultiplier = multiplier;
            slowTimer = duration;
        }

        private void Die()
        {
            SoundManager.PlayClip(data.deathSound, 0.7f);
        
            GameManager.Instance.EnemyKilled(data.rewardGold);
        
            PoolManager.Instance.ReturnEnemy(this);
        }

        private void CreateHealthBar()
        {
            healthBarRoot = new GameObject("HP Bar").transform;
            healthBarRoot.SetParent(transform, false);

            var backObj = new GameObject("Back");
            backObj.transform.SetParent(healthBarRoot, false);

            healthBarBack =
                backObj.AddComponent<SpriteRenderer>();

            healthBarBack.sprite =
                SpriteFactory.CreateSquareSprite("EnemyHPBack", 32);

            healthBarBack.color =
                new Color(0.08f, 0.08f, 0.08f, 0.92f);

            healthBarBack.sortingOrder = 200;

            backObj.transform.localScale =
                new Vector3(0.62f, 0.10f, 1f);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthBarRoot, false);

            healthBarFill =
                fillObj.AddComponent<SpriteRenderer>();

            healthBarFill.sprite =
                SpriteFactory.CreateSquareSprite("EnemyHPFill", 32);

            healthBarFill.color =
                new Color(0.18f, 0.95f, 0.22f, 1f);

            healthBarFill.sortingOrder = 201;

            fillObj.transform.localPosition =
                new Vector3(0f, 0f, -0.01f);
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null || data == null)
                return;

            float ratio =
                Mathf.Clamp01(health / data.maxHealth);

            healthBarFill.transform.localScale =
                new Vector3(0.58f * ratio, 0.075f, 1f);

            healthBarFill.transform.localPosition =
                new Vector3(
                    (-0.58f + 0.58f * ratio) * 0.5f,
                    0f,
                    -0.01f
                );

            if (ratio > 0.55f)
                healthBarFill.color =
                    new Color(0.18f, 0.95f, 0.22f);

            else if (ratio > 0.25f)
                healthBarFill.color =
                    new Color(1f, 0.72f, 0.12f);

            else
                healthBarFill.color =
                    new Color(1f, 0.22f, 0.18f);
        }
    }
}
