using UnityEngine;
using UnityEngine.UI;

namespace TowerDefenseMVP
{
    public sealed class UIController : MonoBehaviour
    {
        private GameManager gameManager;
        private BuildManager buildManager;
        private TowerData[] towers;

        private Font cachedFont;

        private Text hudText;
        private Text selectedTowerText;
        private Text statusText;
        private Text gameOverText;

        private Button startBattleButton;
        private Button restartButton;
        private Button pauseButton;

        private GameObject towerPanel;
        private GameObject menuOverlay;
        private GameObject pauseOverlay;
        private GameObject settingsOverlay;
        private GameObject gameOverOverlay;

        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle musicMuteToggle;
        private Toggle sfxMuteToggle;

        public void Initialize(GameManager manager, BuildManager builder, TowerData[] towerData)
        {
            gameManager = manager;
            buildManager = builder;
            towers = towerData;

            BuildCanvas();
            Refresh();
        }

        public void CloseSettings()
        {
            if (settingsOverlay != null)
                settingsOverlay.SetActive(false);
        }

        private Font GetUIFont()
        {
            if (cachedFont != null)
                return cachedFont;

            cachedFont = Resources.Load<Font>("Fonts/UI_Font");

            if (cachedFont != null)
                return cachedFont;

            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return cachedFont;
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas");

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = GetUIFont();

            BuildHud(canvasObject.transform, font);
            BuildSidePanel(canvasObject.transform, font);
            BuildBottomControls(canvasObject.transform, font);
            BuildMenuOverlay(canvasObject.transform, font);
            BuildPauseOverlay(canvasObject.transform, font);
            BuildSettingsOverlay(canvasObject.transform, font);
            BuildGameOverOverlay(canvasObject.transform, font);
        }

