using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 개별 대포알을 관리하는 클래스
/// </summary>
public class CannonballController : MonoBehaviour
{
    private const float CANNONBALL_DURATION_MODIFIER = 6.0f;
    private const float CANNONBALL_GRAVITY = 16.0f;

    private Vector3 _velocity;

    private float _duration;
    private float _elapsed;

    private PirateController _targetPirate;

    /// <summary>
    /// 대포알의 상태를 초기화한다
    /// </summary>
    /// <param name="startPosition">시작 위치</param>
    /// <param name="endPosition">종료 위치</param>
    public void Initialize(Vector3 startPosition, Vector3 endPosition, PirateController targetPirate = null)
    {
        _duration = Vector3.Distance(startPosition, endPosition) / CANNONBALL_DURATION_MODIFIER;
        _velocity = (endPosition - startPosition) / _duration + Vector3.up * CANNONBALL_GRAVITY * _duration / 2.0f;

        _targetPirate = targetPirate;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.Menu)
        {
            _elapsed += Time.deltaTime;

            transform.Translate(_velocity * Time.deltaTime);
            _velocity -= Vector3.up * CANNONBALL_GRAVITY * Time.deltaTime;

            if (_elapsed > _duration)
            {
                if (_targetPirate != null)
                {
                    _targetPirate.EndAttackPirate();
                }

                PirateManager.Instance.DespawnCannonball(this);
            }
        }
    }

    /// <summary>
    /// 대포알에 대한 세이브 데이터를 생성한다.
    /// </summary>
    /// <returns>세이브 데이터</returns>
    public CannonballSaveData GetSaveData()
    {
        CannonballSaveData saveData = new CannonballSaveData();

        saveData.Position = transform.position;
        saveData.Velocity = _velocity;

        saveData.Duration = _duration;
        saveData.Elapsed = _elapsed;

        saveData.TargetPirateIndex = -1;

        for (int i = 0; i < PirateManager.Instance.Pirates.Count; i++)
        {
            if (PirateManager.Instance.Pirates[i] == _targetPirate)
            {
                saveData.TargetPirateIndex = i;
                break;
            }
        }

        return saveData;
    }

    /// <summary>
    /// 저장된 대포알을 불러온다.
    /// </summary>
    /// <param name="saveData">세이브 데이터</param>
    public void LoadSaveData(CannonballSaveData saveData)
    {
        transform.position = saveData.Position;
        _velocity = saveData.Velocity;

        _duration = saveData.Duration;
        _elapsed = saveData.Elapsed;

        if (saveData.TargetPirateIndex != -1)
        {
            _targetPirate = PirateManager.Instance.Pirates[saveData.TargetPirateIndex];
        }
    }
}
