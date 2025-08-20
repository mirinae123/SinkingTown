/// <summary>
/// 현재 게임의 세션 정보를 저장하는 클래스
/// </summary>
public class SessionManager : SingletonBehaviour<SessionManager>
{
    /// <summary>
    /// 맵 크기
    /// </summary>
    public int MapSize;

    /// <summary>
    /// 해수면 상승 간격
    /// </summary>
    public float OceanRisePeriod;

    /// <summary>
    /// 해적 스폰 간격
    /// </summary>
    public float PirateSpawnPeriod;

    /// <summary>
    /// 일시정지 가능 여부
    /// </summary>
    public bool CanPause;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
