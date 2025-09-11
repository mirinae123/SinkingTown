using UnityEngine;

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

    public bool IsIncreasing
    {
        get => _isIncreasing;
    }
    private bool _isIncreasing;

    public ConsumerStructure(StructureType type, Tile tile) : base(type, tile) { }

    public override void Initialize(StructureSaveData saveData)
    {
        ConsumerSaveData consumerSaveData = (ConsumerSaveData)saveData;

        _isEnabled = consumerSaveData?.IsEnabled ?? true;

        _currentHappiness = consumerSaveData?.CurrentHappiness ?? _structureData.MaxHappiness;
        _isIncreasing = consumerSaveData?.IsIncreasing ?? true;

        if (_isEnabled)
        {
            _tile.AddToProviders();
        }
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

    public override StructureSaveData GetSaveData()
    {
        ConsumerSaveData saveData = new ConsumerSaveData();

        saveData.StructureType = _structureData.StructureType;
        saveData.Coordinate = _tile.Coordinate;

        saveData.IsEnabled = _isEnabled;
        saveData.CurrentHappiness = _currentHappiness;
        saveData.IsIncreasing = _isIncreasing;

        return saveData;
    }
}