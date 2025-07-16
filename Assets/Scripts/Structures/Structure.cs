using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

/// <summary>
/// 건물의 현재 상태
/// </summary>
public enum StructureState { Enabled, Increasing, Decreasing, Disabled }

/// <summary>
/// 건물 종류
/// </summary>
public enum StructureType { TownHall, House, Apartment, Market, School, LumberCamp, Quarry, Pier, Dock, Farm, HydroponicsFarm, Restaurant, TextileMill, Fortress, Deck }

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
    public StructureState CurrentState
    {
        get => _currentState;
    }
    protected StructureState _currentState;

    /// <summary>
    /// 현재 타일
    /// </summary>
    public Tile Tile
    {
        get => _tile;
    }
    protected Tile _tile;

    /// <summary>
    /// 추가 범위를 고려한 효과 범위를 계산한다.
    /// </summary>
    public int GetEffectiveRadius()
    {
        // 추가 범위를 제공하는 건물은 추가 범위 효과를 받지 않음
        if (StructureData.Produces.radiusBonus != 0)
        {
            return StructureData.Radius;
        }
        // 일반 건물은 제공 받은 추가 범위 중 가장 큰 값을 추가 범위로 사용
        else if (StructureData.Radius > 0)
        {
            return StructureData.Radius + _tile.Resource.radiusBonus;
        }
        // 효과 범위가 없는 건물은 추가 범위도 적용받지 않음
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 추가 효율을 고려한 실질 생산량을 계산한다.
    /// </summary>
    public virtual Resource GetEffectiveProduces()
    {
        // 추가 효율성을 제공하는 건물은 추가 효율성의 효과를 받지 않음
        if (StructureData.Produces.efficiencyBonus != 0)
        {
            return StructureData.Produces;
        }
        // 추가 효율성을 고려해 생산량 수정
        else
        {
            return StructureData.Produces * (1f + _tile.Resource.efficiencyBonus);
        }
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
    public virtual void OnUpdate() { }
    public virtual void OnNotified() { }
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

    public ConsumerStructure(StructureType type, Tile tile)
    {
        _structureData = StructureManager.Instance.GetStructureData(type);
        _currentHappiness = _structureData.MaxHappiness;
        _tile = tile;
    }

    public override void Initialize()
    {
        _tile.AddToProviders();
    }

    public override void OnUpdate()
    {
        // 만족도가 증가 중인 경우
        if (_currentState == StructureState.Increasing)
        {
            _currentHappiness += _structureData.IncreaseSpeed * Time.deltaTime;

            // 만족도가 최대치까지 오른 경우
            if (_currentHappiness >= _structureData.MaxHappiness)
            {
                _currentHappiness = _structureData.MaxHappiness;
                _currentState = StructureState.Enabled;

                //_isActive = true;

                _tile.AddToProviders();
            }

        }
        // 만족도가 감소 중인 경우
        else if (_currentState == StructureState.Decreasing)
        {
            _currentHappiness -= _structureData.DecreaseSpeed * Time.deltaTime;

            // 만족도가 최소치까지 준 경우
            if (_currentHappiness <= 0)
            {
                _currentHappiness = 0;
                _currentState = StructureState.Disabled;

                //_isActive = false;

                _tile.RemoveFromProviders();
            }
        }
    }

    public override void OnNotified()
    {
        // 요구 사항 만족 여부
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        // 활성 상태에서 불만족
        if (_currentState == StructureState.Enabled && !satisfied)
        {
            _currentState = StructureState.Decreasing;
        }
        // 비활성 상태에서 만족
        else if (_currentState == StructureState.Disabled && satisfied)
        {
            _currentState = StructureState.Increasing;
        }
        // 증가 상태에서 불만족
        else if (_currentState == StructureState.Increasing && !satisfied)
        {
            _currentState = StructureState.Decreasing;
        }
        // 감소 상태에서 만족
        else if (_currentState == StructureState.Decreasing && satisfied)
        {
            _currentState = StructureState.Increasing;
        }
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
        if (_currentState != StructureState.Disabled)
        {
            _elapsed += Time.deltaTime;

            if (_elapsed > _structureData.TimeToProduce)
            {
                _elapsed -= _structureData.TimeToProduce;

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
                }

                Debug.Log("Produced Something");
            }
        }
    }

    public override void OnNotified()
    {
        // 요구 사항 만족 여부
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        // 비활성 상태에서 만족
        if (satisfied && _currentState == StructureState.Disabled)
        {
            _currentState = StructureState.Enabled;
            _tile.AddToProviders();
        }
        // 활성 상태에서 불만족
        else if (!satisfied && _currentState == StructureState.Enabled)
        {
            _elapsed = 0.0f;

            _currentState = StructureState.Disabled;
            _tile.RemoveFromProviders();
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
            _currentState = StructureState.Enabled;
            _tile.AddToProviders();
        }
        else
        {
            _currentState = StructureState.Disabled;
        }
    }

    public override void OnNotified()
    {
        // 요구 사항 만족 여부
        bool satisfied = !(_tile.Resource < _structureData.Needs) && (!_structureData.RequireOcean || IsOceanNearby());

        if (satisfied)
        {
            // 비활성 상태에서 만족
            if (_currentState == StructureState.Disabled)
            {
                _currentState = StructureState.Enabled;
                _tile.AddToProviders();
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
        else if (!satisfied && _currentState == StructureState.Enabled)
        {
            _currentState = StructureState.Disabled;
            _tile.RemoveFromProviders();
        }
    }
}