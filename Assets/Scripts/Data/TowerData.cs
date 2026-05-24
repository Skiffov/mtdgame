using UnityEngine;

namespace TowerDefenseMVP
{
    [CreateAssetMenu(fileName = "TowerData", menuName = "Tower Defense/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("Identity")]
        public string towerName = "Tower";
        public Color color = Color.white;
        public Sprite sprite;
        public Sprite projectileSprite;
        public AudioClip shootSound;

        [Header("Economy")]
        public int cost = 100;

        [Header("Combat")]
        public TowerAttackType attackType = TowerAttackType.SingleTarget;
        public float range = 2.5f;
        public float fireRate = 1f;
        public float damage = 12f;
        public float projectileSpeed = 7f;

        [Header("Area / Slow")]
        public float splashRadius = 0f;
        [Range(0f, 0.95f)] public float slowPercent = 0f;
        public float slowDuration = 0f;
    }
}
