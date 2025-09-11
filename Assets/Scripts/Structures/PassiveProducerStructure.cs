/// <summary>
/// 수동 생산형 건물 클래스
/// </summary>
public class PassiveProducerStructure : Structure
{
    private float _lastProduced = 0;

    public PassiveProducerStructure(StructureType type, Tile tile) : base(type, tile) { }

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

    public override void Initialize(StructureSaveData saveData)
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

    public override StructureSaveData GetSaveData()
    {
        PassiveProducerSaveData saveData = new PassiveProducerSaveData();

        saveData.StructureType = _structureData.StructureType;
        saveData.Coordinate = _tile.Coordinate;

        return saveData;
    }
}