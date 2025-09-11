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

    protected float _daytimeStart;  // 낮의 시작
    protected float _daytimeEnd;    // 낮의 끝

    protected bool _isDaytime;      // 낮 여부

    public Structure(StructureType type, Tile tile)
    {
        _structureData = StructureManager.Instance.GetStructureData(type);
        _tile = tile;

        _daytimeStart = 360.0f + tile.RandomIndex / 64.0f;
        _daytimeEnd = 1110.0f - tile.RandomIndex / 64.0f;

        _isDaytime = _daytimeStart < GameManager.Instance.CurrentTime && GameManager.Instance.CurrentTime < _daytimeEnd;
    }

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

    public virtual void Initialize(StructureSaveData saveData = null) { }

    public virtual void OnUpdate() {
        // 낮 여부 확인
        bool checkDaytime = _daytimeStart < GameManager.Instance.CurrentTime && GameManager.Instance.CurrentTime < _daytimeEnd;

        if (_isDaytime != checkDaytime)
        {
            _isDaytime = checkDaytime;
            OnRenderUpdate();
        }

        // 해수면 상승 처리
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
            if (_isDaytime)
            {
                _structureObject = GameObject.Instantiate(_structureData.DayStructurePrefab[_tile.RandomIndex % _structureData.DayStructurePrefab.Length], StructureManager.Instance.StructureHolder.transform);
            }
            else
            {
                _structureObject = GameObject.Instantiate(_structureData.NightStructurePrefab[_tile.RandomIndex % _structureData.NightStructurePrefab.Length], StructureManager.Instance.StructureHolder.transform);
            }
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

    public virtual StructureSaveData GetSaveData()
    {
        return new StructureSaveData();
    }
}