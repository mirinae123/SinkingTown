using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 맵을 렌더링하는 클래스.
/// </summary>
public class MapRenderer : SingletonBehaviour<MapRenderer>
{
    private const int CHUNK_SIZE = 64;
    private const float OCEAN_RISE_DURATION = 2f;

    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _oceanPrefab;

    [SerializeField] private GameObject _woodsPrefab;
    [SerializeField] private GameObject _stonePrefab;

    [SerializeField] private Material _sandMaterial;

    private GameObject _tileHolder;         // 모든 타일 오브젝트의 부모 (정리용)
    private GameObject _structureHolder;    // 모든 건물 오브젝트의 부모 (정리용)

    private GameObject[][,] _meshObjects = new GameObject[32][,];   // 모든 타일 오브젝트 [height][w, h]
    private GameObject[,] _structureObjects;                        // 모든 건물 오브젝트

    private Dictionary<Vector2Int, GameObject> _deckObjects;        // 모든 데크 오브젝트
    private HashSet<Vector2Int> _floatingStructures;                // 건물 오브젝트 중 데크 위에 있는 것 목록

    private StructureType[,] _sunkenStructures;                     // 물에 잠긴 건물 타입
    private GameObject[,] _sunkenStructureObjects;                  // 물에 잠긴 건물 오브젝트

    private GameObject _oceanObject;

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

        _structureObjects = new GameObject[w, h];
        _sunkenStructures = new StructureType[w, h];
        _sunkenStructureObjects = new GameObject[w, h];

        _deckObjects = new Dictionary<Vector2Int, GameObject>();
        _floatingStructures = new HashSet<Vector2Int>();

        _tileHolder = new GameObject("Tiles");
        _structureHolder = new GameObject("Structures");

