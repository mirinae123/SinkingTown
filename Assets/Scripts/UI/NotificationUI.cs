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

    private BaseUIAnimator _animator;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Notification, this);

        _closeIcon.onClick.AddListener(UIManager.Instance.HidePanel);

        _animator = GetComponent<BaseUIAnimator>();
    }

    public override void Show(params object[] values)
    {
        _animator.Show();

        if (values.Length > 0)
        {
            _captionText.ChangeKey(((KeyWrapper)values[0]).key, ((KeyWrapper)values[0]).parameters);
            _descriptionText.ChangeKey(((KeyWrapper)values[1]).key, ((KeyWrapper)values[1]).parameters);

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
        _animator.Hide();
    }
}
