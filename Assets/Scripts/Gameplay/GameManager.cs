using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GridManager gridManager;
        private WaveSpawner waveSpawner;
        private WaveAI waveAI;
        private UIController uiController;
        private EnemyData[] enemyData;
        private TowerData[] towerData;

        private int gold;
        private int baseHp;
        private int currentRound;
        private int attackBudget;
        private string statusText = "Press Start Game.";
        private AudioClip baseHitSound;

        private bool isPaused;
        private GameState stateBeforePause;

        public GameState State { get; private set; } = GameState.Menu;
        public int Gold => gold;
        public int BaseHp => baseHp;
        public int CurrentRound => currentRound;
        public int MaxRounds { get; private set; } = 10;
        public int AttackBudget => attackBudget;
        public string StatusText => statusText;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Initialize(
            GridManager grid,
            WaveSpawner spawner,
            WaveAI ai,
            UIController ui,
            EnemyData[] enemies,
            TowerData[] towers)
        {
            gridManager = grid;
            waveSpawner = spawner;
            waveAI = ai;
            uiController = ui;
            enemyData = enemies;
            towerData = towers;

            gold = 300;
            baseHp = 20;
            currentRound = 1;
            attackBudget = 200;

            State = GameState.Menu;
            isPaused = false;
            stateBeforePause = GameState.Menu;

            statusText = "Press Start Game.";
            baseHitSound = Resources.Load<AudioClip>("Audio/BaseHit");

            Time.timeScale = 1f;

            uiController?.Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Menu || State == GameState.GameOver)
                {
                    uiController?.CloseSettings();
                }
                else
                {
                    TogglePause();
                }
            }

            if (!isPaused && State == GameState.Battle)
            {
                bool waveFinished =
                    waveSpawner != null &&
                    !waveSpawner.IsSpawning &&
                    Enemy.ActiveEnemies.Count == 0;

                if (waveFinished)
                    EndRound();
            }

            uiController?.Refresh();
        }

        public void SetStatus(string text)
        {
            statusText = text;
            uiController?.Refresh();
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            isPaused = false;

            gold = 300;
            baseHp = 20;
            currentRound = 1;
            attackBudget = 200;

            State = GameState.Preparation;
            stateBeforePause = State;

            SetStatus("Preparation: choose a unit and build outside the path.");
        }

        public void StartBattle()
        {
            if (State != GameState.Preparation || isPaused)
                return;

            if (waveAI == null || waveSpawner == null)
            {
                SetStatus("Wave system is not initialized.");
                return;
            }

            List<EnemyData> wave = waveAI.GenerateWave(currentRound, attackBudget);

            float interval = Mathf.Clamp(
                1.05f - currentRound * 0.035f,
                0.65f,
                1.05f
            );

            State = GameState.Battle;
            stateBeforePause = State;

            SetStatus($"Battle started. Wave size: {wave.Count}. Attack budget: {attackBudget}.");

            waveSpawner.StartWave(wave, interval);
        }

        public void EndRound()
        {
            if (State != GameState.Battle)
                return;

            if (baseHp <= 0)
            {
                GameOver(false);
                return;
            }

            if (currentRound >= MaxRounds)
            {
                GameOver(true);
                return;
            }

            State = GameState.RoundEnd;

            int roundBonus = 60 + currentRound * 10;

            gold += roundBonus;
            attackBudget += 55 + currentRound * 12;
            currentRound++;

            State = GameState.Preparation;
            stateBeforePause = State;

            SetStatus($"Round finished. Bonus gold: {roundBonus}. Prepare for round {currentRound}.");
        }

        public bool TrySpendGold(int amount)
        {
            if (State != GameState.Preparation || isPaused)
                return false;

            if (gold < amount)
                return false;

            gold -= amount;
            uiController?.Refresh();

            return true;
        }

        public void EnemyKilled(int reward)
        {
            gold += reward;
            uiController?.Refresh();
        }

        public void BaseHit(int damage)
        {
            if (State == GameState.GameOver)
                return;

            SoundManager.PlayClip(baseHitSound, 0.8f);

            baseHp -= damage;

            if (baseHp < 0)
                baseHp = 0;

            uiController?.Refresh();

            if (baseHp <= 0)
            {
                waveSpawner?.StopWave();
                GameOver(false);
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Menu || State == GameState.GameOver)
                return;

            if (isPaused)
                ResumeGame(true);
            else
                PauseGame();
        }

        public void PauseGame()
        {
            if (isPaused)
                return;

            if (State == GameState.Menu || State == GameState.GameOver)
                return;

            stateBeforePause = State;
            isPaused = true;

            Time.timeScale = 0f;

            SetStatus("Paused.");
        }

        public void ResumeGame(bool restoreStatus)
        {
            Time.timeScale = 1f;

            if (!isPaused)
                return;

            isPaused = false;

            if (State != GameState.GameOver && State != GameState.Menu)
                State = stateBeforePause;

            if (restoreStatus)
                SetStatus("Game resumed.");
            else
                uiController?.Refresh();
        }

        private void GameOver(bool defenderWon)
        {
            Time.timeScale = 1f;
            isPaused = false;

            State = GameState.GameOver;

            waveSpawner?.StopWave();

            SetStatus(
                defenderWon
                    ? "Victory: defender survived all rounds."
                    : "Defeat: base HP reached zero."
            );
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            isPaused = false;

            SetStatus("Restarting...");

            TDGameBootstrapper.RestartRuntime();
        }
    }
}