        // 높이 i = 0부터 시작
        for (int i = 0; i < _meshObjects.Length; i++)
        {
            _meshObjects[i] = new GameObject[w / CHUNK_SIZE, h / CHUNK_SIZE];
            
            // 청크 좌표 (p, q)에 대해 반복
            for (int p = 0; p < _meshObjects[i].GetLength(0); p++)
            {
                for (int q = 0; q < _meshObjects[i].GetLength(1); q++)
                {
                    _meshObjects[i][p, q] = Instantiate(_tilePrefab, _tileHolder.transform);
                    _meshObjects[i][p, q].GetComponent<MeshFilter>().mesh = CreateChunkMesh(i, p, q);
                    _meshObjects[i][p, q].GetComponent<MeshCollider>().sharedMesh = _meshObjects[i][p, q].GetComponent<MeshFilter>().mesh;

                    // 바다에 잠겨 있으면 모래 Material로 변경
                    if (i <= MapManager.Instance.OceanLevel)
                    {
                        Material[] materials = _meshObjects[i][p, q].GetComponent<MeshRenderer>().materials;

                        for (int j = 0; j < materials.Length; j++)
                        {
                            materials[j] = _sandMaterial;
                        }

                        _meshObjects[i][p, q].GetComponent<MeshRenderer>().materials = materials;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 건물 및 천연 자원 렌더링을 갱신한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    public void UpdateTile(Vector2Int coordinate)
    {
        int x = coordinate.x;
        int y = coordinate.y;

        // 기존 렌더링 파괴
        if (_structureObjects[x, y])
        {
            Destroy(_structureObjects[x, y]);
            _floatingStructures.Remove(coordinate);
        }

        // World 좌표 계산
        Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);

        if (MapManager.Instance.Tiles[x, y].IsDecked)
        {
            worldCoordinate.y = _oceanObject.transform.position.y;
            _floatingStructures.Add(coordinate);
        }
        else
        {
            worldCoordinate.y = MapManager.Instance.Tiles[x, y].Height + 1;
        }

        // 렌더링할 프리팹 탐색
        GameObject prefabToInstantiate = null;

        if (MapManager.Instance.Tiles[x, y].Structure != null)
        {
            prefabToInstantiate = MapManager.Instance.Tiles[x, y].Structure.StructureData.StructurePrefab;
        }
        else if (MapManager.Instance.Tiles[x, y].NaturalResource == NaturalResourceType.Woods)
        {
            prefabToInstantiate = _woodsPrefab;
        }
        else if (MapManager.Instance.Tiles[x, y].NaturalResource == NaturalResourceType.Stone)
        {
            prefabToInstantiate = _stonePrefab;
        }

        // 주어진 좌표에 프리팹 렌더링
        if (prefabToInstantiate != null)
        {
            _structureObjects[x, y] = Instantiate(prefabToInstantiate, _structureHolder.transform);
            _structureObjects[x, y].name = x + "_" + y;
            _structureObjects[x, y].transform.position = worldCoordinate;
        }
    }

    /// <summary>
    /// 물에 잠긴 건물을 렌더링에 추가한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    /// <param name="structureType">건물 종류</param>
    public void AddSunkenStructure(Vector2Int coordinate, StructureType structureType)
    {
        int x = coordinate.x;
        int y = coordinate.y;

        Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);
        worldCoordinate.y = MapManager.Instance.Tiles[x, y].Height + 1;

        _sunkenStructures[x, y] = structureType;
        _sunkenStructureObjects[x, y] = Instantiate(StructureManager.Instance.GetStructureData(structureType).SunkenStructurePrefab);
        _sunkenStructureObjects[x, y].transform.position = worldCoordinate;
    }

    /// <summary>
    /// 데크를 렌더링에 추가한다.
    /// </summary>
    /// <param name="coordinate">좌표</param>
    public void AddDeckStructure(Vector2Int coordinate)
    {
        Vector3 worldCoordinate = HexaUtility.GetWorldCoordinate(coordinate);
        worldCoordinate.y = _oceanObject.transform.position.y;

        _deckObjects[coordinate] = Instantiate(StructureManager.Instance.GetStructureData(StructureType.Deck).StructurePrefab);
        _deckObjects[coordinate].transform.position = worldCoordinate;
    }

    /// <summary>
    /// 데크를 렌더링에서 제거한다.
    /// </summary>
    /// <param name="coordiate">좌표</param>
    public void RemoveDeckStructure(Vector2Int coordiate)
    {
        _deckObjects.Remove(coordiate);
    }

    /// <summary>
    /// 해수면 상승 애니메이션을 재생한다.
    /// </summary>
    public void RaiseOceanLevel(int before, int after)
    {
        if (before < after)
        {
            StartCoroutine(CoRaiseOceanLevel(before, after));
        }
    }

    /// <summary>
    /// 청크 메시를 생성한다.
    /// </summary>
    /// <param name="i">높이</param>
    /// <param name="p">청크 좌표 p</param>
    /// <param name="q">청크 좌표 q</param>
    /// <returns>메시</returns>
    private Mesh CreateChunkMesh(int i, int p, int q)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.subMeshCount = 8;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int>[] triangles = new List<int>[8];

        for (int j = 0; j < 8; j++)
        {
            triangles[j] = new List<int>();
        }

        int vertexIndex = 0;

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

                int topIndex = 0;
                int sideIndex = 1;

                if (MapManager.Instance.Tiles[x, y].Height > i)
                {
                    if (MapManager.Instance.Tiles[x, y].Height > MapGenerator.Instance.SnowThreshold)
                    {
                        sideIndex = 7;
                    }
                    else
                    {
                        sideIndex = 6;
                    }
                }
                else
                {
                    if (MapManager.Instance.Tiles[x, y].Height > MapGenerator.Instance.SnowThreshold)
                    {
                        topIndex = 4;
                        sideIndex = 5;
                    }
                    else if (MapManager.Instance.Tiles[x, y].IsFertile)
                    {
                        topIndex = 2;
                        sideIndex = 3;
                    }
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

                        uv.Add(HexaRenderUtility.Uv[0]);
                        uv.Add(HexaRenderUtility.Uv[1]);
                        uv.Add(HexaRenderUtility.Uv[2]);
                        uv.Add(HexaRenderUtility.Uv[3]);

                        triangles[sideIndex].Add(vertexIndex);
                        triangles[sideIndex].Add(vertexIndex + 1);
                        triangles[sideIndex].Add(vertexIndex + 2);
                        triangles[sideIndex].Add(vertexIndex + 2);
                        triangles[sideIndex].Add(vertexIndex + 1);
                        triangles[sideIndex].Add(vertexIndex + 3);

                        vertexIndex += 4;
                    }
                }

                // 윗면 그리기
                if (MapManager.Instance.Tiles[x, y].Height == i)
                {
                    vertices.Add(HexaRenderUtility.Vertices[12] + worldPosition);
                    uv.Add(HexaRenderUtility.TopUv[0]);

                    for (int j = 0; j < 6; j++)
                    {
                        vertices.Add(HexaRenderUtility.Vertices[j] + worldPosition);
                        uv.Add(HexaRenderUtility.TopUv[j]);
                    }

                    for (int j = 0; j < 18; j++)
                    {
                        triangles[topIndex].Add(vertexIndex + HexaRenderUtility.TopTriangles[j]);
                    }

                    vertexIndex += 7;
                }
            }
        }

        combinedMesh.SetVertices(vertices);
        combinedMesh.SetUVs(0, uv);

        for (int j = 0; j < 8; j++)
        {
            combinedMesh.SetTriangles(triangles[j], j);
        }

        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateTangents();

        return combinedMesh;
    }

    private IEnumerator CoRaiseOceanLevel(int before, int after)
    {
        Vector3 origianlPosition = _oceanObject.transform.position;
        Vector3 newOceanPosition = _oceanObject.transform.position;
        newOceanPosition.y = after + 0.8f;

        MeshRenderer[] meshRenderers = new MeshRenderer[(after - before) * (_meshObjects[0].GetLength(0) * _meshObjects[0].GetLength(1))];

        for (int i = 0; i < after - before; i++)
        {
            for (int p = 0; p < _meshObjects[0].GetLength(0); p++)
            {
                for (int q = 0; q < _meshObjects[0].GetLength(1); q++)
                {
                    meshRenderers[i * _meshObjects[0].GetLength(0) * _meshObjects[0].GetLength(1) + p * _meshObjects[0].GetLength(1) + q] = _meshObjects[before + i + 1][p, q].GetComponent<MeshRenderer>();
                }
            }
        }

        Material[] materials = meshRenderers[0].materials;

        float elapsed = 0f;

        while (elapsed < OCEAN_RISE_DURATION)
        {
            elapsed += Time.deltaTime;

            _oceanObject.transform.position = Vector3.Lerp(origianlPosition, newOceanPosition, elapsed / OCEAN_RISE_DURATION);

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].Lerp(materials[i], _sandMaterial, elapsed / OCEAN_RISE_DURATION);
            }

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].materials = materials;
            }

            foreach (GameObject deck in _deckObjects.Values)
            {
                Vector3 position = deck.transform.position;
                position.y = _oceanObject.transform.position.y;
                deck.transform.position = position;
            }

            foreach (Vector2Int floatingStructure in _floatingStructures)
            {
                Vector3 position = _structureObjects[floatingStructure.x, floatingStructure.y].transform.position;
                position.y = _oceanObject.transform.position.y;
                _structureObjects[floatingStructure.x, floatingStructure.y].transform.position = position;
            }

            yield return null;
        }

        _oceanObject.transform.position = newOceanPosition;
    }
}
