using UnityEngine;

/// <summary>
/// 일반 UI의 애니메이션을 관리하는 클래스
/// </summary>
public class BaseUIAnimator : MonoBehaviour
{
    [SerializeField] private PositionTweener _positionTweener;
    [SerializeField] private BackgroundTweener _backgroundTweener;
    [SerializeField] private CanvasGroupTweener _canvasGroupTweener;

    [SerializeField] private float _initialTime = 0.0f;

    private void Start()
    {
        if (_positionTweener)
        {
            _positionTweener.Initialize(_initialTime);
        }
        
        if (_backgroundTweener)
        {
            _backgroundTweener.Initialize(_initialTime);
        }

        if (_canvasGroupTweener)
        {
            _canvasGroupTweener.Initialize(_initialTime);
        }
    }

    public void Show()
    {
        if (_positionTweener)
        {
            _positionTweener.Show();
        }

        if (_backgroundTweener)
        {
            _backgroundTweener.Show();
        }

        if (_canvasGroupTweener)
        {
            _canvasGroupTweener.Show();
        }
    }

    public void Hide()
    {
        if (_positionTweener)
        {
            _positionTweener.Hide();
        }

        if (_backgroundTweener)
        {
            _backgroundTweener.Hide();
        }

        if (_canvasGroupTweener)
        {
            _canvasGroupTweener.Hide();
        }
    }
}
