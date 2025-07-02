using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 확인 메뉴 클래스
/// </summary>
public class ConfirmMenuUI : MonoBehaviour
{
    [SerializeField] LocalizedText _captionText;
    [SerializeField] LocalizedText _descriptionText;

    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;

    private UnityAction _onConfirm;
    private UnityAction _onCancel;

    public void Show(string caption, string description, UnityAction onConfirm, UnityAction onCancel)
    {
        transform.parent.gameObject.SetActive(true);

        _captionText.ChangeKey(caption);
        _captionText.UpdateTextLanguage();

        _descriptionText.ChangeKey(description);
        _descriptionText.UpdateTextLanguage();

        _confirmButton.onClick.AddListener(() => { UIManager.Instance.HideConfirmMenu(true); });
        _cancelButton.onClick.AddListener(() => { UIManager.Instance.HideConfirmMenu(false); });

        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }

    public void Hide(bool isConfirmed)
    {
        if (isConfirmed)
        {
            _onConfirm?.Invoke();
        }
        else
        {
            _onCancel?.Invoke();
        }
        
        transform.parent.gameObject.SetActive(false);
    }
}
