using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 애니메이션 상태
/// </summary>
public enum UIAnimationState { Idle, Show, Hide }

/// <summary>
/// UI 애니메이션을 관리하는 클래스
/// </summary>
public class UIAnimator : MonoBehaviour
{
    private const float ANIMATION_DURATION = 0.5f;

    [SerializeField] private Image _background;
    [SerializeField] private RectTransform _windowRectTransform;
    [SerializeField] private CanvasGroup _windowCanvasGroup;

    [SerializeField] private Vector2 _showPosition = new Vector2(0.0f, 0.0f);
    [SerializeField] private Vector2 _hidePosition = new Vector2(0.0f, 1000.0f);

    private UIAnimationState _animationState = UIAnimationState.Idle;

    private float _animationTime = 0.0f;

    private void Update()
    {
        if (_animationState == UIAnimationState.Show)
        {
            _animationTime += Time.deltaTime;

            if (_animationTime > ANIMATION_DURATION)
            {
                _animationTime = ANIMATION_DURATION;
                _animationState = UIAnimationState.Idle;
            }

            ProcessAnimation();
        }

        if (_animationState == UIAnimationState.Hide)
        {
            _animationTime -= Time.deltaTime;

            if (_animationTime < 0.0f)
            {
                _animationTime = 0.0f;
                _animationState = UIAnimationState.Idle;
            }

            ProcessAnimation();
        }
    }

    private void ProcessAnimation()
    {
        if (_background)
        {
            _background.color = new Color(0.0f, 0.0f, 0.0f, Mathf.Lerp(0.0f, 0.2f, _animationTime / ANIMATION_DURATION));
            _background.raycastTarget = (_animationTime > 0.0f);
        }

        if (_windowRectTransform)
        {
            float a = EaseInOutBack(_animationTime / ANIMATION_DURATION);
            _windowRectTransform.anchoredPosition = _hidePosition * (1.0f - a) + _showPosition * a;
        }

        if (_windowCanvasGroup)
        {
            _windowCanvasGroup.alpha = _animationTime / ANIMATION_DURATION;
            _windowCanvasGroup.interactable = (_animationTime == ANIMATION_DURATION);
        }
    }

    private float EaseInOutBack(float x)
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

    /// <summary>
    /// 애니메이션을 초기화한다.
    /// </summary>
    /// <param name="initialState">초기 상태</param>
    public void InitializeAnimation(UIAnimationState initialState = UIAnimationState.Hide)
    {
        _animationState = initialState;
        _animationTime = initialState == UIAnimationState.Hide ? 0.0f : ANIMATION_DURATION;

        ProcessAnimation();
    }

    /// <summary>
    /// 열기 애니메이션을 재생한다.
    /// </summary>
    public void PlayShowAnimation()
    {
        _animationState = UIAnimationState.Show;
    }

    /// <summary>
    /// 닫기 애니메이션을 재생한다.
    /// </summary>
    public void PlayHideAnimation()
    {
        _animationState = UIAnimationState.Hide;
    }
}
