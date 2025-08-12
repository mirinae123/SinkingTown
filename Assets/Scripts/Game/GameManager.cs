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
    private const float OCEAN_RISE_PERIOD = 180.0f;
    private const float DAY_SPEED = 4.8f;
    private const int MAX_RESEARCH_POINT = 100;
    private const float RESEARCH_COOLDOWN = 60.0f;

    private const float PIRATE_SPAWN_PERIOD = 180.0f;
    private const int MAX_PIRATE_SPAWN_COUNT = 2;

    [SerializeField] private MonoBehaviour[] _gameStates;

    [SerializeField] private Animator _dayNightAnimator;

    private float _riseCooldown = OCEAN_RISE_PERIOD;
    private float _researchCooldown = 0.0f;

    private float _pirateSpawnCooldown = OCEAN_RISE_PERIOD;
    private int _pirateSpawnProbabilityIndex = 0;
    private int[] _pirateSpawnProbabilities = { 20, 60, 100 };

    private bool _hasEnded = false;

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
    /// 오늘 하루를 기준으로 현재 시간
    /// </summary>
    public float CurrentTime
    {
        get => _currentTime;
    }
    private float _currentTime = 480.0f;

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
    /// 현재 연구 포인트
    /// </summary>
    public int CurrentResearchPoint
    {
        get => _currentResearchPoint;
    }
    private int _currentResearchPoint = 0;

    /// <summary>
    /// 최대 연구 포인트
    /// </summary>
    public int MaxResearchPoint
    {
        get => MAX_RESEARCH_POINT;
    }

    /// <summary>
    /// 맵에 시청이 있는지 여부
    /// </summary>
    public bool HasTownHall
    {
        get => _hasTownHall;
        set => _hasTownHall = value;
    }
    private bool _hasTownHall = false;

    public bool IsResearchable
    {
        get => _isResearchable;
    }
    private bool _isResearchable = true;

    /// <summary>
    /// 시간을 갱신한다.
    /// </summary>
    public void UpdateTime()
    {
        if (_isPaused)
        {
            return;
        }

        _currentTime += Time.deltaTime * DAY_SPEED;

        if (_currentTime > 1440.0f)
        {
            _currentTime -= 1440.0f;
            _currentDay = _currentDay % 99 + 1;
        }

        _dayNightAnimator.Play("Cycle", 0, _currentTime / 1440.0f);

        // 연구 쿨타임 처리
        if (!_isResearchable)
        {
            _researchCooldown += Time.deltaTime;

            if (_researchCooldown >= RESEARCH_COOLDOWN)
            {
                _isResearchable = true;
            }
        }

        // 해적 스폰 처리
        _pirateSpawnCooldown -= Time.deltaTime;

        if (_pirateSpawnCooldown < 0.0f)
        {
            _pirateSpawnCooldown += PIRATE_SPAWN_PERIOD;

            if (Random.Range(1, 101) < _pirateSpawnProbabilities[_pirateSpawnProbabilityIndex])
            {
                int spawnCount = Random.Range(0, MAX_PIRATE_SPAWN_COUNT) + 1;

                for (int i = 0; i < spawnCount; i++)
                {
                    PirateManager.Instance.SpawnPirate();
                }

                _pirateSpawnProbabilityIndex = 0;
            }
            else
            {
                _pirateSpawnProbabilityIndex = Mathf.Min(_pirateSpawnProbabilityIndex + 1, _pirateSpawnProbabilities.Length - 1);
            }
        }
    }

    /// <summary>
    /// 해수면 상승을 처리한다.
    /// </summary>
    public void ProcessOceanRise()
    {
        if (_isPaused || MapManager.Instance.OceanLevel == MapGenerator.Instance.MaxHeight)
        {
            return;
        }

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
        _gameState = gameState;

        _gameStates[0].enabled = (int)gameState == 0 ? true : false;
        _gameStates[1].enabled = (int)gameState == 1 ? true : false;
        _gameStates[2].enabled = (int)gameState == 2 ? true : false;

        if (gameState == GameState.Build)
        {
            (_gameStates[1] as BuildState).SetStructureToBuild((StructureType)structure);
        }
    }

    /// <summary>
    /// 현재 연구 포인트를 amount 값만큼 변경한다.
    /// </summary>
    /// <param name="amount">변경할 값</param>
    public void ChangeResearchPoint(int amount)
    {
        _currentResearchPoint = Mathf.Clamp(_currentResearchPoint + amount, 0, MAX_RESEARCH_POINT);

        if (_currentResearchPoint == MAX_RESEARCH_POINT)
        {
            EndGame(true);
        }
    }

    public void StartResearchCooldown()
    {
        _isResearchable = false;
        _researchCooldown = 0.0f;
    }

    public void EndGame(bool hasCleared)
    {
        if (_hasEnded)
        {
            return;
        }

        _hasEnded = true;
        UIManager.Instance.ShowEndMenu(hasCleared);
    }
}
