using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 메뉴 클래스
/// </summary>
public class OptionUI : BaseUI
{
    [SerializeField] private Button _quitIcon;

    private BaseUIAnimator _animator;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Option, this);

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel();
        });

        _animator = GetComponent<BaseUIAnimator>();
    }

    public override void Show(params object[] values)
    {
        _animator.Show();
    }

    public override void Hide()
    {
        _animator.Hide();
    }
}
