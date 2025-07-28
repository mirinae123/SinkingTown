using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 모든 해적을 관리하는 클래스
/// </summary>
public class PirateManager : SingletonBehaviour<PirateManager>
{
    private const int PENALTY_RANGE = 3;    // 육지와 가까운 타일을 인식할 범위

    [SerializeField] private GameObject _piratePrefab;

    /// <summary>
    /// 현재 게임에 존재하는 모든 해적
    /// </summary>
    public IReadOnlyList<PirateController> Pirates
    {
        get => _pirates;
    }
    private List<PirateController> _pirates = new List<PirateController>();

    /// <summary>
    /// 육지와 가까운 타일에 부여할 이동 비용 페널티
    /// </summary>
    public int[,] Penalties
    {
        get => _penalties;
    }
    private int[,] _penalties;

    private void Update()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;

        // TEST
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdatePenalties();
            SpawnPirate();
        }
    }

    /// <summary>
    /// 해적을 생성한다.
    /// </summary>
    public void SpawnPirate()
    {
        // 시작 위치 지정
        Vector2Int spawnPosition = Vector2Int.zero;

        do
        {
            int direction = Random.Range(0, 4);

            switch (direction)
            {
                case 0:
                    spawnPosition.x = 0;
                    spawnPosition.y = Random.Range(0, MapManager.Instance.Tiles.GetLength(1));
                    break;
                case 1:
                    spawnPosition.x = MapManager.Instance.Tiles.GetLength(0);
                    spawnPosition.y = Random.Range(0, MapManager.Instance.Tiles.GetLength(1));
                    break;
                case 2:
                    spawnPosition.x = Random.Range(0, MapManager.Instance.Tiles.GetLength(0));
                    spawnPosition.y = 0;
                    break;
                case 3:
                    spawnPosition.x = Random.Range(0, MapManager.Instance.Tiles.GetLength(0));
                    spawnPosition.y = MapManager.Instance.Tiles.GetLength(1);
                    break;
            }
        }
        while (!CheckFreeCoordinate(spawnPosition));

        // 해적 오브젝트 생성
        GameObject pirateObject = Instantiate(_piratePrefab);

        PirateController pirateController = pirateObject.GetComponent<PirateController>();
        _pirates.Add(pirateController);

        // 해적 목표 지정
        Vector2Int[] neighbors = HexaUtility.GetNeighbors(new Vector2Int(MapManager.Instance.Tiles.GetLength(0) / 2, MapManager.Instance.Tiles.GetLength(1) / 2), 30);

        int n = neighbors.Length;

        while (n > 1)
        {
            n--;

            int k = Random.Range(0, n + 1);
            (neighbors[k], neighbors[n]) = (neighbors[n], neighbors[k]);
        }

        foreach (Vector2Int neighbor in neighbors)
        {
            if (MapManager.Instance.Tiles[neighbor.x, neighbor.y].IsUnderWater && CheckOceanAccessibility(neighbor))
            {
                pirateController.Initialize(spawnPosition, neighbor);
                break;
            }
        }
    }

    /// <summary>
    /// 해적을 제거한다.
    /// </summary>
    /// <param name="pirateController">제거할 해적</param>
    public void DespawnPirate(PirateController pirateController)
    {
        _pirates.Remove(pirateController);
        Destroy(pirateController.gameObject);
    }

    /// <summary>
    /// 해당 위치에 다른 해적이 없는지 확인한다.
    /// </summary>
    /// <param name="coordinate">위치</param>
    /// /// <param name="exception">제외할 해적</param>
    public bool CheckFreeCoordinate(Vector2Int coordinate, PirateController exception = null)
    {
        foreach (PirateController pirateController in _pirates)
        {
            if (pirateController == exception)
            {
                continue;
            }

            if (pirateController.CurrentCoordinate == coordinate)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 해당 위치에서 바다로 나갈 수 있는지 확인한다.
    /// </summary>
    /// <param name="coordinate">위치</param>
    private bool CheckOceanAccessibility(Vector2Int coordinate)
    {
        int w = MapManager.Instance.Tiles.GetLength(0);
        int h = MapManager.Instance.Tiles.GetLength(1);

        bool[,] visited = new bool[w, h];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(coordinate);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current.x == 0 || current.x == MapManager.Instance.Tiles.GetLength(0) - 1 ||
                current.y == 0 || current.y == MapManager.Instance.Tiles.GetLength(1) - 1)
            {
                return true;
            }

            if (visited[current.x, current.y])
            {
                continue;
            }

            visited[current.x, current.y] = true;

            for (int i = 0; i < 6; i++)
            {
                Vector2Int neighbor = HexaUtility.GetNeighbor(current, (TileNeighbor)i);

                if (MapManager.Instance.CheckCoordinateValidity(neighbor) && MapManager.Instance.Tiles[neighbor.x, neighbor.y].IsUnderWater && !visited[neighbor.x, neighbor.y])
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 이동 비용 페널티 정보를 갱신한다.
    /// </summary>
    private void UpdatePenalties()
    {
        int w = MapManager.Instance.Tiles.GetLength(0);
        int h = MapManager.Instance.Tiles.GetLength(1);

        _penalties = new int[w, h];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (!MapManager.Instance.Tiles[i, j].IsUnderWater)
                {
                    Vector2Int[] neighbors = HexaUtility.GetNeighbors(new Vector2Int(i, j), PENALTY_RANGE);

                    foreach (Vector2Int neighbor in neighbors)
                    {
                        _penalties[neighbor.x, neighbor.y]++;
                    }
                }
            }
        }
    }
}
