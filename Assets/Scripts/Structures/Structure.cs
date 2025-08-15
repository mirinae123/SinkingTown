using UnityEngine;

/// <summary>
/// 건물 종류
/// </summary>
public enum StructureType { TownHall, House, Market, LumberCamp, Quarry, Pier, Farm, Restaurant, TextileMill, Fortress, Deck }

/// <summary>
/// 기본 건물 클래스
/// </summary>
public class Structure
{
    /// <summary>
    /// 건물 데이터
    /// </summary>
    public StructureData StructureData
    {
        get => _structureData;
    }
    protected StructureData _structureData;

    /// <summary>
    /// 건물 상태
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
    }
    protected bool _isEnabled;

    /// <summary>
    /// 현재 타일
    /// </summary>
    public Tile Tile
    {
        get => _tile;
    }
    protected Tile _tile;

    protected GameObject _structureObject;

    /// <summary>
    /// 추가 범위를 고려한 효과 범위를 계산한다.
    /// </summary>
    public int GetEffectiveRadius()
    {
        // 추가 범위를 제공하는 건물은 추가 범위 효과를 받지 않음
        if (StructureData.Produces.rangeBonus)
        {
            return StructureData.Radius;
        }
        // 일반 건물은 제공 받은 추가 범위 중 가장 큰 값을 추가 범위로 사용
        else if (StructureData.Radius > 0)
        {
            return StructureData.Radius + (_tile.Resource.rangeBonus ? 1 : 0);
        }
        // 효과 범위가 없는 건물은 추가 범위도 적용받지 않음
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 실질 생산량을 계산한다.
    /// </summary>
    public virtual Resource GetEffectiveProduces()
    {
        return StructureData.Produces;
    }

    /// <summary>
    /// 근처에 바다가 있는지 확인한다.
    /// </summary>
    public bool IsOceanNearby()
    {
        foreach (Tile neighbor in _tile.GetNeighbors(1))
        {
            if (neighbor.IsUnderWater && !neighbor.IsDecked)
            {
                return true;
            }
        }

        return false;
    }

    public virtual void Initialize() { }

    public virtual void OnUpdate() {
        if (_tile.IsUnderWater)
        {
            Vector3 position = _structureObject.transform.position;
            position.y = MapRenderer.Instance.OceanHeight;

            _structureObject.transform.position = position;
        }
    }

    public virtual void OnNotified() { }

    public virtual void OnRenderUpdate()
    {
        if (_structureObject)
        {
            GameObject.Destroy(_structureObject);
        }

        Vector3 position = HexaUtility.GetWorldCoordinate(_tile.Coordinate);
        position.y = Mathf.Max(_tile.Height + 1.0f, MapRenderer.Instance.OceanHeight);

        if (_isEnabled)
        {
            _structureObject = GameObject.Instantiate(_structureData.DayStructurePrefab[_tile.RandomIndex % _structureData.DayStructurePrefab.Length], StructureManager.Instance.StructureHolder.transform);
        }
        else
        {
            _structureObject = GameObject.Instantiate(_structureData.DisabledStructurePrefab[_tile.RandomIndex % _structureData.DisabledStructurePrefab.Length], StructureManager.Instance.StructureHolder.transform);
        }

        _structureObject.transform.position = position;
    }

    public virtual void OnRenderEnd()
    {
        if (_structureObject)
        {
            GameObject.Destroy(_structureObject);
        }
    }
}

/// <summary>
/// 소비형 건물 클래스
/// </summary>
public class ConsumerStructure : Structure
{
    // 현재 행복도
    public float CurrentHappiness
    {
        get => _currentHappiness;
    }
    private float _currentHappiness;

    private bool _isIncreasing;

    public ConsumerStructure(StructureType type, Tile tile)
    {
        _structureData = StructureManager.Instance.GetStructureData(type);
        _tile = tile;

        _currentHappiness = _structureData.MaxHappiness;

        _isEnabled = true;
        _isIncreasing = true;
    }

    public override void Initialize()
    {
        _tile.AddToProviders();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // 만족도가 증가 중인 경우
        if (_isIncreasing && _currentHappiness < _structureData.MaxHappiness)
        {
            _currentHappiness += _structureData.IncreaseSpeed * Time.deltaTime;

            // 만족도가 최대치까지 오른 경우
            if (_currentHappiness >= _structureData.MaxHappiness)
            {
                _currentHappiness = _structureData.MaxHappiness;
                _isEnabled = true;

                _tile.AddToProviders();

                OnRenderUpdate();
            }

        }
        // 만족도가 감소 중인 경우
        else if (!_isIncreasing && _currentHappiness > 0.0f)
        {
            _currentHappiness -= _structureData.DecreaseSpeed * Time.deltaTime;

            // 만족도가 최소치까지 준 경우
            if (_currentHappiness <= 0.0f)
            {
                _currentHappiness = 0.0f;
                _isEnabled = false;

                _tile.RemoveFromProviders();

                OnRenderUpdate();
            }
        }
    }

    public override void OnNotified()
    {
        _isIncreasing = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());
    }
}

/// <summary>
/// 능동 생산형 건물 클래스
/// </summary>
public class ActiveProducerStructure : Structure
{
    /// <summary>
    /// 마지막 생산으로부터 지난 시간
    /// </summary>
    public float Elapsed
    {
        get => _elapsed;
    }
    private float _elapsed;

