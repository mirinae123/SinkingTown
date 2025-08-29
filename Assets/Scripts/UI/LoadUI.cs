using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

/// <summary>
/// 불러오기 메뉴 클래스
/// </summary>
public class LoadUI : BaseUI
{
    [SerializeField] GameObject _content;
    [SerializeField] GameObject _emptyContentText;

    [SerializeField] Button _quitIcon;

    private UIAnimator _animator;

    void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Load, this);

        _quitIcon.onClick.AddListener(UIManager.Instance.HidePanel);

        UpdateSaveDataList();

        _animator = GetComponent<UIAnimator>();
        _animator.InitializeAnimation();
    }

    public override void Show(params object[] values)
    {
        _animator.PlayShowAnimation();
    }

    public override void Hide()
    {
        _animator.PlayHideAnimation();
    }

    /// <summary>
    /// 메뉴의 세이브 데이터 목록을 갱신한다.
    /// </summary>
    public void UpdateSaveDataList()
    {
        SaveManager.Instance.PopulateSaveDataButtons(_content.transform, _emptyContentText, true);
    }
}
