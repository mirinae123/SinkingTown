using UnityEngine;

/// <summary>
/// 캔버스 그룹 트위너
/// </summary>
public class CanvasGroupTweener : UITweener
{
    [SerializeField] private CanvasGroup _canvasGroup;

    protected override void OnUpdate(float normalizedTime)
    {
        _canvasGroup.alpha = normalizedTime;
        _canvasGroup.interactable = (normalizedTime == 1.0f);
        _canvasGroup.blocksRaycasts = (normalizedTime == 1.0f);
    }
}
