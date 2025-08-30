using UnityEngine;

/// <summary>
/// UI 위치 트위너
/// </summary>
public class PositionTweener : UITweener
{
    [SerializeField] private RectTransform _rectTransform;

    [SerializeField] private Vector2 _startPosition = new Vector2(0.0f, 100.0f);
    [SerializeField] private Vector2 _endPosition = new Vector2(0.0f, 0.0f);

    protected override void OnUpdate(float normalizedTime)
    {
        float a = EaseInOutBack(normalizedTime);
        _rectTransform.anchoredPosition = _startPosition * (1.0f - a) + _endPosition * a;
    }
}
