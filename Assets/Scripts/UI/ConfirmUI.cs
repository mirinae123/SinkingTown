
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 확인 메뉴 클래스
/// </summary>
public class ConfirmUI : BaseUI
{
    [SerializeField] LocalizedText _captionText;
    [SerializeField] LocalizedText _descriptionText;

    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;

    private BaseUIAnimator _animator;

    private bool _isConfirmed;

    private UnityAction _onConfirm;
    private UnityAction _OnCancel;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Confirm, this);

        _animator = GetComponent<BaseUIAnimator>();
    }

    public override void Show(params object[] values)
    {
        _animator.Show();

        _captionText.ChangeKey(((KeyWrapper)values[0]).key, ((KeyWrapper)values[0]).parameters);
        _descriptionText.ChangeKey(((KeyWrapper)values[1]).key, ((KeyWrapper)values[1]).parameters);

        _confirmButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.RemoveAllListeners();

        _confirmButton.onClick.AddListener(() => { _isConfirmed = true; UIManager.Instance.HidePanel(); });
        _cancelButton.onClick.AddListener(() => { _isConfirmed = false; UIManager.Instance.HidePanel(); });

        _isConfirmed = false;

        _onConfirm = (UnityAction)values[2];
        _OnCancel = (UnityAction)values[3];
    }

    public override void Hide()
    {
        if (_isConfirmed)
        {
            _onConfirm?.Invoke();
        }
        else
        {
            _OnCancel?.Invoke();
        }

        _animator.Hide();
    }
}
