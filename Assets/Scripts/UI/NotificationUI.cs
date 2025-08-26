using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알림 메뉴 클래스
/// </summary>
public class NotificationUI : BaseUI
{
    [SerializeField] LocalizedText _captionText;
    [SerializeField] LocalizedText _descriptionText;

    [SerializeField] Button _quitIcon;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Notification, this);

        _quitIcon.onClick.AddListener(UIManager.Instance.HidePanel);

        transform.parent.gameObject.SetActive(false);
    }

    public override void Show(params object[] values)
    {
        transform.parent.gameObject.SetActive(true);

        if (values.Length > 0)
        {
            _captionText.ChangeKey((string)values[0]);
            _captionText.UpdateTextLanguage();

            _descriptionText.ChangeKey((string)values[1]);
            _descriptionText.UpdateTextLanguage();
        }
    }

    public override void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
