using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 천연 자원 종류
/// </summary>
public enum NaturalResourceType { None, Woods, Stone }

/// <summary>
/// 맵 상에서 각 타일을 나타내는 클래스
/// </summary>
public class Tile
{
    /// <summary>
    /// 좌표 값
    /// </summary>
    public Vector2Int Coordinate
    {
        get => _coordinate;
        set => _coordinate = value;
    }
    private Vector2Int _coordinate;

    /// <summary>
    /// 높이
    /// </summary>
    public int Height
    {
        get => _height;
        set => _height = value;
    }
    private int _height;

    /// <summary>
    /// 비옥한 땅 여부
    /// </summary>
    public bool IsFertile
    {
        get => _isFertile;
        set => _isFertile = value;
    }
    private bool _isFertile;

    /// <summary>
    /// 타일에 제공되는 자원
    /// </summary>
    public Resource Resource
    {
        get => _resource;
    }
    private Resource _resource;

    /// <summary>
    /// 타일에 위치한 건물
    /// </summary>
    public Structure Structure
    {
        get => _structure;
    }
    private Structure _structure;

    /// <summary>
    /// 데크 여부
    /// </summary>
    public bool IsDecked
    {
        get => _isDecked;
    }
    private bool _isDecked;

    /// <summary>
    /// 물에 잠겨 있는지 여부
    /// </summary>
    public bool IsUnderWater
    {
        get => _height < MapManager.Instance.OceanLevel;
    }

    /// <summary>
    /// 타일에 위치한 천연 자원
    /// </summary>
    public NaturalResourceType NaturalResource
    {
        get => _naturalResource;
        set => _naturalResource = value;
    }
    private NaturalResourceType _naturalResource = NaturalResourceType.None;

    /// <summary>
    /// 현재 타일에 자원을 제공하는 건물 리스트
    /// </summary>
    public IReadOnlyList<Structure> Providers
    {
        get => _providers;
    }
    private List<Structure> _providers = new List<Structure>();

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="coordinate">좌표 값</param>
    /// <param name="height">높이 </param>
    public Tile(Vector2Int coordinate, int height)
    {
        _coordinate = coordinate;
        _height = height;
    }

    /// <summary>
    /// 현재 타일에 건물을 생성한다.
    /// </summary>
    /// <param name="structure">건물 유형</param>
    public void CreateStructure(StructureType structure)
    {
        // 데크를 생성하는 경우
        if (structure == StructureType.Deck)
        {
            _isDecked = true;
            MapRenderer.Instance.AddDeckStructure(_coordinate);

            foreach (var neighbor in GetNeighbors(1))
            {
                neighbor.OnNotified();
            }
        }
        // 그 외 건물을 생성하는 경우
        else
        {
            _structure = StructureManager.Instance.GetStructure(structure, this);
            MapRenderer.Instance.UpdateTile(_coordinate);

            // 주변 타일의 자원 제공자에 현재 건물을 추가
            AddToProviders();
        }
    }

    /// <summary>
    /// 현재 타일에 위치한 건물을 파괴한다.
    /// </summary>
    public void DestroyStructure()
    {
        // 현재 타일에 다른 건물이 없고 데크만 있는 경우
        if (_structure == null && _isDecked)
        {
            _isDecked = false;
            MapRenderer.Instance.RemoveDeckStructure(_coordinate);

            foreach (var neighbor in GetNeighbors(1))
            {
                neighbor.OnNotified();
            }
        }
        // 현재 타일에 건물이 있는 경우
        else if (_structure != null)
        {
            // 주변 타일의 자원 제공자에서 현재 건물을 제거
            RemoveFromProviders();

            _structure = null;
            MapRenderer.Instance.UpdateTile(_coordinate);
        }
    }

