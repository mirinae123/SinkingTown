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
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;

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
            UIManager.Instance.ShowPanel(PanelType.Confirm, new KeyWrapper("quit_confirm_caption"), new KeyWrapper("quit_confirm_description"), (UnityAction)(() =>
            {
                SceneLoadManager.Instance.LoadScene(0);
            }),
            null);
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
