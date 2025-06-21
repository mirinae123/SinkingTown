using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StructurePair
{
    public StructureType Type;
    public StructureData Data;
}

/// <summary>
/// 건물 정보를 관리하는 클래스
/// </summary>
public class StructureManager : SingletonBehaviour<StructureManager>
{
    [SerializeField] private StructurePair[] _structurePairs;

    private Dictionary<StructureType, StructureData> _structureData;

    private void OnValidate()
    {
        _structureData = new Dictionary<StructureType, StructureData>();

        foreach (StructurePair pair in _structurePairs)
        {
            _structureData[pair.Type] = pair.Data;
        }
    }

    /// <summary>
    /// 특정 타일에 배치할 건물 클래스를 생성한다.
    /// </summary>
    /// <param name="type">건물 종류</param>
    /// <param name="tile">타일</param>
    /// <returns>건물 클래스</returns>
    public Structure GetStructure(StructureType type, Tile tile)
    {
        Structure structure = IsConsumer(type) ? new ConsumerStructure() : new ProducerStructure();
        structure.Initialize(type, tile);

        return structure;
    }

    /// <summary>
    /// 특정 건물 종류에 대응되는 건물 데이터를 가져온다.
    /// </summary>
    /// <param name="type">건물 종류</param>
    /// <returns>건물 데이터</returns>
    public StructureData GetStructureData(StructureType type)
    {
        return _structureData[type];
    }

    /// <summary>
    /// 소비형 건물 여부를 확인한다.
    /// </summary>
    /// <param name="type">건물 종류</param>
    /// <returns>소비형 건물인 경우 true</returns>
    public bool IsConsumer(StructureType type)
    {
        switch (type)
        {
            case StructureType.House:
            case StructureType.Apartment:
            case StructureType.Market:
            case StructureType.School:
            case StructureType.Fortress:
                return true;

            default:
                return false;
        }
    }
}
