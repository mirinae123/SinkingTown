using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배경화면 트위너
/// </summary>
public class BackgroundTweener : UITweener
{
    [SerializeField] private Image _background;

    [SerializeField] private Color _startColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    [SerializeField] private Color _endColor = new Color(0.0f, 0.0f, 0.0f, 0.2f);

    protected override void OnUpdate(float normalizedTime)
    {
        _background.color = Color.Lerp(_startColor, _endColor, normalizedTime);
        _background.raycastTarget = (normalizedTime > 0.0f);
    }
}
