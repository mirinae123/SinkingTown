using UnityEngine;

/// <summary>
/// UI 크기 트위너
/// </summary>
public class SizeTweener : UITweener
{
    [SerializeField] private RectTransform _rectTransform;

    [SerializeField] private Vector2 _startSize;
    [SerializeField] private Vector2 _endSize;

    protected override void OnUpdate(float normalizedTime)
    {
        float a = EaseOutBack(normalizedTime);
        _rectTransform.sizeDelta = _startSize * (1.0f - a) + _endSize * a;
    }
}
