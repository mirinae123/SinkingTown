using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 맵을 렌더링하는 클래스.
/// </summary>
public class MapRenderer : SingletonBehaviour<MapRenderer>
{
    private const int CHUNK_SIZE = 32;
    private const float OCEAN_RISE_DURATION = 2f;

    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _oceanPrefab;

    [SerializeField] private GameObject _woodsPrefab;
    [SerializeField] private GameObject _stonePrefab;

    [SerializeField] private Material _sandMaterial;

    [SerializeField] private GameObject _rangePrefab;

    [SerializeField] Material _validMaterial;
    [SerializeField] Material _invalidMaterial;

    private GameObject _tileHolder;         // 모든 타일 오브젝트의 부모 (정리용)
    private GameObject _rangeHolder;        // 모든 효과 범위 오브젝트의 부모

    private GameObject[,] _meshObjects;
    private GameObject[,] _structureObjects;                 // 모든 천연 자원 오브젝트
    private Material _sharedTileMaterial;

    private Dictionary<Vector2Int, GameObject> _deckObjects;        // 모든 데크 오브젝트
    private Dictionary<Vector2Int, GameObject> _rangeObjects;       // 모든 효과 범위 오브젝트

    private Vector2Int _rangePosition = new Vector2Int(-1, -1);
    private int _rangeRadius = -1;

    private StructureType _previewType;
    private GameObject _previewObject;
    private MeshRenderer _previewMeshRenderer;
    private Vector2Int _previewTarget;

    private GameObject _oceanObject;

    public bool IsOceanRising
    {
        get => _isOceanRising;
    }
    private bool _isOceanRising;

    public float OceanHeight
    {
        get => _oceanObject.transform.position.y;
    }

    private void Update()
    {
        if (_previewObject)
        {
            Vector3 worldPosition = HexaUtility.GetWorldCoordinate(_previewTarget);
            worldPosition.y = Mathf.Max(_oceanObject.transform.position.y, MapManager.Instance.Tiles[_previewTarget.x, _previewTarget.y].Height + 1.0f);

            _previewObject.transform.position = worldPosition;
        }
    }

    /// <summary>
    /// 맵을 렌더링한다.
    /// </summary>
    public void RenderMap()
    {
        // 필요한 객체, 배열 초기화
        int oceanLevel = MapManager.Instance.OceanLevel;
        int w = MapManager.Instance.Tiles.GetLength(0);
        int h = MapManager.Instance.Tiles.GetLength(1);

        _oceanObject = Instantiate(_oceanPrefab);
        _oceanObject.transform.position = (HexaUtility.GetWorldCoordinate(new Vector2Int(0, 0)) + HexaUtility.GetWorldCoordinate(new Vector2Int(w - 1, h - 1))) / 2f + Vector3.up * (oceanLevel + 0.8f);
        _oceanObject.transform.localScale = Vector3.one * 100f;

        _meshObjects = new GameObject[w / CHUNK_SIZE, h / CHUNK_SIZE];
        _structureObjects = new GameObject[w, h];

        _deckObjects = new Dictionary<Vector2Int, GameObject>();
        _rangeObjects = new Dictionary<Vector2Int, GameObject>();

        _tileHolder = new GameObject("Tiles");
        _rangeHolder = new GameObject("Range");

        // 청크 좌표 (p, q)에 대해 반복
        for (int p = 0; p < _meshObjects.GetLength(0); p++)
        {
            for (int q = 0; q < _meshObjects.GetLength(1); q++)
            {
                _meshObjects[p, q] = Instantiate(_tilePrefab, _tileHolder.transform);
                _meshObjects[p, q].GetComponent<MeshFilter>().mesh = CreateChunkMesh(p, q);
                _meshObjects[p, q].GetComponent<MeshCollider>().sharedMesh = _meshObjects[p, q].GetComponent<MeshFilter>().mesh;

                if (_sharedTileMaterial == null)
                {
                    _sharedTileMaterial = _meshObjects[p, q].GetComponent<MeshRenderer>().sharedMaterial;
                }
            }
        }

        _sharedTileMaterial.SetFloat("_OceanLevel", _oceanObject.transform.position.y);
    }

