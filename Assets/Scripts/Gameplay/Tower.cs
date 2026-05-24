using UnityEngine;

namespace TowerDefenseMVP
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Tower : MonoBehaviour
    {
        private const int RangeSegments = 96;

        private TowerData data;
        private float cooldown;

        private SpriteRenderer spriteRenderer;
        private LineRenderer rangeLine;

        public TowerData Data => data;

        public void Initialize(TowerData towerData)
        {
            data = towerData;
            cooldown = 0f;

            spriteRenderer = GetComponent<SpriteRenderer>();

            if (data.sprite != null)
                spriteRenderer.sprite = data.sprite;

            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 10;

            transform.localScale = Vector3.one * 0.9f;

            CreateRangeIndicator();
        }

        private void Update()
        {
            UpdateRangeIndicatorVisibility();

            if (GameManager.Instance == null ||
                GameManager.Instance.State != GameState.Battle)
                return;

            if (data == null)
                return;

            cooldown -= Time.deltaTime;

            if (cooldown > 0f)
                return;

            Enemy target = FindTargetClosestToBase();

            if (target == null)
                return;

            Fire(target);

            cooldown = 1f / Mathf.Max(0.05f, data.fireRate);
        }

        private Enemy FindTargetClosestToBase()
        {
            Enemy bestTarget = null;
            float bestProgress = float.MinValue;

            float rangeSqr = data.range * data.range;

            for (int i = Enemy.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.ActiveEnemies[i];

                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distanceSqr =
                    (enemy.transform.position - transform.position).sqrMagnitude;

                if (distanceSqr > rangeSqr)
                    continue;

                if (enemy.PathProgress > bestProgress)
                {
                    bestProgress = enemy.PathProgress;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private void Fire(Enemy target)
        {
            Projectile projectile = PoolManager.Instance.GetProjectile();

            projectile.transform.position =
                transform.position + new Vector3(0f, 0.12f, 0f);

            projectile.Initialize(target, data);

            SoundManager.PlayClip(
                data.shootSound,
                data.towerName == "Cannon" ? 0.75f : 0.55f
            );
        }

        private void CreateRangeIndicator()
        {
            GameObject ringObject = new GameObject("Range Indicator");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = Vector3.zero;
            ringObject.transform.localScale = Vector3.one;

            rangeLine = ringObject.AddComponent<LineRenderer>();

            rangeLine.useWorldSpace = false;
            rangeLine.loop = true;
            rangeLine.positionCount = RangeSegments;
            rangeLine.widthMultiplier = 0.035f;
            rangeLine.sortingOrder = 50;

            Material material = new Material(Shader.Find("Sprites/Default"));
            material.color = new Color(0.25f, 0.85f, 1f, 0.85f);

            rangeLine.material = material;
            rangeLine.startColor = new Color(0.25f, 0.85f, 1f, 0.85f);
            rangeLine.endColor = new Color(0.25f, 0.85f, 1f, 0.85f);

            DrawRangeCircle();

            rangeLine.enabled = false;
        }

        private void DrawRangeCircle()
        {
            if (rangeLine == null || data == null)
                return;

            float parentScale = transform.localScale.x;

            if (Mathf.Abs(parentScale) < 0.001f)
                parentScale = 1f;

            float visualRadius = data.range * 0.72f;
            float correctedRadius = visualRadius / parentScale;

            for (int i = 0; i < RangeSegments; i++)
            {
                float angle = (i / (float)RangeSegments) * Mathf.PI * 2f;

                float x = Mathf.Cos(angle) * correctedRadius;
                float y = Mathf.Sin(angle) * correctedRadius;

                rangeLine.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        private void UpdateRangeIndicatorVisibility()
        {
            if (rangeLine == null || GameManager.Instance == null)
                return;

            rangeLine.enabled =
                GameManager.Instance.State == GameState.Preparation;
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null)
                return;

            Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }
}