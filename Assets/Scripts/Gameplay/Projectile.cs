using UnityEngine;

namespace TowerDefenseMVP
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Projectile : MonoBehaviour
    {
        private Enemy target;
        private TowerData sourceTower;
        private SpriteRenderer spriteRenderer;
        private float lifeTimer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Enemy enemyTarget, TowerData towerData)
        {
            target = enemyTarget;
            sourceTower = towerData;
            lifeTimer = 3.5f;
            transform.localScale = Vector3.one * GetScale(towerData.towerName);
            spriteRenderer.sprite = towerData.projectileSprite != null ? towerData.projectileSprite : spriteRenderer.sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 20;
            gameObject.SetActive(true);
        }

        private static float GetScale(string towerName)
        {
            if (towerName == "Cannon") return 0.85f;
            if (towerName == "Mage") return 0.72f;
            return 0.64f;
        }

        private void Update()
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f || target == null || !target.IsAlive)
            {
                PoolManager.Instance.ReturnProjectile(this);
                return;
            }

            Vector3 previous = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, sourceTower.projectileSpeed * Time.deltaTime);
            Vector3 direction = transform.position - previous;
            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (Vector3.Distance(transform.position, target.transform.position) <= 0.10f)
                Hit();
        }

        private void Hit()
        {
            switch (sourceTower.attackType)
            {
                case TowerAttackType.AreaDamage:
                    ApplyAreaDamage();
                    break;
                case TowerAttackType.Slow:
                    target.TakeDamage(sourceTower.damage);
                    target.ApplySlow(sourceTower.slowPercent, sourceTower.slowDuration);
                    break;
                default:
                    target.TakeDamage(sourceTower.damage);
                    break;
            }

            PoolManager.Instance.ReturnProjectile(this);
        }

        private void ApplyAreaDamage()
        {
            Vector3 center = target.transform.position;
            float radiusSqr = sourceTower.splashRadius * sourceTower.splashRadius;

            for (int i = Enemy.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                float distanceSqr = (enemy.transform.position - center).sqrMagnitude;
                if (distanceSqr <= radiusSqr)
                    enemy.TakeDamage(sourceTower.damage);
            }
        }
    }
}
