using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

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
        Random.InitState(0);

        _tiles = GetComponent<MapGenerator>().GenerateMap(128, 128, 6f);
        GetComponent<MapRenderer>().RenderMap();
    }

    private void Update()
    {
        foreach (Tile tile in Tiles)
        {
            if (tile.Structure != null) tile.Structure.OnUpdate();
        }

        // !TEST
        if (Input.GetKeyDown(KeyCode.R))
        {
            RaiseOceanLevel();
        }
    }

    /// <summary>
    /// 해수면을 상승시킨다.
    /// </summary>
    public void RaiseOceanLevel()
    {
        foreach (Tile tile in Tiles)
        {
            if (tile.Height == _oceanLevel && tile.Structure != null)
            {
                MapRenderer.Instance.AddSunkenStructure(tile.Coordinate, tile.Structure.StructureData.StructureType);
                tile.DestroyStructure();
            }
        }

        MapRenderer.Instance.RaiseOceanLevel(_oceanLevel, ++_oceanLevel);
    }

    // !TEST
    void OnDrawGizmos()
    {
        bool display = true;
        if (!display) return;

        if (Tiles == null) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = new Color(0, 0, 0, .2f);
        style.alignment = TextAnchor.MiddleCenter;

        Vector3? target = null;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            target = hit.point;
        }

        if (target == null) return;

        foreach (Tile td in Tiles)
        {
            if (Vector3.Distance((Vector3)target, HexaUtility.GetWorldCoordinate(td.Coordinate)) > 10f) continue;
            float height = td.Height <= _oceanLevel ? (_oceanLevel + 1) * 0.5f + 0.75f : td.Height * 0.5f + 0.75f;
            Handles.Label(HexaUtility.GetWorldCoordinate(td.Coordinate) + Vector3.up * height, string.Format("{0}/{1}-{2}/{3}-{4}\n{5}-{6}/{7}-{8}", td.Resource.population, td.Resource.fish, td.Resource.food, td.Resource.wood, td.Resource.stone, td.Resource.cotton, td.Resource.clothe, td.Resource.efficiencyBonus, td.Resource.radiusBonus), style);
        }
    }
}