    /// <summary>
    /// 천연 자원 및 물에 잠긴 건물의 렌더링을 갱신한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    public void UpdateTile(Vector2Int coordinate)
    {
        int x = coordinate.x;
        int y = coordinate.y;

        Tile currentTile = MapManager.Instance.Tiles[x, y];

        if (_structureObjects[x, y] != null)
        {
            Destroy(_structureObjects[x, y]);
        }

        // 렌더링할 프리팹 탐색
        GameObject prefabToInstantiate = null;

        if (currentTile.Structure == null)
        {
            if (currentTile.NaturalResource == NaturalResourceType.Woods)
            {
                prefabToInstantiate = _woodsPrefab;
            }
            else if (currentTile.NaturalResource == NaturalResourceType.Stone)
            {
                prefabToInstantiate = _stonePrefab;
            }
        }

        if (currentTile.SunkenStructure != null)
        {
            StructureData structureData = StructureManager.Instance.GetStructureData((StructureType)currentTile.SunkenStructure);
            prefabToInstantiate = structureData.SunkenStructurePrefab[currentTile.RandomIndex % structureData.SunkenStructurePrefab.Length];
        }

        // 주어진 좌표에 프리팹 렌더링
        if (prefabToInstantiate != null)
        {
            // World 좌표 계산
            Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);
            worldCoordinate.y = currentTile.Height + 1;

            _structureObjects[x, y] = Instantiate(prefabToInstantiate, StructureManager.Instance.StructureHolder.transform);
            _structureObjects[x, y].name = x + "_" + y;
            _structureObjects[x, y].transform.position = worldCoordinate;
        }
    }

    /// <summary>
    /// 데크를 렌더링에 추가한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    public void AddDeckStructure(Vector2Int coordinate)
    {
        int x = coordinate.x;
        int y = coordinate.y;

        Tile currentTile = MapManager.Instance.Tiles[x, y];
        StructureData structureData = StructureManager.Instance.GetStructureData(StructureType.Deck);

        Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);
        worldCoordinate.y = _oceanObject.transform.position.y;

        _deckObjects[coordinate] = Instantiate(structureData.DayStructurePrefab[currentTile.RandomIndex % structureData.DayStructurePrefab.Length]);
        _deckObjects[coordinate].transform.position = worldCoordinate;
    }

    /// <summary>
    /// 데크를 렌더링에서 제거한다.
    /// </summary>
    /// <param name="coordiate">좌표</param>
    public void RemoveDeckStructure(Vector2Int coordiate)
    {
        Destroy(_deckObjects[coordiate]);
        _deckObjects.Remove(coordiate);
    }

    /// <summary>
    /// 효과 범위를 표시한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    /// <param name="radius">범위</param>
    public void ShowRangeHighlight(Vector2Int coordinate, int radius)
    {
        if (coordinate == _rangePosition && radius == _rangeRadius)
        {
            return;
        }

        foreach (Transform child in _rangeHolder.transform)
        {
            Destroy(child.gameObject);
        }

        _rangeObjects.Clear();

        Tile[] neighbors = MapManager.Instance.Tiles[coordinate.x, coordinate.y].GetNeighbors(radius);

        foreach (Tile neighbor in neighbors)
        {
            if (neighbor.Coordinate == coordinate)
            {
                continue;
            }

            GameObject newHighlighObject = Instantiate(_rangePrefab, _rangeHolder.transform);
            _rangeObjects[neighbor.Coordinate] = newHighlighObject;

            Vector3 newPosition = HexaUtility.GetWorldCoordinate(neighbor.Coordinate);

            if (neighbor.IsUnderWater)
            {
                newPosition.y = _oceanObject.transform.position.y;
            }
            else
            {
                newPosition.y = neighbor.Height + 1.0f;
            }

            newHighlighObject.transform.position = newPosition;
        }

        _rangePosition = coordinate;
        _rangeRadius = radius;
    }

    /// <summary>
    /// 효과 범위를 숨긴다.
    /// </summary>
    public void HideRangeHighlight()
    {
        foreach (Transform child in _rangeHolder.transform)
        {
            Destroy(child.gameObject);
        }

        _rangeObjects.Clear();

        _rangePosition = new Vector2Int(-1, -1);
        _rangeRadius = -1;
    }

    /// <summary>
    /// 건물 미리보기를 표시한다.
    /// </summary>
    /// <param name="structure">건물</param>
    public void ShowStructurePreview(StructureType structure)
    {
        if (_previewObject)
        {
            Destroy(_previewObject.gameObject);
        }

        _previewType = structure;

        _previewObject = Instantiate(StructureManager.Instance.GetStructureData(structure).DayStructurePrefab[0]);
        _previewMeshRenderer = _previewObject.GetComponent<MeshRenderer>();

        Destroy(_previewObject.GetComponent<Collider>());

        _previewMeshRenderer.material = _invalidMaterial;
        _previewTarget = Vector2Int.zero;
    }

    /// <summary>
    /// 건물 미리보기가 표시될 위치를 지정한다.
    /// </summary>
    /// <param name="coordinate">목표 좌표</param>
    /// <param name="isValid">유효 여부</param>
    public void SetStructurePreviewTarget(Vector2Int coordinate, bool isValid)
    {
        if (_previewTarget == coordinate)
        {
            return;
        }

        _previewMeshRenderer.material = isValid ? _validMaterial : _invalidMaterial;
        _previewTarget = coordinate;
    }

    /// <summary>
    /// 건물 미리보기를 숨긴다.
    /// </summary>
    public void HideStructurePreview()
    {
        if (_previewObject)
        {
            Destroy(_previewObject);
            _previewObject = null;
            _previewMeshRenderer = null;
        }
    }

    /// <summary>
    /// 해수면 상승 애니메이션을 재생한다.
    /// </summary>
    public void RaiseOceanLevel(int before, int after)
    {
        if (before < after)
        {
            StartCoroutine(CoRaiseOceanLevel(before, after));

            if (_previewMeshRenderer)
            {
                bool isValid = StructureManager.Instance.CheckStructureValidity(MapManager.Instance.Tiles[_previewTarget.x, _previewTarget.y], _previewType);
                _previewMeshRenderer.material = isValid ? _validMaterial : _invalidMaterial;
            }
        }
    }

    /// <summary>
    /// 청크 메시를 생성한다.
    /// </summary>
    /// <param name="i">높이</param>
    /// <param name="p">청크 좌표 p</param>
    /// <param name="q">청크 좌표 q</param>
    /// <returns>메시</returns>
    private Mesh CreateChunkMesh(int p, int q)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.subMeshCount = 8;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> uv = new List<Vector3>();
        List<int> triangles = new List<int>();

        int vertexIndex = 0;

        for (int i = 0; i <= MapGenerator.Instance.MaxHeight; i++)
        {
            for (int x = p * CHUNK_SIZE; x < (p + 1) * CHUNK_SIZE; x++)
            {
                for (int y = q * CHUNK_SIZE; y < (q + 1) * CHUNK_SIZE; y++)
                {
                    if (MapManager.Instance.Tiles[x, y].Height < i)
                    {
                        continue;
                    }
                    else if (MapManager.Instance.Tiles[x, y].Height == i)
                    {
                        UpdateTile(new Vector2Int(x, y));
                    }

                    Vector3 worldPosition = HexaUtility.GetWorldCoordinate(new Vector2Int(x, y));
                    worldPosition.y = i;

                    TextureType sideTexture = TextureType.GrassSide;
                    TextureType topTexture = TextureType.Grass;

                    if (MapManager.Instance.Tiles[x, y].Height > i)
                    {
                        if (MapManager.Instance.Tiles[x, y].Height > MapGenerator.Instance.SnowThreshold)
                        {
                            sideTexture = TextureType.Rock;
                        }
                        else
                        {
                            sideTexture = TextureType.Dirt;
                        }
                    }
                    else if (MapManager.Instance.Tiles[x, y].Height > MapGenerator.Instance.SnowThreshold)
                    {
                        sideTexture = TextureType.SnowSide;
                        topTexture = TextureType.Snow;
                    }

                    for (int t = 0; t < 6; t++)
                    {
                        Vector2Int neighbor = HexaUtility.GetNeighbor(new Vector2Int(x, y), (TileNeighbor)t);

                        if (neighbor.x < 0 || neighbor.x >= MapManager.Instance.Tiles.GetLength(0) ||
                            neighbor.y < 0 || neighbor.y >= MapManager.Instance.Tiles.GetLength(1))
                        {
                            continue;
                        }

                        // 옆면 그리기
                        if (MapManager.Instance.Tiles[neighbor.x, neighbor.y].Height < i)
                        {
                            vertices.Add(HexaRenderUtility.Vertices[HexaRenderUtility.Triangles[t][0]] + worldPosition);
                            vertices.Add(HexaRenderUtility.Vertices[HexaRenderUtility.Triangles[t][1]] + worldPosition);
                            vertices.Add(HexaRenderUtility.Vertices[HexaRenderUtility.Triangles[t][2]] + worldPosition);
                            vertices.Add(HexaRenderUtility.Vertices[HexaRenderUtility.Triangles[t][5]] + worldPosition);

                            uv.Add(new Vector3(HexaRenderUtility.Uv[0][0], HexaRenderUtility.Uv[0][1], (float)sideTexture));
                            uv.Add(new Vector3(HexaRenderUtility.Uv[1][0], HexaRenderUtility.Uv[1][1], (float)sideTexture));
                            uv.Add(new Vector3(HexaRenderUtility.Uv[2][0], HexaRenderUtility.Uv[2][1], (float)sideTexture));
                            uv.Add(new Vector3(HexaRenderUtility.Uv[3][0], HexaRenderUtility.Uv[3][1], (float)sideTexture));

                            triangles.Add(vertexIndex);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 3);

                            vertexIndex += 4;
                        }
                    }

                    // 윗면 그리기
                    if (MapManager.Instance.Tiles[x, y].Height == i)
                    {
                        vertices.Add(HexaRenderUtility.Vertices[12] + worldPosition);
                        uv.Add(new Vector3(HexaRenderUtility.TopUv[0][0], HexaRenderUtility.TopUv[0][1], (float)topTexture));

                        for (int j = 0; j < 6; j++)
                        {
                            vertices.Add(HexaRenderUtility.Vertices[j] + worldPosition);
                            uv.Add(new Vector3(HexaRenderUtility.TopUv[j + 1][0], HexaRenderUtility.TopUv[j + 1][1], (float)topTexture));
                        }

                        for (int j = 0; j < 18; j++)
                        {
                            triangles.Add(vertexIndex + HexaRenderUtility.TopTriangles[j]);
                        }

                        vertexIndex += 7;
                    }
                }
            }
        }

        combinedMesh.SetVertices(vertices);
        combinedMesh.SetUVs(0, uv);
        combinedMesh.SetTriangles(triangles, 0);

        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateTangents();

        return combinedMesh;
    }

    private IEnumerator CoRaiseOceanLevel(int before, int after)
    {
        _isOceanRising = true;

        Vector3 origianlPosition = _oceanObject.transform.position;
        Vector3 newOceanPosition = _oceanObject.transform.position;
        newOceanPosition.y = after + 0.8f;

        float elapsed = 0f;

        while (elapsed < OCEAN_RISE_DURATION)
        {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
            {
                elapsed += Time.deltaTime;

                _oceanObject.transform.position = Vector3.Lerp(origianlPosition, newOceanPosition, elapsed / OCEAN_RISE_DURATION);
                _sharedTileMaterial.SetFloat("_OceanLevel", origianlPosition.y + (newOceanPosition.y - origianlPosition.y) * (1.0f - Mathf.Pow(1.0f - elapsed / OCEAN_RISE_DURATION, 3)));

                foreach (GameObject deck in _deckObjects.Values)
                {
                    Vector3 position = deck.transform.position;
                    position.y = _oceanObject.transform.position.y;
                    deck.transform.position = position;
                }

                foreach (KeyValuePair<Vector2Int, GameObject> highlight in _rangeObjects)
                {
                    Vector3 position = highlight.Value.transform.position;
                    position.y = Mathf.Max(_oceanObject.transform.position.y, MapManager.Instance.Tiles[highlight.Key.x, highlight.Key.y].Height + 1.0f);
                    highlight.Value.transform.position = position;
                }
            }

            yield return null;
        }

        _isOceanRising = false;
        _oceanObject.transform.position = newOceanPosition;
    }
}
