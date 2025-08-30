using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 클래스
/// </summary>
public class MainUI : BaseUI
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private Button _quitIcon;

    private BaseUIAnimator _animator;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Main, this);

        _saveButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Save);
        });

        _optionButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Option);
        });

        _resumeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel();
        });

        _quitButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Confirm, "QUIT", "QUIT", (UnityAction)(() =>
            {
                SceneLoadManager.Instance.LoadScene(0);
            }),
            null);
        });

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
