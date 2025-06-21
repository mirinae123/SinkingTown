using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState { None, Build, Menu }

/// <summary>
/// 게임 플레이를 관리하는 클래스
/// </summary>
public class GameManager : SingletonBehaviour<GameManager>
{
    private const float OCEAN_RISE_PERIOD = 300f;
    private const float DAY_SPEED = 9.6f;

    [SerializeField] private Transform _dirLightTransform;
    [SerializeField] private Light _dirLightColor;

    private float _elapsedTime = 0f;
    private float _riseCooldown = OCEAN_RISE_PERIOD;

    private bool _isPaused = false;

    /// <summary>
    /// 현재 게임 상태
    /// </summary>
    public GameState GameState
    {
        get => _gameState;
        set => _gameState = value;
    }
    private GameState _gameState = GameState.None;

    /// <summary>
    /// 현재 날짜
    /// </summary>
    public int CurrentDay
    {
        get => _currentDay;
    }
    private int _currentDay = 1;

    /// <summary>
    /// 해수면 상승으로부터 지난 시간
    /// </summary>
    public float TimeSinceOceanRise
    {
        get => _timeSinceOceanRise;
    }
    private float _timeSinceOceanRise = 0;

    /// <summary>
    /// 해수면 상승 주기
    /// </summary>
    public float OceanRisePeriod
    {
        get => _oceanRisePeriod;
    }
    private float _oceanRisePeriod = OCEAN_RISE_PERIOD;

    /// <summary>
    /// 목재 소지량
    /// </summary>
    public int CurrentWoods
    {
        get => _currentWoods;
        set => _currentWoods = value;
    }
    [SerializeField] private int _currentWoods = 999;

    /// <summary>
    /// 석재 소지량
    /// </summary>
    public int CurrentStones
    {
        get => _currentStones;
        set => _currentStones = value;
    }
    [SerializeField] private int _currentStones = 999;

    /// <summary>
    /// 건설하려고 하는 건물
    /// </summary>
    public StructureType StructureToBuild
    {
        get => _structureToBuild;
        set => _structureToBuild = value;
    }
    private StructureType _structureToBuild;

    /// <summary>
    /// 맵에 시청이 있는지 여부
    /// </summary>
    public bool HasTownHall
    {
        get => _hasTownHall;
        set => _hasTownHall = value;
    }
    private bool _hasTownHall = false;

    void Update()
    {
        // 게임 진행 조건: 정지 상태가 아닐 것, 메뉴가 열려 있지 않을 것
        if (!_isPaused && _gameState != GameState.Menu)
        {
            UpdateTime();
            ProcessOceanRise();
        }

        switch (_gameState)
        {
            case GameState.None:
                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            Vector2Int tilePos = HexaUtility.GetTileCoordinate(hit.point);
                            UIManager.Instance.ShowTileInfo(tilePos);
                        }
                    }
                };
                break;

            case GameState.Build:
                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            Vector2Int coordinate = HexaUtility.GetTileCoordinate(hit.point);
                            Tile tile = MapManager.Instance.Tiles[coordinate.x, coordinate.y];

                            if (CheckStructureValidity(tile, _structureToBuild))
                            {
                                tile.CreateStructure(_structureToBuild);
                                _gameState = GameState.None;
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 시간을 갱신한다.
    /// </summary>
    private void UpdateTime()
    {
        _elapsedTime += Time.deltaTime;

        _currentDay = (Mathf.RoundToInt(_elapsedTime * DAY_SPEED) + 200) / 1440 + 1;
        float dayTime = (Mathf.RoundToInt(_elapsedTime * DAY_SPEED) + 200) % 1440;
        float sunRotation = Mathf.Lerp(0, 360, dayTime / 1440f);
    }

    /// <summary>
    /// 해수면 상승을 처리한다.
    /// </summary>
    private void ProcessOceanRise()
    {
        _riseCooldown -= Time.deltaTime;

        if (_riseCooldown < 0.0f)
        {
            _riseCooldown += OCEAN_RISE_PERIOD;
            MapManager.Instance.RaiseOceanLevel();
        }

        _timeSinceOceanRise = OceanRisePeriod - _riseCooldown;
    }

    /// <summary>
    /// 타일에 건물을 지을 수 있는지 확인한다.
    /// </summary>
    /// <param name="tile">타일</param>
    /// <param name="structureType">건물 종류</param>
    /// <returns>지을 수 있는지 여부</returns>
    private bool CheckStructureValidity(Tile tile, StructureType structureType)
    {
        if (tile.Structure != null)
        {
            return false;
        }

        if (tile.IsUnderWater && !tile.IsDecked)
        {
            return structureType == StructureType.Deck && !tile.IsDecked;
        }

        switch (structureType)
        {
            case StructureType.Deck:
                return false;
            case StructureType.Pier:
            case StructureType.Dock:
                foreach (Tile neighbor in tile.GetNeighbors(1))
                {
                    if (neighbor.IsUnderWater && !neighbor.IsDecked)
                    {
                        return true;
                    }
                }
                return false;
            case StructureType.Farm:
                return !tile.IsDecked && tile.IsFertile;
            case StructureType.HydroponicsFarm:
                return tile.IsDecked;
            case StructureType.LumberCamp:
                return !tile.IsDecked && tile.NaturalResource == NaturalResourceType.Woods;
            case StructureType.Quarry:
                return !tile.IsDecked && tile.NaturalResource == NaturalResourceType.Stone;
            default:
                return true;

        }
    }
}