    public ActiveProducerStructure(StructureType type, Tile tile)
    {
        _structureData = StructureManager.Instance.GetStructureData(type);
        _tile = tile;
    }

    public override void Initialize()
    {
        _tile.AddToProviders();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (_isEnabled)
        {
            _elapsed += Time.deltaTime;

            if (_elapsed > _structureData.TimeToProduce)
            {
                if (_structureData.StructureType != StructureType.Fortress)
                {
                    _elapsed -= _structureData.TimeToProduce;
                }

                switch (_structureData.StructureType)
                {
                    case StructureType.TownHall:
                        GameManager.Instance.ChangeResearchPoint(2);
                        break;
                    case StructureType.LumberCamp:
                        GameManager.Instance.CurrentWoods += 2; ;
                        break;
                    case StructureType.Quarry:
                        GameManager.Instance.CurrentStones += 1;
                        break;
                    case StructureType.Fortress:
                        if (AttackPirate())
                        {
                            _elapsed -= _structureData.TimeToProduce;
                        }
                        else
                        {
                            _elapsed = _structureData.TimeToProduce;
                        }
                        break;
                }
            }
        }
    }

    public override void OnNotified()
    {
        // 요구 사항 만족 여부
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        // 비활성 상태에서 만족
        if (satisfied && !_isEnabled)
        {
            _isEnabled = true;
            _tile.AddToProviders();

            OnRenderUpdate();
        }
        // 활성 상태에서 불만족
        else if (!satisfied && _isEnabled)
        {
            _elapsed = 0.0f;

            _isEnabled = false;
            _tile.RemoveFromProviders();

            OnRenderUpdate();
        }
    }

    /// <summary>
    /// 범위 안에 들어온 해적을 공격한다. 다른 요새로부터 공격을 받고 있지 않은 해적을 우선시한다.
    /// </summary>
    /// <returns>공격 여부</returns>
    private bool AttackPirate()
    {
        PirateController targetPirate = null;
        PirateController pirateAlreadyUnderAttack = null;

        foreach(PirateController pirate in PirateManager.Instance.Pirates)
        {
            if (pirate.CurrentState == PirateState.Despawn || pirate.CurrentState == PirateState.Destroy)
            {
                continue;
            }

            if (HexaUtility.GetDistance(_tile.Coordinate, pirate.CurrentCoordinate) <= _structureData.Radius)
            {
                if (pirate.IsUnderAttack)
                {
                    pirateAlreadyUnderAttack = pirate;
                }
                else
                {
                    targetPirate = pirate;
                }
            }
        }
        
        if (targetPirate == null)
        {
            targetPirate = pirateAlreadyUnderAttack;
        }

        if (targetPirate == null)
        {
            return false;
        }
        else
        {
            Vector3 fortressPosition = HexaUtility.GetWorldCoordinate(_tile.Coordinate);
            fortressPosition.y = MapManager.Instance.OceanLevel;

            Vector3 piratePosition = (HexaUtility.GetWorldCoordinate(targetPirate.GetFutureCoordinate(0)) + HexaUtility.GetWorldCoordinate(targetPirate.GetFutureCoordinate(1))) / 2.0f;
            piratePosition.y = MapRenderer.Instance.OceanHeight;

            targetPirate.StartAttackPirate();

            PirateManager.Instance.SpawnCannonball(fortressPosition, piratePosition, targetPirate);

            return true;
        }
    }
}

/// <summary>
/// 수동 생산형 건물 클래스
/// </summary>
public class PassiveProducerStructure : Structure
{
    private float _lastProduced = 0;

    public PassiveProducerStructure(StructureType type, Tile tile)
    {
        _structureData = StructureManager.Instance.GetStructureData(type);
        _tile = tile;
    }

    public override Resource GetEffectiveProduces()
    {
        switch (_structureData.StructureType)
        {
            case StructureType.Restaurant:
                return new Resource(food: _tile.Resource.fish);
            case StructureType.TextileMill:
                return new Resource(clothe: _tile.Resource.cotton);
            default:
                return base.GetEffectiveProduces();
        }
    }

    public override void Initialize()
    {
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        if (satisfied)
        {
            _isEnabled = true;
            _tile.AddToProviders();
        }
        else
        {
            _isEnabled = false;
        }
    }

    public override void OnNotified()
    {
        // 요구 사항 만족 여부
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        if (satisfied)
        {
            // 비활성 상태에서 만족
            if (!_isEnabled)
            {
                _isEnabled = true;
                _tile.AddToProviders();

                OnRenderUpdate();
            }
            // 식당에 공급되는 물고기 값이 변한 경우
            else if (_structureData.StructureType == StructureType.Restaurant &&
                _tile.Resource.fish != _lastProduced)
            {
                _lastProduced = _tile.Resource.fish;

                _tile.RemoveFromProviders();
                _tile.AddToProviders();
            }
            // 방직소에 공급되는 목화 값이 변한 경우
            else if (_structureData.StructureType == StructureType.TextileMill &&
                _tile.Resource.cotton != _lastProduced)
            {
                _lastProduced = _tile.Resource.cotton;

                _tile.RemoveFromProviders();
                _tile.AddToProviders();
            }
        }
        // 활성 상태에서 불만족
        else if (!satisfied && _isEnabled)
        {
            _isEnabled = false;
            _tile.RemoveFromProviders();

            OnRenderUpdate();
        }
    }
}