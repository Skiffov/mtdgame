using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefenseMVP
{
    public sealed class BuildManager : MonoBehaviour
    {
        private GridManager gridManager;
        private GameManager gameManager;
        private TowerData[] towerData;
        private Sprite towerSprite;
        private int selectedTowerIndex;
        private Transform towerRoot;
        private AudioClip buildSound;
        private SpriteRenderer hoverRenderer;

        public TowerData SelectedTower => towerData != null && towerData.Length > 0 ? towerData[selectedTowerIndex] : null;

        public void Initialize(GridManager grid, GameManager manager, TowerData[] towers, Sprite sprite)
        {
            gridManager = grid;
            gameManager = manager;
            towerData = towers;
            towerSprite = sprite;
            selectedTowerIndex = 0;
            towerRoot = new GameObject("Friendly Units").transform;
            towerRoot.SetParent(transform, false);
            buildSound = Resources.Load<AudioClip>("Audio/Build");
            CreateHoverPreview();
        }

        private void Update()
        {
            if (gameManager == null || gridManager == null) return;

            bool canInteract = gameManager.State == GameState.Preparation;
            UpdateHoverPreview(canInteract);

            if (!canInteract) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector3 mousePosition = Input.mousePosition;
            if (Camera.main != null)
                mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 world = Camera.main != null ? Camera.main.ScreenToWorldPoint(mousePosition) : Vector3.zero;
            world.z = 0f;
            Vector2Int cell = gridManager.WorldToCell(world);
            TryBuild(cell);
        }

        public void SelectTower(int index)
        {
            if (towerData == null || towerData.Length == 0) return;
            selectedTowerIndex = Mathf.Clamp(index, 0, towerData.Length - 1);
            gameManager?.SetStatus($"Selected {SelectedTower.towerName}. Place it on a free grass tile.");
        }

        private void TryBuild(Vector2Int cell)
        {
            TowerData selected = SelectedTower;
            if (selected == null) return;

            if (!gridManager.CanBuildAt(cell))
            {
                gameManager.SetStatus("Cannot build here: choose a free grass tile, not the road.");
                return;
            }

            if (!gameManager.TrySpendGold(selected.cost))
            {
                gameManager.SetStatus($"Not enough gold for {selected.towerName}.");
                return;
            }

            var towerObject = new GameObject(selected.towerName);
            towerObject.transform.SetParent(towerRoot, false);
            towerObject.transform.position = gridManager.CellToWorld(cell);
            var renderer = towerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = selected.sprite != null ? selected.sprite : towerSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 10;
            var tower = towerObject.AddComponent<Tower>();
            tower.Initialize(selected);
            gridManager.SetOccupied(cell);
            SoundManager.PlayClip(buildSound, 0.7f);
            gameManager.SetStatus($"Built {selected.towerName}. Gold left: {gameManager.Gold}.");
        }

        private void CreateHoverPreview()
        {
            var obj = new GameObject("Build Hover Preview");
            obj.transform.SetParent(transform, false);
            hoverRenderer = obj.AddComponent<SpriteRenderer>();
            hoverRenderer.sprite = SpriteFactory.CreateSquareSprite("HoverPreview", 100);
            hoverRenderer.sortingOrder = 50;
            hoverRenderer.color = new Color(0.3f, 1f, 0.35f, 0.25f);
            obj.SetActive(false);
        }

        private void UpdateHoverPreview(bool visible)
        {
            if (hoverRenderer == null) return;

            if (!visible || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() || Camera.main == null)
            {
                hoverRenderer.gameObject.SetActive(false);
                return;
            }

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(mousePosition);
            world.z = 0f;
            Vector2Int cell = gridManager.WorldToCell(world);

            if (!gridManager.IsInside(cell))
            {
                hoverRenderer.gameObject.SetActive(false);
                return;
            }

            hoverRenderer.gameObject.SetActive(true);
            hoverRenderer.transform.position = gridManager.CellToWorld(cell);
            hoverRenderer.transform.localScale = Vector3.one * 1.0f;
            hoverRenderer.color = gridManager.CanBuildAt(cell)
                ? new Color(0.3f, 1f, 0.35f, 0.25f)
                : new Color(1f, 0.2f, 0.15f, 0.25f);
        }
    }
}
