using UnityEngine;

namespace TowerDefenseMVP
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Tower Defense/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Enemy";
        public Color color = Color.white;
        public Sprite sprite;
        public AudioClip deathSound;

        [Header("Stats")]
        public float maxHealth = 40f;
        public float speed = 1.5f;
        public int baseDamage = 1;

        [Header("Economy")]
        public int attackCost = 10;
        public int rewardGold = 8;

        [Header("Special")]
        public bool immuneToSlow = false;
    }
}
