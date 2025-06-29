using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildState : MonoBehaviour
{
    [SerializeField] Material _validMaterial;
    [SerializeField] Material _invalidMaterial;

    private Camera _mainCamera;
    private Vector2 _mousePosition;
    private bool _isPointerOverGameObject = false;

    private StructureType _structureToBuild;
    private GameObject _previewObject;
    private MeshRenderer _previewMeshRenderer;

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
        if (_previewObject)
        {
            Destroy(_previewObject);
            _previewObject = null;
        }

        MapRenderer.Instance.RemoveHighlight();

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
    }

    private void OnPointMoveInput(InputValue value)
    {
        _mousePosition = value.Get<Vector2>();

        if (Physics.Raycast(_mainCamera.ScreenPointToRay(_mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            if (!_isPointerOverGameObject)
            {
                Vector2Int coordinate = HexaUtility.GetTileCoordinate(hit.point);

                MapRenderer.Instance.AddHighlight(coordinate, StructureManager.Instance.GetStructureData(_structureToBuild).Radius);

                Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);

                if (MapManager.Instance.Tiles[coordinate.x, coordinate.y].IsUnderWater)
                {
                    worldCoordinate.y = MapRenderer.Instance.OceanHeight;
                }
                else
                {
                    worldCoordinate.y = MapManager.Instance.Tiles[coordinate.x, coordinate.y].Height + 1.0f;
                }

                if (CheckStructureValidity(MapManager.Instance.Tiles[coordinate.x, coordinate.y], _structureToBuild))
                {
                    _previewMeshRenderer.material = _validMaterial;
                }
                else
                {
                    _previewMeshRenderer.material = _invalidMaterial;
                }

                _previewObject.transform.position = worldCoordinate;
            }
        }
    }

    private void OnClickInput()
    {
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

        _previewObject = Instantiate(StructureManager.Instance.GetStructureData(_structureToBuild).StructurePrefab);
        _previewMeshRenderer = _previewObject.GetComponent<MeshRenderer>();
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
