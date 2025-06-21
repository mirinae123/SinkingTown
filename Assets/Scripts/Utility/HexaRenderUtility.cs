using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 육각형 타일을 렌더링할 때 필요한 정보를 담은 클래스
/// </summary>
public class HexaRenderUtility
{
    // Top vertices index:
    //     1 == 2
    //   //      \\
    //   0        3
    //   \\      //
    //     5 == 4

    // Bottom vertices index:
    //     7 == 8
    //   //      \\
    //   6        9
    //   \\      //
    //    11 == 10

    private static Vector3[] _vertices = {
        new Vector3(-1.0f, 1.0f, 0.0f),
        new Vector3(-0.5f, 1.0f, Mathf.Sqrt(3.0f) / 2f),
        new Vector3(0.5f, 1.0f, Mathf.Sqrt(3.0f) / 2f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.5f, 1.0f, -Mathf.Sqrt(3.0f) / 2f),
        new Vector3(-0.5f, 1.0f, -Mathf.Sqrt(3.0f) / 2f),

        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(-0.5f, 0.0f, Mathf.Sqrt(3.0f) / 2f),
        new Vector3(0.5f, 0.0f, Mathf.Sqrt(3.0f) / 2f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(0.5f, 0.0f, -Mathf.Sqrt(3.0f) / 2f),
        new Vector3(-0.5f, 0.0f, -Mathf.Sqrt(3.0f) / 2f),

        new Vector3(0.0f, 1.0f, 0.0f)   // Top
    };
    public static Vector3[] Vertices {
        get => _vertices;
    }

    private static Vector2[] _uv = {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };
    public static Vector2[] Uv
    {
        get => _uv;
    }

    // Triangles order:
    //     * 1= *
    //   0/      2\
    //   *        *
    //   5\      3/
    //     * 4= *

    private static int[][] _triangles = {
        new int[]{ 7, 1, 6, 6, 1, 0 },
        new int[]{ 8, 2, 7, 7, 2, 1 },
        new int[]{ 9, 3, 8, 8, 3, 2 },
        new int[]{ 10, 4, 9, 9, 4, 3 },
        new int[]{ 11, 5, 10, 10, 5, 4 },
        new int[]{ 6, 0, 11, 11, 0, 5 }
    };
    public static int[][] Triangles
    {
        get => _triangles;
    }

    private static int[] _topTriangles = {
        0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 1
    };
    public static int[] TopTriangles
    {
        get => _topTriangles;
    }

    private static Vector2[] _topUv = {
        new Vector2(0.5f, 0.5f), new Vector2(0.0f, 0.5f), new Vector2(0.25f, 1.0f),
         new Vector2(0.75f, 1.0f),
         new Vector2(1.0f, 0.5f),
         new Vector2(0.75f, 0.0f),
         new Vector2(0.25f, 0.0f)
    };
    public static Vector2[] TopUv
    {
        get => _topUv;
    }
}
