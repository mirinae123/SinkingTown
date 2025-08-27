using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알림 메뉴 클래스
/// </summary>
public class NotificationUI : BaseUI
{
    [SerializeField] LocalizedText _captionText;
    [SerializeField] LocalizedText _descriptionText;

    [SerializeField] Button _closeIcon;

    /// <summary>
    /// 닫기 가능 여부
    /// </summary>
    public bool IsClosable
    {
        get => _isClosable;
    }
    private bool _isClosable;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Notification, this);

        _closeIcon.onClick.AddListener(UIManager.Instance.HidePanel);

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

            if (values.Length > 2)
            {
                _isClosable = (bool)values[2];
            }
            else
            {
                _isClosable = true;
            }

            _closeIcon.gameObject.SetActive(_isClosable);
        }
    }

    public override void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
