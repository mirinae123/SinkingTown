using UnityEngine;

/// <summary>
/// 건설 UI의 애니메이션을 관리하는 클래스
/// </summary>
public class BuildUIAnimator : MonoBehaviour
{
    [SerializeField] private PositionTweener _positionTweener;

    [SerializeField] private CanvasGroupTweener _outercanvasGroupTweener;
    [SerializeField] private CanvasGroupTweener _innerCanvasGroupTweener;

    [SerializeField] private SizeTweener _outerSizeTweener;
    [SerializeField] private SizeTweener _innerSizeTweener;

    private void Start()
    {
        _positionTweener.Initialize(1.0f);
        _outercanvasGroupTweener.Initialize(1.0f);

        _innerCanvasGroupTweener.Initialize(0.0f);
        _outerSizeTweener.Initialize(0.0f);
        _innerSizeTweener.Initialize(0.0f);
    }

    public void Show()
    {
        _positionTweener.Show();
        _outercanvasGroupTweener.Show();
    }

    public void Hide()
    {
        _positionTweener.Hide();
        _outercanvasGroupTweener.Hide();
    }

    public void Open()
    {
        _outerSizeTweener.Show();
        _innerSizeTweener.Show();
        _innerCanvasGroupTweener.Show();
    }

    public void Close()
    {
        _outerSizeTweener.Hide();
        _innerSizeTweener.Hide();
        _innerCanvasGroupTweener.Hide();
    }
}
