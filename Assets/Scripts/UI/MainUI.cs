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

    private UIAnimator _animator;

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

        _animator = GetComponent<UIAnimator>();
        _animator.InitializeAnimation();
    }

    public override void Show(params object[] values)
    {
        _animator.PlayShowAnimation();

        //_saveButton.interactable = true;
        //_loadButton.interactable = true;
        //_optionButton.interactable = true;
        //_resumeButton.interactable = true;
        //_quitButton.interactable = true;

        //_quitIcon.interactable = true;
    }

    public override void Hide()
    {
        _animator.PlayHideAnimation();

        //_saveButton.interactable = false;
        //_loadButton.interactable = false;
        //_optionButton.interactable = false;
        //_resumeButton.interactable = false;
        //_quitButton.interactable = false;

        //_quitIcon.interactable = false;
    }
}
