using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클리어 메뉴 클래스
/// </summary>
public class EndUI : BaseUI
{
    [SerializeField] LocalizedText _captionText;

    [SerializeField] Image _image;

    [SerializeField] LocalizedText _firstDescriptionText;
    [SerializeField] LocalizedText _secondDescriptionText;

    [SerializeField] LocalizedText _scoreLabelText;
    [SerializeField] TMP_Text _scoreText;

    [SerializeField] Button _quitIcon;

    private UIAnimator _animator;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.End, this);

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel();
        });

        _animator = GetComponent<UIAnimator>();
        _animator.InitializeAnimation();
    }

    public override void Show(params object[] values)
    {
        _animator.PlayShowAnimation();

        if ((bool)values[0])
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

    public override void Hide()
    {
        _animator.PlayHideAnimation();
    }
}
