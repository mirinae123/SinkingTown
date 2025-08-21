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

    private void Start()
    {
        _tiles = GetComponent<MapGenerator>().GenerateMap(SessionManager.Instance.MapSize, SessionManager.Instance.MapSize, 6f);
        GetComponent<MapRenderer>().RenderMap();

        PirateManager.Instance.UpdatePenalties();
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
                    MapRenderer.Instance.AddSunkenStructure(tile.Coordinate, tile.Structure.StructureData.StructureType);
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
}
