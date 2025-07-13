using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클리어 메뉴 클래스
/// </summary>
public class EndMenuUI : MonoBehaviour
{
    [SerializeField] LocalizedText _captionText;

    [SerializeField] Image _image;

    [SerializeField] LocalizedText _firstDescriptionText;
    [SerializeField] LocalizedText _secondDescriptionText;

    [SerializeField] LocalizedText _scoreLabelText;
    [SerializeField] TMP_Text _scoreText;

    [SerializeField] Button _quitIcon;

    private void Start()
    {
        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HideEndMenu();
        });
    }

    public void Show(bool hasCleared)
    {
        transform.parent.gameObject.SetActive(true);

        if (hasCleared)
        {
            _captionText.ChangeKey("option_title");

            _firstDescriptionText.ChangeKey("option_title");
            _secondDescriptionText.ChangeKey("option_title");

            _scoreLabelText.ChangeKey("option_title");
            _scoreText.text = "2500";
        }
        else
        {
            _captionText.ChangeKey("language_label");

            _firstDescriptionText.ChangeKey("language_label");
            _secondDescriptionText.ChangeKey("language_label");

            _scoreLabelText.ChangeKey("language_label");
            _scoreText.text = "0";
        }
    }

    public void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
