using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] public int width = 10;
    [SerializeField] public int height = 10;
    [SerializeField] public float cellSize = 1f;

    private Dictionary<Vector2Int, CellType> grid = new Dictionary<Vector2Int, CellType>();

    public enum CellType
    {
        Empty,
        Obstacle,
        Player,
        Enemy
    }

    // 为每种CellType定义颜色
    private static readonly Color emptyColor = Color.white;
    private static readonly Color obstacleColor = Color.black;
    private static readonly Color playerColor = Color.blue;
    private static readonly Color enemyColor = Color.red;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[new Vector2Int(x, y)] = CellType.Empty;
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    public void SetCellType(Vector2Int gridPosition, CellType type)
    {
        if (IsValidGridPosition(gridPosition))
        {
            grid[gridPosition] = type;
        }
    }

    public CellType GetCellType(Vector2Int gridPosition)
    {
        if (IsValidGridPosition(gridPosition))
        {
            return grid[gridPosition];
        }
        return CellType.Obstacle; // Treat out-of-bounds as obstacles
    }

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < width &&
               gridPosition.y >= 0 && gridPosition.y < height;
    }

    public bool CanMoveTo(Vector2Int gridPosition)
    {
        return IsValidGridPosition(gridPosition) && GetCellType(gridPosition) == CellType.Empty;
    }

    // Editor method to set up the grid
    public void SetupGrid(CellType[,] setupGrid)
    {
        if (setupGrid.GetLength(0) != width || setupGrid.GetLength(1) != height)
        {
            Debug.LogError("Setup grid dimensions do not match the current grid size.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[new Vector2Int(x, y)] = setupGrid[x, y];
            }
        }
    }

    public void MoveEntity(Vector2Int fromPosition, Vector2Int toPosition, CellType entityType)
    {
        if (IsValidGridPosition(fromPosition) && IsValidGridPosition(toPosition))
        {
            SetCellType(fromPosition, CellType.Empty);
            SetCellType(toPosition, entityType);
        }
        else
        {
            Debug.LogError($"Invalid move from {fromPosition} to {toPosition}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // 如果不在游戏运行状态，初始化网格
            InitializeGrid();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorldPosition(gridPos);

                // 绘制单元格
                Gizmos.color = GetColorForCellType(GetCellType(gridPos));
                Gizmos.DrawCube(worldPos + new Vector3(cellSize / 2, cellSize / 2, 0), Vector3.one * cellSize * 0.9f);

                // 绘制网格线
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(worldPos + new Vector3(cellSize / 2, cellSize / 2, 0), Vector3.one * cellSize);
            }
        }
    }

    private Color GetColorForCellType(CellType type)
    {
        switch (type)
        {
            case CellType.Empty:
                return emptyColor;
            case CellType.Obstacle:
                return obstacleColor;
            case CellType.Player:
                return playerColor;
            case CellType.Enemy:
                return enemyColor;
            default:
                return Color.magenta; // 用于未知类型
        }
    }
}