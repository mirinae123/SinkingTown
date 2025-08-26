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

        transform.parent.gameObject.SetActive(false);
    }

    public override void Show(params object[] values)
    {
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        int saveDataButtonCount = SaveManager.Instance.PopulateSaveDataButtons(_content.transform, true);

        if (saveDataButtonCount > 0)
        {
            _emptyContentText.SetActive(false);
        }
        else
        {
            _emptyContentText.SetActive(true);
        }

        transform.parent.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
