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
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnPointMoveInput += OnPointMoveInput;
        InputHandler.Instance.OnClickInput += OnClickInput;
        InputHandler.Instance.OnEscapeInput += OnEscapeInput;
    }

    private void OnDisable()
    {
        MapRenderer.Instance.HideRangeHighlight();
        MapRenderer.Instance.HideStructurePreview();

        InputHandler.Instance.OnPointMoveInput -= OnPointMoveInput;
        InputHandler.Instance.OnClickInput -= OnClickInput;
        InputHandler.Instance.OnEscapeInput -= OnEscapeInput;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPaused)
        {
            GameManager.Instance.UpdateTime();
            GameManager.Instance.ProcessOceanRise();
        }

        _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();

        if (Physics.Raycast(_mainCamera.ScreenPointToRay(_mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            if (!_isPointerOverGameObject)
            {
                Vector2Int coordinate = HexaUtility.GetTileCoordinate(hit.point);

                MapRenderer.Instance.ShowRangeHighlight(coordinate, StructureManager.Instance.GetStructureData(_structureToBuild).Radius);
                MapRenderer.Instance.SetStructurePreviewTarget(coordinate, StructureManager.Instance.CheckStructureValidity(MapManager.Instance.Tiles[coordinate.x, coordinate.y], _structureToBuild));
            }
        }
    }

    private void OnPointMoveInput(InputValue value)
    {
        _mousePosition = value.Get<Vector2>();
    }

    private void OnClickInput()
    {
        if (Physics.Raycast(_mainCamera.ScreenPointToRay(_mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            if (!_isPointerOverGameObject)
            {
                Vector2Int coordinate = HexaUtility.GetTileCoordinate(hit.point);
                Tile tile = MapManager.Instance.Tiles[coordinate.x, coordinate.y];

                if (StructureManager.Instance.CheckStructureValidity(tile, _structureToBuild))
                {
                    tile.CreateStructure(_structureToBuild);

                    GameManager.Instance.CurrentWoods -= StructureManager.Instance.GetStructureData(_structureToBuild).WoodCost;
                    GameManager.Instance.CurrentStones -= StructureManager.Instance.GetStructureData(_structureToBuild).StoneCost;

                    GameManager.Instance.ChangeGameState(GameState.None);
                }
            }
        }
    }

    private void OnEscapeInput()
    {
        GameManager.Instance.ChangeGameState(GameState.None);
    }
    
    /// <summary>
    /// 건설할 건물을 지정한다.
    /// </summary>
    /// <param name="structureType">건설할 건물</param>
    public void SetStructureToBuild(StructureType structureType)
    {
        _structureToBuild = structureType;
        MapRenderer.Instance.ShowStructurePreview(_structureToBuild);
    }
}
