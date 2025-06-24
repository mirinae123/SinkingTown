using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildState : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector2 _mousePosition;
    private bool _isPointerOverGameObject = false;

    private StructureType _structureToBuild;

    private void Start()
    {
        _mainCamera = Camera.main;

        InputHandler.Instance.OnPointMoveInput += OnPointMoveInput;
        InputHandler.Instance.OnClickInput += OnClickInput;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPaused)
        {
            GameManager.Instance.UpdateTime();
            GameManager.Instance.ProcessOceanRise();
        }

        _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
    }

    private void OnPointMoveInput(InputValue value)
    {
        if (!enabled)
        {
            return;
        }

        _mousePosition = value.Get<Vector2>();
    }

    private void OnClickInput()
    {
        if (!enabled)
        {
            return;
        }

        if (Physics.Raycast(_mainCamera.ScreenPointToRay(_mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            if (!_isPointerOverGameObject)
            {
                Vector2Int coordinate = HexaUtility.GetTileCoordinate(hit.point);
                Tile tile = MapManager.Instance.Tiles[coordinate.x, coordinate.y];

                if (CheckStructureValidity(tile, _structureToBuild))
                {
                    tile.CreateStructure(_structureToBuild);
                    GameManager.Instance.ChangeGameState(GameState.None);
                }
            }
        }
    }
    
    /// <summary>
    /// 건설할 건물을 지정한다.
    /// </summary>
    /// <param name="structureType">건설할 건물</param>
    public void SetStructureToBuild(StructureType structureType)
    {
        _structureToBuild = structureType;
    }

    /// <summary>
    /// 타일에 건물을 지을 수 있는지 확인한다.
    /// </summary>
    /// <param name="tile">타일</param>
    /// <param name="structureType">건물 종류</param>
    /// <returns>지을 수 있는지 여부</returns>
    private bool CheckStructureValidity(Tile tile, StructureType structureType)
    {
        if (tile.Structure != null)
        {
            return false;
        }

        if (tile.IsUnderWater && !tile.IsDecked)
        {
            return structureType == StructureType.Deck && !tile.IsDecked;
        }

        switch (structureType)
        {
            case StructureType.Deck:
                return false;
            case StructureType.Pier:
            case StructureType.Dock:
                foreach (Tile neighbor in tile.GetNeighbors(1))
                {
                    if (neighbor.IsUnderWater && !neighbor.IsDecked)
                    {
                        return true;
                    }
                }
                return false;
            case StructureType.Farm:
                return !tile.IsDecked && tile.IsFertile;
            case StructureType.HydroponicsFarm:
                return tile.IsDecked;
            case StructureType.LumberCamp:
                return !tile.IsDecked && tile.NaturalResource == NaturalResourceType.Woods;
            case StructureType.Quarry:
                return !tile.IsDecked && tile.NaturalResource == NaturalResourceType.Stone;
            default:
                return true;

        }
    }
}
