using UnityEngine;

/// <summary>
/// UI 애니메이션 상태
/// </summary>
public enum UIAnimationState { Idle, Show, Hide }

/// <summary>
/// 모든 UI 트위너의 기반이 되는 클래스
/// </summary>
public class UITweener : MonoBehaviour
{
    [SerializeField] private float _duration = 0.3f;

    private UIAnimationState _animationState = UIAnimationState.Idle;
    private float _animationTime = 0.0f;

    private void Update()
    {
        if (_animationState == UIAnimationState.Show)
        {
            _animationTime += Time.deltaTime;

            if (_animationTime > _duration)
            {
                _animationTime = _duration;
                _animationState = UIAnimationState.Idle;
            }

            OnUpdate(_animationTime / _duration);
        }

        if (_animationState == UIAnimationState.Hide)
        {
            _animationTime -= Time.deltaTime;

            if (_animationTime < 0.0f)
            {
                _animationTime = 0.0f;
                _animationState = UIAnimationState.Idle;
            }

            OnUpdate(_animationTime / _duration);
        }
    }

    protected virtual void OnUpdate(float normalizedTime) { }

    protected float EaseInBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1.0f;

        return c3 * Mathf.Pow(x, 3) - c1 * Mathf.Pow(x, 2);
    }

    protected float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1.0f;

        return 1.0f + c3 * Mathf.Pow(x - 1.0f, 3) + c1 * Mathf.Pow(x - 1.0f, 2);
    }

    protected float EaseInOutBack(float x)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        if (x < 0.5f)
        {
            return (Mathf.Pow(2.0f * x, 2) * ((c2 + 1.0f) * 2.0f * x - c2)) / 2.0f;
        }
        else
        {
            return (Mathf.Pow(2.0f * x - 2.0f, 2) * ((c2 + 1.0f) * (x * 2.0f - 2.0f) + c2) + 2.0f) / 2.0f;
        }
    }

    public virtual void Initialize(float normalizedTime)
    {
        _animationTime = normalizedTime * _duration;
        _animationState = UIAnimationState.Idle;

        OnUpdate(normalizedTime);
    }

    public virtual void Show()
    {
        _animationState = UIAnimationState.Show;
    }

    public virtual void Hide()
    {
        _animationState = UIAnimationState.Hide;
    }
}
