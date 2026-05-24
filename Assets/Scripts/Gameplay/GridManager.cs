using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseMVP
{
    public sealed class GridManager : MonoBehaviour
    {
        public const int Width = 12;
        public const int Height = 8;
        public const float CellSize = 1f;

        private readonly HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        private readonly List<Vector3> waypoints = new List<Vector3>();

        private Vector3 origin;
        private Transform tileRoot;

        public IReadOnlyList<Vector3> Waypoints => waypoints;

        public void Initialize(Sprite tileSprite, Sprite pathSprite)
        {
            origin = new Vector3(-Width * CellSize / 2f, -Height * CellSize / 2f, 0f);
            tileRoot = new GameObject("Grid").transform;
            tileRoot.SetParent(transform, false);

            pathCells.Clear();
            occupiedCells.Clear();
            waypoints.Clear();

            BuildPath();
            DrawTiles(tileSprite, pathSprite);
            SetupCamera();
        }

        private void BuildPath()
        {
            int[,] cells =
            {
                {0,4}, {1,4}, {2,4}, {3,4},
                {3,5}, {4,5}, {5,5}, {6,5},
                {6,4}, {7,4}, {8,4}, {8,3},
                {9,3}, {10,3}, {11,3}
            };

            for (int i = 0; i < cells.GetLength(0); i++)
            {
                var cell = new Vector2Int(cells[i, 0], cells[i, 1]);
                pathCells.Add(cell);
                waypoints.Add(CellToWorld(cell));
            }
        }

        private void DrawTiles(Sprite tileSprite, Sprite pathSprite)
        {
            if (tileRoot != null)
            {
                for (int i = tileRoot.childCount - 1; i >= 0; i--)
                    Destroy(tileRoot.GetChild(i).gameObject);
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    bool isPath = pathCells.Contains(cell);
                    var tile = new GameObject(isPath ? $"Path_{x}_{y}" : $"Tile_{x}_{y}");
                    tile.transform.SetParent(tileRoot, false);
                    tile.transform.position = CellToWorld(cell);
                    tile.transform.localScale = Vector3.one * 1.01f;

                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = isPath ? pathSprite : tileSprite;
                    renderer.color = Color.white;
                    renderer.sortingOrder = isPath ? 1 : 0;
                }
            }
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            cam.orthographic = true;
            cam.orthographicSize = 4.75f;
            cam.transform.position = new Vector3(0f, 0.05f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.055f, 0.065f, 0.075f, 1f);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            return origin + new Vector3((cell.x + 0.5f) * CellSize, (cell.y + 0.5f) * CellSize, 0f);
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - origin.x) / CellSize);
            int y = Mathf.FloorToInt((worldPosition.y - origin.y) / CellSize);
            return new Vector2Int(x, y);
        }

        public bool IsInside(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
        }

        public bool IsPath(Vector2Int cell)
        {
            return pathCells.Contains(cell);
        }

        public bool IsOccupied(Vector2Int cell)
        {
            return occupiedCells.Contains(cell);
        }

        public bool CanBuildAt(Vector2Int cell)
        {
            return IsInside(cell) && !IsPath(cell) && !occupiedCells.Contains(cell);
        }

        public void SetOccupied(Vector2Int cell)
        {
            if (IsInside(cell)) occupiedCells.Add(cell);
        }
    }
}
