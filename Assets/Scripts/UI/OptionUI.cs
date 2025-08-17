using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 메뉴 클래스
/// </summary>
public class OptionUI : BaseUI
{
    [SerializeField] private Button _quitIcon;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Option, this);

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel();
        });

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
}
