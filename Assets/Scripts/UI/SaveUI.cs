using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 저장 메뉴 클래스
/// </summary>
public class SaveUI : BaseUI
{
    [SerializeField] GameObject _content;
    [SerializeField] GameObject _emptyContentText;

    [SerializeField] Button _saveAsNewButton;
    [SerializeField] Button _quitIcon;

    void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Save, this);

        _saveAsNewButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Confirm, "Save?", "Save?", (UnityAction)(() =>
            {
                SaveManager.Instance.SaveGame();
            }), null);
        });

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
        SaveManager.Instance.PopulateSaveDataButtons(_content.transform, _emptyContentText, false);
    }
}
