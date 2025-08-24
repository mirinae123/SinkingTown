using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 모든 해적을 관리하는 클래스
/// </summary>
public class PirateManager : SingletonBehaviour<PirateManager>
{
    private const int PENALTY_RANGE = 3;    // 육지와 가까운 타일을 인식할 범위

    [SerializeField] private GameObject _piratePrefab;
    [SerializeField] private GameObject _cannonballPrefab;

    private HashSet<Vector2Int> _targets = new HashSet<Vector2Int>();                       // 현재 해적이 공격하고 있는 타일
    private List<CannonballController> _cannonballs = new List<CannonballController>();     // 현재 게임에 존재하는 모든 대포알

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

    private void Start()
    {
        LoadSaveData(SaveManager.Instance.SaveData);
    }

    /// <summary>
    /// 해적을 생성한다.
    /// </summary>
    public bool SpawnPirate()
    {
        // 해적 목표 지정
        List<Vector2Int> targetCandidates = new List<Vector2Int>();

        foreach (Tile tile in MapManager.Instance.Tiles)
        {
            if (tile.Structure != null)
            {
                Vector2Int[] neighbors = HexaUtility.GetNeighbors(tile.Coordinate, 3);

                foreach (Vector2Int neighbor in neighbors)
                {
                    if (!_targets.Contains(neighbor) && MapManager.Instance.CheckCoordinateValidity(neighbor) && MapManager.Instance.Tiles[neighbor.x, neighbor.y].IsUnderWater && CheckOceanAccessibility(neighbor))
                    {
                        targetCandidates.Add(neighbor);
                    }
                }
            }
        }

        if (targetCandidates.Count == 0)
        {
            Debug.LogError("Couldn't find an attack target!");
            return false;
        }

        Vector2Int targetCoordinate = targetCandidates[Random.Range(0, targetCandidates.Count)];

        // 시작 위치 지정
        Vector2Int startCoordinate = Vector2Int.zero;

        do
        {
            int direction = Random.Range(0, 4);

            switch (direction)
            {
                case 0:
                    startCoordinate.x = 0;
                    startCoordinate.y = Random.Range(0, MapManager.Instance.Tiles.GetLength(1));
                    break;
                case 1:
                    startCoordinate.x = MapManager.Instance.Tiles.GetLength(0);
                    startCoordinate.y = Random.Range(0, MapManager.Instance.Tiles.GetLength(1));
                    break;
                case 2:
                    startCoordinate.x = Random.Range(0, MapManager.Instance.Tiles.GetLength(0));
                    startCoordinate.y = 0;
                    break;
                case 3:
                    startCoordinate.x = Random.Range(0, MapManager.Instance.Tiles.GetLength(0));
                    startCoordinate.y = MapManager.Instance.Tiles.GetLength(1);
                    break;
            }
        }
        while (!CheckFreeCoordinate(startCoordinate));

        // 해적 오브젝트 생성
        GameObject pirateObject = Instantiate(_piratePrefab);

        PirateController pirateController = pirateObject.GetComponent<PirateController>();
        _pirates.Add(pirateController);

        pirateController.Initialize(startCoordinate, targetCoordinate);
        _targets.Add(targetCoordinate);

        return true;
    }

    /// <summary>
    /// 해적을 제거한다.
    /// </summary>
    /// <param name="pirateController">제거할 해적</param>
    public void DespawnPirate(PirateController pirateController)
    {
        _pirates.Remove(pirateController);
        _targets.Remove(pirateController.TargetCoordinate);

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
    public void UpdatePenalties()
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

    /// <summary>
    /// 대포알을 스폰한다.
    /// </summary>
    /// <param name="startPosition">시작 위치</param>
    /// <param name="endPosition">종료 위치</param>
    /// <param name="duration">지속 시간</param>
    public void SpawnCannonball(Vector3 startPosition, Vector3 endPosition, PirateController targetPirate = null)
    {
        GameObject cannonball = Instantiate(_cannonballPrefab);
        cannonball.transform.position = startPosition;

        CannonballController cannonballController = cannonball.GetComponent<CannonballController>();
        cannonballController.Initialize(startPosition, endPosition, targetPirate);
        _cannonballs.Add(cannonballController);
    }

    /// <summary>
    /// 대포알을 제거한다.
    /// </summary>
    /// <param name="cannonballController">제거할 대포알</param>
    public void DespawnCannonball(CannonballController cannonballController)
    {
        _cannonballs.Remove(cannonballController);

        Destroy(cannonballController.gameObject);
    }

    /// <summary>
    /// 저장 데이터에 정보를 추가한다.
    /// </summary>
    /// <param name="saveData">저장 데이터</param>
    public void PopulateSaveData(SaveData saveData)
    {
        List<PirateSaveData> pirateList = new List<PirateSaveData>();
        List<CannonballSaveData> cannonballList = new List<CannonballSaveData>();

        foreach (PirateController pirateController in _pirates)
        {
            if (pirateController.CurrentState < PirateState.Despawn)
            {
                pirateList.Add(pirateController.GetSaveData());
            }
        }

        foreach (CannonballController cannonballController in _cannonballs)
        {
            cannonballList.Add(cannonballController.GetSaveData());
        }

        saveData.Pirates = pirateList.ToArray();
        saveData.Cannonballs = cannonballList.ToArray();
    }

    /// <summary>
    /// 저장 데이터로부터 정보를 불러온다.
    /// </summary>
    /// <param name="saveData">저장 데이터</param>
    public void LoadSaveData(SaveData saveData)
    {
        foreach (PirateSaveData pirateSaveData in saveData.Pirates)
        {
            GameObject pirateObject = Instantiate(_piratePrefab);

            PirateController pirateController = pirateObject.GetComponent<PirateController>();
            _pirates.Add(pirateController);

            pirateController.LoadSaveData(pirateSaveData);
            _targets.Add(pirateSaveData.TargetCoordinate);
        }

        foreach (CannonballSaveData cannonballSaveData in saveData.Cannonballs)
        {
            GameObject cannonballObject = Instantiate(_cannonballPrefab);

            CannonballController cannonballController = cannonballObject.GetComponent<CannonballController>();
            cannonballController.LoadSaveData(cannonballSaveData);
            _cannonballs.Add(cannonballController);
        }
    }
}
