using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PirateState { Move, FirstAttack, SecondAttack, ThirdAttack, Despawn, Destroy }

/// <summary>
/// 개별 해적을 관리하는 클래스
/// </summary>
public class PirateController : MonoBehaviour
{
    private const float MOVE_SPEED = 2.0f;
    private const float ROTATION_SPEED = 2.0f;

    private const int COLLISION_FACTOR = 32;

    private const float ATTACK_INTERVAL = 2.0f;
    private const int MAX_HEALTH = 3;

    private const float SPAWN_ANIMATION_DURATION = 3.0f;
    private const float DESPAWN_ANIMATION_DELAY = 3.0f;
    private const float DESPAWN_ANIMATION_DURATION = 3.0f;
    private const float DESTROY_ANIMATION_DURATION = 3.0f;

    /// <summary>
    /// 현재 위치 좌표
    /// </summary>
    public Vector2Int CurrentCoordinate
    {
        get => _currentCoordinate;
    }
    private Vector2Int _currentCoordinate;

    /// <summary>
    /// 공격 위치 좌표
    /// </summary>
    public Vector2Int TargetCoordinate
    {
        get => _targetCoordinate;
    }
    private Vector2Int _targetCoordinate;

    /// <summary>
    /// 해적이 공격받고 있는지 여부
    /// </summary>
    public bool IsUnderAttack
    {
        get => _attackingFortressCount > 0;
    }

    /// <summary>
    /// 해적의 현재 상태
    /// </summary>
    public PirateState CurrentState
    {
        get => _currentState;
    }
    private PirateState _currentState;

    private int _attackingFortressCount = 0;

    private int _currentHealth = MAX_HEALTH;
    private float _elapsed = 0.0f;

    private Queue<Vector2Int> _path = new Queue<Vector2Int>();