        private void BuildHud(Transform parent, Font font)
        {
            GameObject hudPanel = CreatePanel(parent, "HUDPanel", Anchor.TopLeft, new Vector2(20, -20), new Vector2(320, 215), new Color(0.02f, 0.025f, 0.03f, 0.88f));
            CreateBorder(hudPanel.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            Text hudTitle = CreateTextInPanel(hudPanel.transform, "HUDTitle", font, 18, TextAnchor.UpperLeft, new RectOffset(16, 16, 12, 0), 30);
            hudTitle.text = "BATTLE STATUS";
            hudTitle.color = new Color(1f, 0.78f, 0.30f, 1f);

            hudText = CreateTextInPanel(hudPanel.transform, "HUDText", font, 15, TextAnchor.UpperLeft, new RectOffset(16, 16, 46, 12), 150);
        }

        private void BuildSidePanel(Transform parent, Font font)
        {
            GameObject selectedPanel = CreatePanel(parent, "SelectedPanel", Anchor.TopRight, new Vector2(-20, -20), new Vector2(320, 128), new Color(0.02f, 0.025f, 0.03f, 0.88f));
            CreateBorder(selectedPanel.transform, new Color(0.55f, 0.55f, 0.60f, 1f));

            selectedTowerText = CreateTextInPanel(selectedPanel.transform, "SelectedTowerText", font, 14, TextAnchor.UpperLeft, new RectOffset(16, 16, 14, 12), 98);

            towerPanel = CreatePanel(parent, "TowerPanel", Anchor.TopRight, new Vector2(-20, -162), new Vector2(320, 252), new Color(0.02f, 0.025f, 0.03f, 0.88f));
            CreateBorder(towerPanel.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            Text towerTitle = CreateTextInPanel(towerPanel.transform, "TowerTitle", font, 18, TextAnchor.UpperLeft, new RectOffset(16, 16, 12, 0), 30);
            towerTitle.text = "UNITS";
            towerTitle.color = new Color(1f, 0.78f, 0.30f, 1f);

            for (int i = 0; i < towers.Length; i++)
            {
                int index = i;

                CreateButtonInPanel(
                    towerPanel.transform,
                    $"{towers[i].towerName}  ({towers[i].cost})",
                    font,
                    new Vector2(16, -52 - i * 44),
                    new Vector2(288, 36),
                    () =>
                    {
                        buildManager.SelectTower(index);
                        gameManager.SetStatus($"Selected {towers[index].towerName}.");
                    }
                );
            }
        }

        private void BuildBottomControls(Transform parent, Font font)
        {
            GameObject statusPanel = CreatePanel(parent, "StatusPanel", Anchor.BottomLeft, new Vector2(20, 20), new Vector2(760, 52), new Color(0.02f, 0.025f, 0.03f, 0.88f));
            CreateBorder(statusPanel.transform, new Color(0.35f, 0.35f, 0.40f, 1f));

            statusText = CreateTextInPanel(statusPanel.transform, "StatusText", font, 14, TextAnchor.MiddleLeft, new RectOffset(16, 16, 0, 0), 52);

            startBattleButton = CreateButton(parent, "START BATTLE", font, Anchor.BottomRight, new Vector2(-20, 20), new Vector2(190, 52), () =>
            {
                if (gameManager.State == GameState.Menu)
                    gameManager.StartGame();
                else
                    gameManager.StartBattle();
            });

            pauseButton = CreateButton(parent, "PAUSE", font, Anchor.BottomRight, new Vector2(-220, 20), new Vector2(150, 52), gameManager.TogglePause);
            restartButton = CreateButton(parent, "RESTART", font, Anchor.BottomRight, new Vector2(-20, 84), new Vector2(190, 52), gameManager.Restart);
        }

        private void BuildMenuOverlay(Transform parent, Font font)
        {
            menuOverlay = CreatePanel(parent, "MenuOverlay", Anchor.Center, Vector2.zero, new Vector2(580, 315), new Color(0.02f, 0.025f, 0.03f, 0.94f));
            CreateBorder(menuOverlay.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            Text title = CreateTextInPanel(menuOverlay.transform, "MenuTitle", font, 28, TextAnchor.MiddleCenter, new RectOffset(24, 24, 28, 0), 60);
            title.text = "MEDIEVAL DEFENSE";
            title.color = new Color(1f, 0.78f, 0.30f, 1f);

            Text subtitle = CreateTextInPanel(menuOverlay.transform, "MenuSubtitle", font, 15, TextAnchor.MiddleCenter, new RectOffset(40, 40, 96, 0), 52);
            subtitle.text = "Build units. Hold the road.\nSurvive all waves.";

            CreateButtonInPanel(menuOverlay.transform, "START GAME", font, new Vector2(170, -180), new Vector2(240, 52), gameManager.StartGame);
            CreateButtonInPanel(menuOverlay.transform, "SETTINGS", font, new Vector2(170, -242), new Vector2(240, 44), OpenSettings);
        }

        private void BuildPauseOverlay(Transform parent, Font font)
        {
            pauseOverlay = CreatePanel(parent, "PauseOverlay", Anchor.Center, Vector2.zero, new Vector2(430, 305), new Color(0.02f, 0.025f, 0.03f, 0.94f));
            CreateBorder(pauseOverlay.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            Text title = CreateTextInPanel(pauseOverlay.transform, "PauseTitle", font, 26, TextAnchor.MiddleCenter, new RectOffset(24, 24, 24, 0), 56);
            title.text = "PAUSED";
            title.color = new Color(1f, 0.78f, 0.30f, 1f);

            CreateButtonInPanel(pauseOverlay.transform, "RESUME", font, new Vector2(115, -96), new Vector2(200, 46), () => gameManager.ResumeGame(true));
            CreateButtonInPanel(pauseOverlay.transform, "SETTINGS", font, new Vector2(115, -150), new Vector2(200, 46), OpenSettings);
            CreateButtonInPanel(pauseOverlay.transform, "RESTART", font, new Vector2(115, -204), new Vector2(200, 46), gameManager.Restart);
        }

        private void BuildSettingsOverlay(Transform parent, Font font)
        {
            settingsOverlay = CreatePanel(parent, "SettingsOverlay", Anchor.Center, Vector2.zero, new Vector2(520, 330), new Color(0.02f, 0.025f, 0.03f, 0.97f));
            CreateBorder(settingsOverlay.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            Text title = CreateTextInPanel(settingsOverlay.transform, "SettingsTitle", font, 24, TextAnchor.MiddleCenter, new RectOffset(24, 24, 22, 0), 54);
            title.text = "SETTINGS";
            title.color = new Color(1f, 0.78f, 0.30f, 1f);

            CreateTextInPanel(settingsOverlay.transform, "MusicLabel", font, 15, TextAnchor.UpperLeft, new RectOffset(50, 50, 88, 0), 24).text = "Music volume";
            musicSlider = CreateSliderInPanel(settingsOverlay.transform, new Vector2(50, -122), new Vector2(320, 24), MusicManager.Instance != null ? MusicManager.Instance.MusicVolume : 0.35f, value => MusicManager.SetMusicVolume(value));
            musicMuteToggle = CreateToggleInPanel(settingsOverlay.transform, "Mute", font, new Vector2(385, -113), MusicManager.Instance != null && MusicManager.Instance.MusicMuted, value => MusicManager.SetMusicMuted(value));

            CreateTextInPanel(settingsOverlay.transform, "SfxLabel", font, 15, TextAnchor.UpperLeft, new RectOffset(50, 50, 162, 0), 24).text = "SFX volume";
            sfxSlider = CreateSliderInPanel(settingsOverlay.transform, new Vector2(50, -196), new Vector2(320, 24), SoundManager.Instance != null ? SoundManager.Instance.SfxVolume : 0.65f, value => SoundManager.SetSfxVolume(value));
            sfxMuteToggle = CreateToggleInPanel(settingsOverlay.transform, "Mute", font, new Vector2(385, -187), SoundManager.Instance != null && SoundManager.Instance.SfxMuted, value => SoundManager.SetSfxMuted(value));

            CreateButtonInPanel(settingsOverlay.transform, "CLOSE", font, new Vector2(160, -260), new Vector2(200, 46), CloseSettings);

            settingsOverlay.SetActive(false);
        }

        private void BuildGameOverOverlay(Transform parent, Font font)
        {
            gameOverOverlay = CreatePanel(parent, "GameOverOverlay", Anchor.Center, Vector2.zero, new Vector2(520, 230), new Color(0.02f, 0.025f, 0.03f, 0.94f));
            CreateBorder(gameOverOverlay.transform, new Color(0.85f, 0.62f, 0.28f, 1f));

            gameOverText = CreateTextInPanel(gameOverOverlay.transform, "GameOverText", font, 24, TextAnchor.MiddleCenter, new RectOffset(24, 24, 38, 0), 90);
            CreateButtonInPanel(gameOverOverlay.transform, "RESTART", font, new Vector2(150, -158), new Vector2(220, 52), gameManager.Restart);
        }

        private void OpenSettings()
        {
            if (settingsOverlay == null)
                return;

            if (musicSlider != null && MusicManager.Instance != null)
                musicSlider.value = MusicManager.Instance.MusicVolume;

            if (sfxSlider != null && SoundManager.Instance != null)
                sfxSlider.value = SoundManager.Instance.SfxVolume;

            if (musicMuteToggle != null && MusicManager.Instance != null)
                musicMuteToggle.isOn = MusicManager.Instance.MusicMuted;

            if (sfxMuteToggle != null && SoundManager.Instance != null)
                sfxMuteToggle.isOn = SoundManager.Instance.SfxMuted;

            settingsOverlay.SetActive(true);
        }

        private enum Anchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        private static void ApplyAnchor(RectTransform rect, Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case Anchor.TopRight:
                    rect.anchorMin = new Vector2(1f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case Anchor.BottomLeft:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case Anchor.BottomRight:
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    break;
                case Anchor.Center:
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Anchor anchor, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            ApplyAnchor(rect, anchor);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        private static void CreateBorder(Transform parent, Color color)
        {
            Outline outline = parent.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static Text CreateTextInPanel(Transform parent, string name, Font font, int fontSize, TextAnchor alignment, RectOffset padding, float height)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(padding.left, -padding.top - height);
            rect.offsetMax = new Vector2(-padding.right, -padding.top);

            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;

            return text;
        }

        private static Button CreateButton(Transform parent, string label, Font font, Anchor anchor, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            ApplyAnchor(rect, anchor);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            return SetupButton(go, label, font, onClick);
        }

        private static Button CreateButtonInPanel(Transform parent, string label, Font font, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            return SetupButton(go, label, font, onClick);
        }

        private static Button SetupButton(GameObject go, string label, Font font, UnityEngine.Events.UnityAction onClick)
        {
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.09f, 0.10f, 0.12f, 0.96f);

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.09f, 0.10f, 0.12f, 0.96f);
            colors.highlightedColor = new Color(0.18f, 0.16f, 0.11f, 1f);
            colors.pressedColor = new Color(0.45f, 0.32f, 0.16f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(go.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            text.raycastTarget = false;

            return button;
        }

        private static Slider CreateSliderInPanel(Transform parent, Vector2 anchoredPosition, Vector2 size, float value, UnityEngine.Events.UnityAction<float> onChanged)
        {
            GameObject go = new GameObject("Slider");
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Slider slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(value);

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.09f, 1f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.85f, 0.62f, 0.28f, 1f);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(go.transform, false);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18, 28);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            slider.targetGraphic = handleImage;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.onValueChanged.AddListener(onChanged);

            return slider;
        }

        private static Toggle CreateToggleInPanel(Transform parent, string label, Font font, Vector2 anchoredPosition, bool value, UnityEngine.Events.UnityAction<bool> onChanged)
        {
            GameObject go = new GameObject(label + "Toggle");
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(100, 32);

            Toggle toggle = go.AddComponent<Toggle>();

            GameObject box = new GameObject("Box");
            box.transform.SetParent(go.transform, false);
            RectTransform boxRect = box.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(24, 24);
            Image boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.10f, 0.10f, 0.12f, 1f);

            GameObject check = new GameObject("Checkmark");
            check.transform.SetParent(box.transform, false);
            RectTransform checkRect = check.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.pivot = new Vector2(0.5f, 0.5f);
            checkRect.sizeDelta = new Vector2(14, 14);
            Image checkImage = check.AddComponent<Image>();
            checkImage.color = new Color(0.85f, 0.62f, 0.28f, 1f);

            Text text = CreateTextInPanel(go.transform, "Label", font, 14, TextAnchor.MiddleLeft, new RectOffset(34, 0, 0, 0), 32);
            text.text = label;

            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(onChanged);

            return toggle;
        }

        public void Refresh()
        {
            if (gameManager == null || hudText == null)
                return;

            hudText.text =
                $"State: {(gameManager.IsPaused ? "Paused" : gameManager.State.ToString())}\n" +
                $"Wave: {gameManager.CurrentRound}/{gameManager.MaxRounds}\n" +
                $"Gold: {gameManager.Gold}\n" +
                $"Base HP: {gameManager.BaseHp}\n" +
                $"Threat: {gameManager.AttackBudget}\n" +
                $"Enemies: {Enemy.ActiveEnemies.Count}";

            statusText.text = string.IsNullOrWhiteSpace(gameManager.StatusText) ? "Ready." : gameManager.StatusText;

            if (buildManager != null && buildManager.SelectedTower != null)
            {
                TowerData selected = buildManager.SelectedTower;
                selectedTowerText.text =
                    $"SELECTED UNIT\n" +
                    $"{selected.towerName}\n" +
                    $"Cost: {selected.cost}\n" +
                    $"Range: {selected.range:0.0}\n" +
                    $"Damage: {selected.damage:0}";
            }
            else
            {
                selectedTowerText.text = "SELECTED UNIT\nNone";
            }

            bool menu = gameManager.State == GameState.Menu;
            bool prep = gameManager.State == GameState.Preparation;
            bool battle = gameManager.State == GameState.Battle;
            bool gameOver = gameManager.State == GameState.GameOver;

            menuOverlay.SetActive(menu && !settingsOverlay.activeSelf);
            pauseOverlay.SetActive(gameManager.IsPaused && !settingsOverlay.activeSelf);
            gameOverOverlay.SetActive(gameOver && !settingsOverlay.activeSelf);
            towerPanel.SetActive(prep && !gameManager.IsPaused);

            startBattleButton.gameObject.SetActive(prep && !gameManager.IsPaused);
            pauseButton.gameObject.SetActive(!menu && !gameOver);
            pauseButton.GetComponentInChildren<Text>().text = gameManager.IsPaused ? "RESUME" : "PAUSE";
            restartButton.gameObject.SetActive(!menu && !battle);

            if (gameOver)
            {
                gameOverText.text = gameManager.BaseHp > 0
                    ? "VICTORY\nThe base survived."
                    : "DEFEAT\nThe base has fallen.";
            }
        }
    }
}
