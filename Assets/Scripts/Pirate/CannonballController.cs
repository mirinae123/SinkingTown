using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 개별 대포알을 관리하는 클래스
/// </summary>
public class CannonballController : MonoBehaviour
{
    private const float CANNONBALL_DURATION_MODIFIER = 6.0f;
    private const float CANNONBALL_GRAVITY = 16.0f;

    private Vector3 _startPosition;
    private Vector3 _endPosition;

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
        _startPosition = startPosition;
        _endPosition = endPosition;

        _duration = Vector3.Distance(_startPosition, _endPosition) / CANNONBALL_DURATION_MODIFIER;
        _velocity = (_endPosition - _startPosition) / _duration + Vector3.up * CANNONBALL_GRAVITY * _duration / 2.0f;

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

                Destroy(gameObject);
            }
        }
    }
}
