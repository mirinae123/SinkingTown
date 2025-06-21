using UnityEngine;

/// <summary>
/// 맵을 생성하는 클래스
/// </summary>
public class MapGenerator : SingletonBehaviour<MapGenerator>
{
    private int _initOceanLevel = 6;
    private int _maxHeight = 21;

    /// <summary>
    /// 설산 기준. 높이가 이것보다 높은 타일은 설산으로 간주한다.
    /// </summary>
    public int SnowThreshold
    {
        get => _snowThreshold;
    }
    private int _snowThreshold = 18;

    private float _woodsSpawnProb = 0.2f;
    private float _stoneSpawnProb = 0.1f;
    private float _envSpawnModifier = 0.1f;

    /// <summary>
    /// 가로 w, 세로 h, 복잡도 c를 가진 맵을 생성한다.
    /// </summary>
    /// <param name="w">기로 크기</param>
    /// <param name="h">세로 크기</param>
    /// <param name="c">복잡도</param>
    /// <returns>생성된 맵</returns>
    public Tile[,] GenerateMap(int w, int h, float c)
    {
        MapManager.Instance.OceanLevel = _initOceanLevel;

        Tile[,] tiles = new Tile[w, h];

        Random.InitState((int)System.DateTime.Now.Ticks);

        float mapOffset = Random.Range(-50, 50);
        float fertileOffset = Random.Range(-50, 50);

        int min = 1000;
        int max = -1000;

        // 최초 높이 값 생성
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int height = (int)((Mathf.PerlinNoise((float)i / w * c + mapOffset, (float)j / h * c + mapOffset) +
                    Mathf.PerlinNoise((float)i / w * c * 2f + mapOffset, (float)j / h * c * 2f + mapOffset)) * _maxHeight * 2);

                float t = w * w / 4f + h * h / 4f;
                t -= (i - w / 2f) * (i - w / 2f) + (j - h / 2f) * (j - h / 2f);
                t /= w * w / 4f + h * h / 4f;
                t *= t;

                height = (int)(height * t);

                if (height < min) min = height;
                if (height > max) max = height;

                tiles[i, j] = new Tile(new Vector2Int(i, j), height);
            }
        }

        float alpha = (float)_maxHeight / ((max - min) * (max - min));

        // 높이 값 보정 및 자원 배치
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                tiles[i, j].Coordinate = new Vector2Int(i, j);
                tiles[i, j].Height = (int)(alpha * (tiles[i, j].Height - min) * (tiles[i, j].Height - min));

                int height = tiles[i, j].Height;

                if (height <= _initOceanLevel)
                {
                    continue;
                }

                // 비옥한 땅 여부 결정
                bool isFertile = Mathf.PerlinNoise((float)i / w * c + fertileOffset, (float)j / h * c + fertileOffset) > .65f;
                tiles[i, j].IsFertile = isFertile;

                // 천연 자원 배치
                float resourceProb = Random.Range(0f, 1f);

                if (height > _snowThreshold)
                {
                    if (resourceProb > 1f - _woodsSpawnProb * _envSpawnModifier)
                    {
                        tiles[i, j].NaturalResource = NaturalResourceType.Woods;
                    }
                    else if (resourceProb < _stoneSpawnProb)
                    {
                        tiles[i, j].NaturalResource = NaturalResourceType.Stone;
                    }
                }
                else
                {
                    if (resourceProb > 1f - _woodsSpawnProb)
                    {
                        tiles[i, j].NaturalResource = NaturalResourceType.Woods;
                    }
                    else if (resourceProb < _stoneSpawnProb * _envSpawnModifier)
                    {
                        tiles[i, j].NaturalResource = NaturalResourceType.Stone;
                    }
                }
            }
        }

        return tiles;
    }
}
