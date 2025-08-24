using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 맵을 관리하는 클래스
/// </summary>
public class MapManager : SingletonBehaviour<MapManager>
{
    /// <summary>
    /// 맵
    /// </summary>
    public Tile[,] Tiles
    {
        get => _tiles;
    }
    private Tile[,] _tiles;

    /// <summary>
    /// 현재 해수면 높이
    /// </summary>
    public int OceanLevel
    {
        get => _oceanLevel;
        set => _oceanLevel = value;
    }
    private int _oceanLevel;

    /// <summary>
    /// 초기화 진행 여부
    /// </summary>
    public bool IsInitializing
    {
        get => _isInitializing;
    }
    private bool _isInitializing;

    private void Start()
    {
        _oceanLevel = SaveManager.Instance.SaveData.OceanHeight;
        _tiles = GetComponent<MapGenerator>().GenerateMap(SaveManager.Instance.SaveData.MapSize, SaveManager.Instance.SaveData.MapSize, 6f);
      
        PirateManager.Instance.UpdatePenalties();
        GetComponent<MapRenderer>().RenderMap();

        LoadSaveData(SaveManager.Instance.SaveData);
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused || GameManager.Instance.GameState == GameState.Menu)
        {
            return;
        }

        foreach (Tile tile in Tiles)
        {
            if (tile.Structure != null) tile.Structure.OnUpdate();
        }
    }

    /// <summary>
    /// 해수면을 상승시킨다.
    /// </summary>
    public void RaiseOceanLevel()
    {
        foreach (Tile tile in Tiles)
        {
            if (tile.Height == _oceanLevel)
            {
                if (tile.NaturalResource != NaturalResourceType.None)
                {
                    tile.NaturalResource = NaturalResourceType.None;
                    MapRenderer.Instance.UpdateTile(tile.Coordinate);

                    UIManager.Instance.ShowPanel(PanelType.Tile, tile.Coordinate);
                }

                if (tile.Structure != null)
                {
                    tile.SunkenStructure = tile.Structure.StructureData.StructureType;
                    tile.DestroyStructure();
                }
            }
        }

        if (_oceanLevel == MapGenerator.Instance.MaxHeight)
        {
            GameManager.Instance.EndGame(false);
        }

        MapRenderer.Instance.RaiseOceanLevel(_oceanLevel, ++_oceanLevel);
        PirateManager.Instance.UpdatePenalties();
    }

    public bool CheckCoordinateValidity(Vector2Int coordinate)
    {
        return 0 <= coordinate.x && coordinate.x < _tiles.GetLength(0) && 0 <= coordinate.y && coordinate.y < _tiles.GetLength(1);
    }

    /// <summary>
    /// 세이브 데이터에 정보를 추가한다.
    /// </summary>
    /// <param name="saveData">세이브 데이터</param>
    public void PopulateSaveData(SaveData saveData)
    {
        saveData.OceanHeight = _oceanLevel;

        List<ConsumerSaveData> consumerStructureList = new List<ConsumerSaveData>();
        List<PassiveProducerSaveData> passiveProducerStructureList = new List<PassiveProducerSaveData>();
        List<ActiveProducerSaveData> activeProducerStructureList = new List<ActiveProducerSaveData>();

        List<SunkenStructureSaveData> sunkenStructureList = new List<SunkenStructureSaveData>();
        List<Vector2Int> deckList = new List<Vector2Int>();

        foreach (Tile tile in _tiles)
        {
            if (tile.Structure != null)
            {
                if (tile.Structure is ConsumerStructure)
                {
                    consumerStructureList.Add((ConsumerSaveData)tile.Structure.GetSaveData());
                }
                else if (tile.Structure is PassiveProducerStructure)
                {
                    passiveProducerStructureList.Add((PassiveProducerSaveData)tile.Structure.GetSaveData());
                }
                else
                {
                    activeProducerStructureList.Add((ActiveProducerSaveData)tile.Structure.GetSaveData());
                }
            }

            if (tile.SunkenStructure != null)
            {
                SunkenStructureSaveData sunkenStructure = new SunkenStructureSaveData();

                sunkenStructure.StructureType = (StructureType)tile.SunkenStructure;
                sunkenStructure.Coordinate = tile.Coordinate;

                sunkenStructureList.Add(sunkenStructure);
            }

            if (tile.IsDecked)
            {
                deckList.Add(tile.Coordinate);
            }
        }

        saveData.ConsumerStructures = consumerStructureList.ToArray();
        saveData.PassiveProducerStructures = passiveProducerStructureList.ToArray();
        saveData.ActiveProducerStructures = activeProducerStructureList.ToArray();

        saveData.SunkenStructures = sunkenStructureList.ToArray();
        saveData.Decks = deckList.ToArray();
    }

    /// <summary>
    /// 세이브 데이터로부터 정보를 불러온다.
    /// </summary>
    /// <param name="saveData">세이브 데이터</param>
    public void LoadSaveData(SaveData saveData)
    {
        _isInitializing = true;

        foreach (Vector2Int deck in saveData.Decks)
        {
            _tiles[deck.x, deck.y].CreateStructure(StructureType.Deck);
        }

        foreach (ConsumerSaveData structure in saveData.ConsumerStructures)
        {
            _tiles[structure.Coordinate.x, structure.Coordinate.y].CreateStructure(structure.StructureType, structure);
        }

        foreach (PassiveProducerSaveData structure in saveData.PassiveProducerStructures)
        {
            _tiles[structure.Coordinate.x, structure.Coordinate.y].CreateStructure(structure.StructureType, structure);
        }

        foreach (ActiveProducerSaveData structure in saveData.ActiveProducerStructures)
        {
            _tiles[structure.Coordinate.x, structure.Coordinate.y].CreateStructure(structure.StructureType, structure);
        }

        foreach (SunkenStructureSaveData sunkenStructure in saveData.SunkenStructures)
        {
            _tiles[sunkenStructure.Coordinate.x, sunkenStructure.Coordinate.y].SunkenStructure = sunkenStructure.StructureType;
        }

        _isInitializing = false;
    }
}
