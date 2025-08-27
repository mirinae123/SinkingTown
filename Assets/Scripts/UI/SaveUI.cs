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

        transform.parent.gameObject.SetActive(false);
    }

    public override void Show(params object[] values)
    {
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        int saveDataButtonCount = SaveManager.Instance.PopulateSaveDataButtons(_content.transform, false);

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
