using UnityEngine;

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

    public ActiveProducerStructure(StructureType type, Tile tile) : base(type, tile) { }

    public override void Initialize(StructureSaveData saveData)
    {
        _elapsed = ((ActiveProducerSaveData)saveData)?.Elapsed ?? 0.0f;
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
                        GameManager.Instance.ChangeResearchPoint(_structureData.ResearchPointProduce);
                        break;
                    case StructureType.LumberCamp:
                        GameManager.Instance.CurrentWoods += _structureData.WoodProduce;
                        break;
                    case StructureType.Quarry:
                        GameManager.Instance.CurrentStones += _structureData.StoneProduce;
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

        foreach (PirateController pirate in PirateManager.Instance.Pirates)
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

    public override StructureSaveData GetSaveData()
    {
        ActiveProducerSaveData saveData = new ActiveProducerSaveData();

        saveData.StructureType = _structureData.StructureType;
        saveData.Coordinate = _tile.Coordinate;

        saveData.Elapsed = _elapsed;

        return saveData;
    }
}