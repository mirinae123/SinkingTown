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

    void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Load, this);

        _quitIcon.onClick.AddListener(UIManager.Instance.HidePanel);

        UpdateSaveDataList();

        transform.parent.gameObject.SetActive(false);
    }

    public override void Show(params object[] values)
    {
        transform.parent.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 메뉴의 세이브 데이터 목록을 갱신한다.
    /// </summary>
    public void UpdateSaveDataList()
    {
        SaveManager.Instance.PopulateSaveDataButtons(_content.transform, _emptyContentText, true);
    }
}
