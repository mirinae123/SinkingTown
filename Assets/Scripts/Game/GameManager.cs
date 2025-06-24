using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    [SerializeField] private MonoBehaviour[] _gameStates;

    [SerializeField] private Transform _dirLightTransform;
    [SerializeField] private Light _dirLightColor;

    private float _elapsedTime = 0f;
    private float _riseCooldown = OCEAN_RISE_PERIOD;

    private Camera _mainCamera;
    private Vector2 _mousePosition;
    private bool _isPointerOverGameObject = false;

    /// <summary>
    /// 현재 게임 상태
    /// </summary>
    public GameState GameState
    {
        get => _gameState;
    }
    private GameState _gameState = GameState.None;

    // 정지 여부
    public bool IsPaused
    {
        get => _isPaused;
        set => _isPaused = value;
    }
    private bool _isPaused = false;

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
    /// 맵에 시청이 있는지 여부
    /// </summary>
    public bool HasTownHall
    {
        get => _hasTownHall;
        set => _hasTownHall = value;
    }
    private bool _hasTownHall = false;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        // 게임 진행 조건: 정지 상태가 아닐 것, 메뉴가 열려 있지 않을 것
        if (!_isPaused && _gameState != GameState.Menu)
        {
            UpdateTime();
            ProcessOceanRise();
        }

        _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 시간을 갱신한다.
    /// </summary>
    public void UpdateTime()
    {
        _elapsedTime += Time.deltaTime;

        _currentDay = (Mathf.RoundToInt(_elapsedTime * DAY_SPEED) + 200) / 1440 + 1;
        float dayTime = (Mathf.RoundToInt(_elapsedTime * DAY_SPEED) + 200) % 1440;
        float sunRotation = Mathf.Lerp(0, 360, dayTime / 1440f);
    }

    /// <summary>
    /// 해수면 상승을 처리한다.
    /// </summary>
    public void ProcessOceanRise()
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
    /// 게임 상태를 변경한다.
    /// </summary>
    /// <param name="gameState">게임 상태</param>
    /// <param name="structure">Build 상태인 경우 대상 건물</param>
    public void ChangeGameState(GameState gameState, StructureType? structure = null)
    {
        _gameStates[0].enabled = (int)gameState == 0 ? true : false;
        _gameStates[1].enabled = (int)gameState == 1 ? true : false;
        _gameStates[2].enabled = (int)gameState == 2 ? true : false;

        if (gameState == GameState.Build)
        {
            (_gameStates[1] as BuildState).SetStructureToBuild((StructureType)structure);
        }
    }
}
