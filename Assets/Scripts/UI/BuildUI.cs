using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 건설 메뉴 클래스
/// </summary>
public class BuildUI : BaseUI
{
    [SerializeField] private Button _buildIcon;
    [SerializeField] private Button _quitIcon;

    [SerializeField] private GameObject _content;

    [SerializeField] private Animator _animator;

    private Button[] _buttons;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Build, this);

        _buildIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Build);
        });

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel();
        });

        _buttons = _content.GetComponentsInChildren<Button>();
    }

    public override void Show(params object[] values)
    {
        _animator.SetBool("IsOpen", true);
        GameManager.Instance.ChangeGameState(GameState.None);

        foreach (Button button in _buttons)
        {
            button.enabled = true;
        }
    }

    public override void Hide()
    {
        _animator.SetBool("IsOpen", false);

        foreach (Button button in _buttons)
        {
            button.enabled = false;
        }
    }
}