    /// <summary>
    /// 현재 타일의 자원 정보가 변한 경우, 관련 정보를 갱신한다.
    /// </summary>
    public void OnNotified()
    {
        // 새로운 자원 제공량 계산
        Resource newResource = new Resource();
        int maxRadius = 0;

        foreach (var structure in _providers)
        {
            if (structure.StructureData.Produces.radiusBonus > maxRadius)
                maxRadius = structure.StructureData.Produces.radiusBonus;

            newResource += structure.GetEffectiveProduces();
        }

        newResource.radiusBonus = maxRadius;

        // 현재 타일에 건물이 없는 경우
        if (_structure == null)
        {
            _resource = newResource;
            UIManager.Instance.UpdateTileInfo(_coordinate);

            return;
        }
        // 현재 타일에 건물이 있는 경우
        else
        {
            // 추가 효율이 변한 경우
            if (newResource.efficiencyBonus != _resource.efficiencyBonus)
            {
                _resource = newResource;

                // 주변 타일에 변경 사실을 알림
                Tile[] neighbors = GetNeighbors(_structure.GetEffectiveRadius());
                foreach (var neighbor in neighbors)
                {
                    neighbor.OnNotified();
                }
            }
            // 추가 범위가 변한 경우
            else if (newResource.radiusBonus != _resource.radiusBonus)
            {
                // 영향을 받는 이웃 타일에 해당 사실을 알림
                Tile[] oldNeighbors = GetNeighbors(_structure.GetEffectiveRadius());
                _resource.radiusBonus = maxRadius;
                Tile[] newNeighbors = GetNeighbors(_structure.GetEffectiveRadius());

                if (oldNeighbors.Length > newNeighbors.Length)
                {
                    foreach (var neighbor in oldNeighbors.Except(newNeighbors))
                    {
                        // 자원 제공자에서 삭제하고 해당 사실을 알림
                        neighbor._providers.Remove(_structure);
                        neighbor.OnNotified();
                    }
                }
                else
                {
                    foreach (var neighbor in newNeighbors.Except(oldNeighbors))
                    {
                        neighbor._providers.Add(_structure);
                        neighbor.OnNotified();
                    }
                }
            }
            // 추가 범위나 효율이 변하지 않은 경우
            else
            {
                _resource = newResource;
            }

            _structure.OnNotified();
        }

        UIManager.Instance.UpdateTileInfo(_coordinate);
    }

    /// <summary>
    /// 주변 타일의 자원 제공자에 현재 건물을 추가한다.
    /// </summary>
    public void AddToProviders()
    {
        foreach (var neighbor in GetNeighbors(_structure.GetEffectiveRadius()))
        {
            if (!neighbor._providers.Contains(_structure))
            {
                neighbor._providers.Add(_structure);
                neighbor.OnNotified();
            }
        }
    }

    /// <summary>
    /// 주변 타일의 자원 제공자에서 현재 건물을 삭제한다.
    /// </summary>
    public void RemoveFromProviders()
    {
        foreach (var neighbor in GetNeighbors(_structure.GetEffectiveRadius()))
        {
            neighbor._providers.Remove(_structure);
            neighbor.OnNotified();
        }
    }

    /// <summary>
    /// 현재 타일의 이웃 타일을 찾는다.
    /// </summary>
    /// <param name="radius">반경</param>
    /// <returns>이웃 타일 배열 (현재 타일 포함)</returns>
    public Tile[] GetNeighbors(int radius)
    {
        Vector2Int[] neighboringCoords = HexaUtility.GetNeighbors(_coordinate, radius);
        List<Tile> neighbors = new List<Tile>();

        foreach (Vector2Int coord in neighboringCoords)
        {
            if (coord.x >= 0 && coord.y >= 0 &&
                coord.x < MapManager.Instance.Tiles.GetLength(0) &&
                coord.y < MapManager.Instance.Tiles.GetLength(1))
            {
                neighbors.Add(MapManager.Instance.Tiles[coord.x, coord.y]);
            }
        }

        return neighbors.ToArray();
    }
}
