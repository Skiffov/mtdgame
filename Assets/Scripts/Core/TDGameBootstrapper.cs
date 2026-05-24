using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefenseMVP
{
    public sealed class TDGameBootstrapper : MonoBehaviour
    {
        private static bool bootstrapped;
        private static TDGameBootstrapper current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Bootstrap()
        {
            if (bootstrapped && current != null)
                return;

            bootstrapped = true;

            GameObject root = new GameObject("TD_MVP_Runtime");
            current = root.AddComponent<TDGameBootstrapper>();
        }

        public static void ResetBootstrapState()
        {
            bootstrapped = false;
            current = null;
            Enemy.ActiveEnemies.Clear();
        }

        public static void RestartRuntime()
        {
            GameObject runnerObject = new GameObject("TD_Restart_Runner");
            RestartRunner runner = runnerObject.AddComponent<RestartRunner>();
            runner.StartRestart();
        }

        private sealed class RestartRunner : MonoBehaviour
        {
            public void StartRestart()
            {
                StartCoroutine(RestartRoutine());
            }

            private IEnumerator RestartRoutine()
            {
                Time.timeScale = 1f;

                Enemy.ActiveEnemies.Clear();

                DestroyAll<Enemy>();
                DestroyAll<Projectile>();
                DestroyAll<Tower>();
                DestroyAll<Canvas>();

                GameObject runtime = GameObject.Find("TD_MVP_Runtime");

                if (runtime != null)
                    Destroy(runtime);

                ResetBootstrapState();

                yield return null;

                Bootstrap();

                Destroy(gameObject);
            }

            private static void DestroyAll<T>() where T : Object
            {
                T[] objects = FindObjectsByType<T>(FindObjectsSortMode.None);

                foreach (T obj in objects)
                {
                    if (obj is Component component)
                    {
                        Destroy(component.gameObject);
                    }
                    else if (obj is GameObject go)
                    {
                        Destroy(go);
                    }
                    else
                    {
                        Destroy(obj);
                    }
                }
            }
        }

        private void Awake()
        {
            current = this;

            EnsureCamera();
            EnsureEventSystem();

            gameObject.AddComponent<SoundManager>();
            gameObject.AddComponent<MusicManager>();

            Sprite tileSprite = LoadSpriteOrFallback(
                "Sprites/Tiles/Grass",
                () => SpriteFactory.CreateSquareSprite("TileSprite")
            );

            Sprite pathSprite = LoadSpriteOrFallback(
                "Sprites/Tiles/Path",
                () => SpriteFactory.CreateSquareSprite("PathSprite")
            );

            Sprite towerSprite = LoadSpriteOrFallback(
                "Sprites/Towers/Archer",
                () => SpriteFactory.CreateCircleSprite("TowerSprite")
            );

            Sprite enemySprite = LoadSpriteOrFallback(
                "Sprites/Enemies/Goblin",
                () => SpriteFactory.CreateCircleSprite("EnemySprite")
            );

            Sprite projectileSprite = LoadSpriteOrFallback(
                "Sprites/Projectiles/Arrow",
                () => SpriteFactory.CreateCircleSprite("ProjectileSprite", 32)
            );

            TowerData[] towerData = CreateDefaultTowers();
            EnemyData[] enemyData = CreateDefaultEnemies();

            PoolManager poolManager = gameObject.AddComponent<PoolManager>();
            poolManager.Initialize(enemySprite, projectileSprite);

            GridManager gridManager = gameObject.AddComponent<GridManager>();
            gridManager.Initialize(tileSprite, pathSprite);

            WaveSpawner waveSpawner = gameObject.AddComponent<WaveSpawner>();
            waveSpawner.Initialize(poolManager, gridManager);

            WaveAI waveAI = gameObject.AddComponent<WaveAI>();
            waveAI.Initialize(enemyData);

            GameManager gameManager = gameObject.AddComponent<GameManager>();
            BuildManager buildManager = gameObject.AddComponent<BuildManager>();
            UIController uiController = gameObject.AddComponent<UIController>();

            gameManager.Initialize(
                gridManager,
                waveSpawner,
                waveAI,
                uiController,
                enemyData,
                towerData
            );

            buildManager.Initialize(
                gridManager,
                gameManager,
                towerData,
                towerSprite
            );

            uiController.Initialize(
                gameManager,
                buildManager,
                towerData
            );
        }

        private void OnDestroy()
        {
            if (current == this)
                current = null;
        }

        private static Sprite LoadSpriteOrFallback(
            string resourcePath,
            System.Func<Sprite> fallbackFactory)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);

            if (sprite != null)
            {
                sprite.texture.filterMode = FilterMode.Point;
                sprite.texture.wrapMode = TextureWrapMode.Clamp;
                return sprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);

            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }

            Sprite fallback = fallbackFactory();

            if (fallback != null && fallback.texture != null)
            {
                fallback.texture.filterMode = FilterMode.Point;
                fallback.texture.wrapMode = TextureWrapMode.Clamp;
            }

            return fallback;
        }

        private static AudioClip LoadAudio(string resourcePath)
        {
            return Resources.Load<AudioClip>(resourcePath);
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = 4.75f;
                Camera.main.transform.position = new Vector3(0f, 0f, -10f);
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.055f, 0.065f, 0.075f);
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 4.75f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.075f);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static TowerData[] CreateDefaultTowers()
        {
            TowerData archer = ScriptableObject.CreateInstance<TowerData>();
            archer.towerName = "Archer";
            archer.cost = 100;
            archer.attackType = TowerAttackType.SingleTarget;
            archer.range = 2.8f;
            archer.fireRate = 1.15f;
            archer.damage = 18f;
            archer.projectileSpeed = 8f;
            archer.color = new Color(0.20f, 0.70f, 0.25f);
            archer.sprite = LoadSpriteOrFallback("Sprites/Towers/Archer", () => SpriteFactory.CreateCircleSprite("ArcherFallback"));
            archer.projectileSprite = LoadSpriteOrFallback("Sprites/Projectiles/Arrow", () => SpriteFactory.CreateCircleSprite("ArrowFallback", 32));
            archer.shootSound = LoadAudio("Audio/TowerShoot");

            TowerData mage = ScriptableObject.CreateInstance<TowerData>();
            mage.towerName = "Mage";
            mage.cost = 150;
            mage.attackType = TowerAttackType.AreaDamage;
            mage.range = 2.2f;
            mage.fireRate = 0.55f;
            mage.damage = 22f;
            mage.projectileSpeed = 6f;
            mage.splashRadius = 1.1f;
            mage.color = new Color(0.55f, 0.20f, 0.95f);
            mage.sprite = LoadSpriteOrFallback("Sprites/Towers/Mage", () => SpriteFactory.CreateCircleSprite("MageFallback"));
            mage.projectileSprite = LoadSpriteOrFallback("Sprites/Projectiles/Magic", () => SpriteFactory.CreateCircleSprite("MagicFallback", 32));
            mage.shootSound = LoadAudio("Audio/TowerShoot");

            TowerData freezer = ScriptableObject.CreateInstance<TowerData>();
            freezer.towerName = "Freezer";
            freezer.cost = 120;
            freezer.attackType = TowerAttackType.Slow;
            freezer.range = 2.7f;
            freezer.fireRate = 0.9f;
            freezer.damage = 4f;
            freezer.projectileSpeed = 7f;
            freezer.slowPercent = 0.45f;
            freezer.slowDuration = 2.4f;
            freezer.color = new Color(0.25f, 0.75f, 1.00f);
            freezer.sprite = LoadSpriteOrFallback("Sprites/Towers/Freezer", () => SpriteFactory.CreateCircleSprite("FreezerFallback"));
            freezer.projectileSprite = LoadSpriteOrFallback("Sprites/Projectiles/Ice", () => SpriteFactory.CreateCircleSprite("IceFallback", 32));
            freezer.shootSound = LoadAudio("Audio/TowerShoot");

            TowerData cannon = ScriptableObject.CreateInstance<TowerData>();
            cannon.towerName = "Cannon";
            cannon.cost = 200;
            cannon.attackType = TowerAttackType.SingleTarget;
            cannon.range = 3.6f;
            cannon.fireRate = 0.35f;
            cannon.damage = 55f;
            cannon.projectileSpeed = 5.5f;
            cannon.color = new Color(0.85f, 0.45f, 0.15f);
            cannon.sprite = LoadSpriteOrFallback("Sprites/Towers/Cannon", () => SpriteFactory.CreateCircleSprite("CannonFallback"));
            cannon.projectileSprite = LoadSpriteOrFallback("Sprites/Projectiles/Cannonball", () => SpriteFactory.CreateCircleSprite("CannonballFallback", 32));
            cannon.shootSound = LoadAudio("Audio/CannonShoot");

            return new[] { archer, mage, freezer, cannon };
        }

        private static EnemyData[] CreateDefaultEnemies()
        {
            EnemyData goblin = ScriptableObject.CreateInstance<EnemyData>();
            goblin.enemyName = "Goblin";
            goblin.maxHealth = 28f;
            goblin.speed = 2.2f;
            goblin.attackCost = 10;
            goblin.rewardGold = 8;
            goblin.color = new Color(0.35f, 0.95f, 0.25f);
            goblin.sprite = LoadSpriteOrFallback("Sprites/Enemies/Goblin", () => SpriteFactory.CreateCircleSprite("GoblinFallback"));
            goblin.deathSound = LoadAudio("Audio/EnemyDeath");

            EnemyData orc = ScriptableObject.CreateInstance<EnemyData>();
            orc.enemyName = "Orc";
            orc.maxHealth = 95f;
            orc.speed = 1.05f;
            orc.attackCost = 25;
            orc.rewardGold = 18;
            orc.color = new Color(0.85f, 0.18f, 0.12f);
            orc.sprite = LoadSpriteOrFallback("Sprites/Enemies/Orc", () => SpriteFactory.CreateCircleSprite("OrcFallback"));
            orc.deathSound = LoadAudio("Audio/EnemyDeath");

            EnemyData ghost = ScriptableObject.CreateInstance<EnemyData>();
            ghost.enemyName = "Ghost";
            ghost.maxHealth = 55f;
            ghost.speed = 1.55f;
            ghost.attackCost = 20;
            ghost.rewardGold = 14;
            ghost.immuneToSlow = true;
            ghost.color = new Color(0.80f, 0.80f, 1.00f, 0.80f);
            ghost.sprite = LoadSpriteOrFallback("Sprites/Enemies/Ghost", () => SpriteFactory.CreateCircleSprite("GhostFallback"));
            ghost.deathSound = LoadAudio("Audio/EnemyDeath");

            return new[] { goblin, orc, ghost };
        }
    }
}