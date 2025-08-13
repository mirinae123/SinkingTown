using UnityEngine;

/// <summary>
/// 유사난수를 생성하는 클래스
/// </summary>
public class RandomUtility
{
    /// <summary>
    /// 시드
    /// </summary>
    public static int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            Random.InitState(_seed);
        }
    }
    private static int _seed;

    /// <summary>
    /// 2D 무작위 패턴을 생성한다.
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <param name="m">최댓값</param>
    /// <returns>[0, m)의 값</returns>
    public static int GetRandomNoise(int x, int y, int m)
    {
        int hash = _seed;
        hash = hash * 31 + x;
        hash = hash * 31 + y;

        System.Random rand = new System.Random(hash);
        
        return rand.Next(0, m);
    }
}