    private void Update()
    {
        if (GameManager.Instance.IsPaused || GameManager.Instance.GameState == GameState.Menu)
        {
            return;
        }

        // 해수면의 위치에 따라 해적의 위치도 변경
        Vector3 newPosition = transform.position;
        newPosition.y = MapRenderer.Instance.OceanHeight;
        transform.position = newPosition;

        // 체력이 0이 된 경우 파괴
        if (_currentState < PirateState.Despawn && _currentHealth <= 0)
        {
            _currentState = PirateState.Destroy;
            _path.Clear();

            StartCoroutine(CoDestroyPirate());
        }

        // 현재 상태에 따라 로직 수행
        switch (_currentState)
        {
            case PirateState.Move:
                {
                    // 이동할 경로가 남은 경우
                    if (_path.Count > 0)
                    {
                        float distanceToMove = Time.deltaTime * MOVE_SPEED;

                        while (distanceToMove > 0.0f && _path.Count > 0)
                        {
                            Vector3 targetPosition = HexaUtility.GetWorldCoordinate(_path.Peek());
                            targetPosition.y = MapRenderer.Instance.OceanHeight;
                            Vector3 forward = targetPosition - transform.position;

                            if (forward != Vector3.zero)
                            {
                                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * ROTATION_SPEED);
                            }

                            float moveDelta = Vector3.Distance(transform.position, targetPosition);

                            if (distanceToMove >= moveDelta)
                            {
                                distanceToMove -= moveDelta;
                                transform.position = targetPosition;

                                _currentCoordinate = _path.Dequeue();

                                if (_path.Count > 0 && !PirateManager.Instance.CheckFreeCoordinate(_path.Peek(), this))
                                {
                                    GetPath();
                                    break;
                                }
                            }
                            else
                            {
                                transform.position = Vector3.MoveTowards(transform.position, targetPosition, distanceToMove);
                                break;
                            }
                        }
                    }
                    // 목표에 도달한 경우
                    else
                    {
                        _currentState++;
                        _elapsed = 0.0f;
                    }
                    break;
                }
            case PirateState.FirstAttack:
            case PirateState.SecondAttack:
            case PirateState.ThirdAttack:
                {
                    _elapsed += Time.deltaTime;

                    if (_elapsed > ATTACK_INTERVAL)
                    {
                        _elapsed -= ATTACK_INTERVAL;
                        _currentState++;

                        // 공격 위치 탐색
                        Vector3 startPosition = transform.position;
                        Vector2Int endCoordinate = new Vector2Int();
                        Vector3 endPosition = new Vector3();

                        List<Vector2Int> neighborsWithStructure = new List<Vector2Int>();
                        List<Vector2Int> neighborsWithoutStructure = new List<Vector2Int>();
                        List<Vector2Int> neighborsUnderWater = new List<Vector2Int>();

                        foreach (Vector2Int neighbor in HexaUtility.GetNeighbors(_currentCoordinate, 4))
                        {
                            if (MapManager.Instance.CheckCoordinateValidity(neighbor))
                            {
                                if (MapManager.Instance.Tiles[neighbor.x, neighbor.y].Structure != null)
                                {
                                    neighborsWithStructure.Add(neighbor);
                                }
                                else if (!MapManager.Instance.Tiles[neighbor.x, neighbor.y].IsUnderWater)
                                {
                                    neighborsWithoutStructure.Add(neighbor);
                                }
                                else
                                {
                                    neighborsUnderWater.Add(neighbor);
                                }
                            }
                        }

                        if (neighborsWithStructure.Count > 0)
                        {
                            endCoordinate = neighborsWithStructure[Random.Range(0, neighborsWithStructure.Count)];
                        }
                        else if (neighborsWithoutStructure.Count > 0)
                        {
                            endCoordinate = neighborsWithoutStructure[Random.Range(0, neighborsWithoutStructure.Count)];
                        }
                        else
                        {
                            endCoordinate = neighborsUnderWater[Random.Range(0, neighborsUnderWater.Count)];
                        }

                        endPosition = HexaUtility.GetWorldCoordinate(endCoordinate);
                        endPosition.y = Mathf.Max(MapManager.Instance.Tiles[endCoordinate.x, endCoordinate.y].Height, MapRenderer.Instance.OceanHeight);

                        PirateManager.Instance.SpawnCannonball(startPosition, endPosition);
                    }

                    // 공격을 마친 경우
                    if (_currentState == PirateState.Despawn)
                    {
                        GameManager.Instance.ChangeResearchPoint(-10);

                        StartCoroutine(CoDespawnPirate());
                    }

                    break;
                }
        }
    }

    /// <summary>
    /// 해적의 상태를 초기화한다
    /// </summary>
    /// <param name="startCoordinate">시작 위치 좌표</param>
    /// <param name="targetCoordinate">공격 위치 좌표</param>
    public void Initialize(Vector2Int startCoordinate, Vector2Int targetCoordinate, bool playSpawnAnimation = true)
    {
        _currentCoordinate = startCoordinate;
        _targetCoordinate = targetCoordinate;

        _currentState = PirateState.Move;

        GetPath();

        if (_path.Count > 0)
        {
            Vector3 currentPosition = HexaUtility.GetWorldCoordinate(_currentCoordinate);
            currentPosition.y = MapRenderer.Instance.OceanHeight;
            transform.position = currentPosition;

            Vector3 targetPosition = HexaUtility.GetWorldCoordinate(_path.Peek());
            targetPosition.y = MapRenderer.Instance.OceanHeight;
            Vector3 forward = targetPosition - transform.position;

            if (forward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        if (playSpawnAnimation)
        {
            StartCoroutine(CoSpawnPirate());
        }
    }

    /// <summary>
    /// 경로 상에서 나중에 접근할 예정인 타일을 반환한다.
    /// </summary>
    /// <param name="futureIndex">경로 인덱스 (0: 바로 다음 타일)</param>
    public Vector2Int GetFutureCoordinate(int futureIndex)
    {
        Vector2Int[] futureCoordinates = _path.ToArray();

        if (_path.Count == 0)
        {
            return _currentCoordinate;
        }
        else
        {
            return futureCoordinates[Mathf.Min(futureIndex, futureCoordinates.Length - 1)];
        }
    }

    /// <summary>
    /// 현재 해적을 상대로 공격을 시작한다.
    /// </summary>
    public void StartAttackPirate()
    {
        _attackingFortressCount++;
    }

    /// <summary>
    /// 현재 해적을 상대로 공격을 중단한다.
    /// </summary>
    public void EndAttackPirate()
    {
        _attackingFortressCount--;
        _currentHealth--;
    }

    /// <summary>
    /// 현재 좌표에서 목표 좌표로의 경로를 계산한다.
    /// </summary>
    private void GetPath()
    {
        if (_currentCoordinate == _targetCoordinate)
        {
            _path.Clear();
            return;
        }

        PriorityQueue<Vector2Int, int> queue = new PriorityQueue<Vector2Int, int>();
        Dictionary<Vector2Int, Vector2Int> previous = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();

        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();

        queue.Enqueue(_currentCoordinate, 0);
        previous[_currentCoordinate] = new Vector2Int(-1, -1);
        distance[_currentCoordinate] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == _targetCoordinate)
            {
                break;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2Int next = HexaUtility.GetNeighbor(current, (TileNeighbor)i);

                if (MapManager.Instance.CheckCoordinateValidity(next) && MapManager.Instance.Tiles[next.x, next.y].IsUnderWater) {
                    int alternative = distance[current] + 1 + PirateManager.Instance.Penalties[next.x, next.y];

                    if (!PirateManager.Instance.CheckFreeCoordinate(next, this))
                    {
                        alternative += COLLISION_FACTOR;
                    }

                    if (!distance.ContainsKey(next) || distance[next] > alternative)
                    {
                        previous[next] = current;
                        distance[next] = alternative;

                        queue.Enqueue(next, alternative + HexaUtility.GetDistance(next, _targetCoordinate));
                    }
                }
            }
        }

        Vector2Int pathTrack = _targetCoordinate;
        pathStack.Push(pathTrack);

        while (previous[pathTrack].x != -1) {
            pathStack.Push(previous[pathTrack]);

            pathTrack = previous[pathTrack];
        }

        pathStack.Pop();
        _path.Clear();

        while (pathStack.Count > 0)
        {
            _path.Enqueue(pathStack.Pop());
        }
    }

    /// <summary>
    /// 스폰 애니메이션을 재생한다.
    /// </summary>
    private IEnumerator CoSpawnPirate()
    {
        float elapsed = 0.0f;

        // Start Animation

        while (true)
        {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
            {
                elapsed += Time.deltaTime;

                if (elapsed > SPAWN_ANIMATION_DURATION)
                {
                    break;
                }
            }

            yield return null;
        }

        // End Animation
    }

    /// <summary>
    /// 디스폰 애니메이션을 재생한다.
    /// </summary>
    private IEnumerator CoDespawnPirate()
    {
        float elapsed = 0.0f;

        while (true)
        {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
            {
                elapsed += Time.deltaTime;

                if (elapsed > DESPAWN_ANIMATION_DELAY)
                {
                    break;
                }
            }

            yield return null;
        }

        elapsed -= 3.0f;

        // Start Animation

        while (true)
        {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
            {
                elapsed += Time.deltaTime;

                if (elapsed > DESPAWN_ANIMATION_DURATION)
                {
                    break;
                }
            }

            yield return null;
        }

        // End Animation

        PirateManager.Instance.DespawnPirate(this);
    }

    /// <summary>
    /// 파괴 애니메이션을 재생한다.
    /// </summary>
    private IEnumerator CoDestroyPirate()
    {
        float elapsed = 0.0f;

        // Start Animation

        while (true)
        {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
            {
                elapsed += Time.deltaTime;

                if (elapsed > DESTROY_ANIMATION_DURATION)
                {
                    break;
                }
            }

            yield return null;
        }

        // End Animation

        PirateManager.Instance.DespawnPirate(this);
    }
}
