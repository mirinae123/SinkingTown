using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타일 맵 이웃 방향
/// </summary>
public enum TileNeighbor { UpperLeft, Up, UpperRight, LowerRight, Down, LowerLeft }

/// <summary>
/// 육각형 타일 맵에 필요한 유틸리티를 담은 클래스.
/// </summary>
public class HexaUtility
{
    /// <summary>
    /// 특정 좌표를 기준으로 특정 방향의 이웃을 찾는다.
    /// </summary>
    /// <param name="i">좌표</param>
    /// <param name="n">방향</param>
    /// <returns>이웃 좌표</returns>
    public static Vector2Int GetNeighbor(Vector2Int i, TileNeighbor n)
    {
        int x = i.x;
        int y = i.y;

        switch (n)
        {
            case TileNeighbor.Up:
                return new Vector2Int(x, y + 1);

            case TileNeighbor.Down:
                return new Vector2Int(x, y - 1);

            case TileNeighbor.UpperLeft:
                return new Vector2Int(x - 1, y + (x & 1));

            case TileNeighbor.LowerLeft:
                return new Vector2Int(x - 1, y - (1 - (x & 1)));

            case TileNeighbor.UpperRight:
                return new Vector2Int(x + 1, y + (x & 1));

            default:
                return new Vector2Int(x + 1, y - (1 - (x & 1)));
        }
    }

    /// <summary>
    /// 특정 좌표를 기준으로 범위 내의 이웃을 찾는다.
    /// </summary>
    /// <param name="i">좌표</param>
    /// <param name="n">반경</param>
    /// <returns>이웃 좌표들</returns>
    public static Vector2Int[] GetNeighbors(Vector2Int i, int n)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        neighbors.Add(i);

        int currentDistance = 0;
        int searchLow = 0, searchHigh = 0;

        while (currentDistance < n)
        {
            int count = 0;
            for (; searchLow <= searchHigh; searchLow++)
            {
                for (int k = 0; k < 6; k++)
                {
                    Vector2Int neighbor = GetNeighbor(neighbors[searchLow], (TileNeighbor)k);
                    if (!neighbors.Contains(neighbor))
                    {
                        neighbors.Add(neighbor);
                        count++;
                    }
                }
            }
            searchHigh += count;

            currentDistance++;
        }

        return neighbors.ToArray();
    }

    /// <summary>
    /// 두 좌표의 거리를 계산한다.
    /// </summary>
    /// <param name="a">좌표 1</param>
    /// <param name="b">좌표 2</param>
    /// <returns>거리 </returns>
    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        if (a == b) return 0;

        int distance = 0;

        List<Vector2Int> neighbors = new List<Vector2Int>();
        neighbors.Add(a);

        int temp = -1;

        while (true)
        {
            temp += 1;

            for (int k = 0; k < 6; k++)
            {
                distance++;

                Vector2Int neighbor = GetNeighbor(neighbors[temp], (TileNeighbor)k);
                if (neighbor == b)
                {
                    return distance;
                }
                else if (!neighbors.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// 타일 좌표를 기반으로 World 좌표를 계산한다.
    /// </summary>
    /// <param name="i">타일 좌표</param>
    /// <returns>World 좌표</returns>
    public static Vector3 GetWorldCoordinate(Vector2Int i)
    {
        float offset = Mathf.Sqrt(3.0f) / 2f;

        float x = i.x * 1.5f;
        float z = i.y * offset * 2f + (i.x % 2 == 0 ? 0.0f : offset);

        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// World 좌표를 기반으로 타일 좌표를 계산한다.
    /// </summary>
    /// <param name="i">World 좌표</param>
    /// <returns>타일 좌표</returns>
    public static Vector2Int GetTileCoordinate(Vector3 i)
    {
        Vector2Int retCoord = new Vector2Int();
        float offset = Mathf.Sqrt(3.0f) / 2f;

        retCoord.x = (int)(i.x / 1.5f);
        retCoord.y = (int)((i.z - (retCoord.x % 2 == 0 ? 0.0f : offset)) / offset / 2f);

        Vector3 minWorldCoord = GetWorldCoordinate(retCoord);
        float minDist = (i.x - minWorldCoord.x) * (i.x - minWorldCoord.x) + (i.z - minWorldCoord.z) * (i.z - minWorldCoord.z);

        Vector2Int[] neighbors = GetNeighbors(retCoord, 1);

        foreach (Vector2Int neighbor in neighbors)
        {
            if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x > MapManager.Instance.Tiles.GetLength(0) || neighbor.y > MapManager.Instance.Tiles.GetLength(1))
            {
                continue;
            }

            Vector3 neighborWorldCoord = GetWorldCoordinate(neighbor);
            float candDist = (i.x - neighborWorldCoord.x) * (i.x - neighborWorldCoord.x) + (i.z - neighborWorldCoord.z) * (i.z - neighborWorldCoord.z);

            if (candDist < minDist)
            {
                minDist = candDist;
                retCoord = neighbor;
            }
        }

        return retCoord;
    }
}
