using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 게임 정보 클래스
/// </summary>
public class GameInfoUI : BaseUI
{
    [SerializeField] private Button _mainMenuButton;

    [SerializeField] private TMP_Text _currentDayText;
    [SerializeField] private EventTrigger _currentDayInfo;

    [SerializeField] private Slider _timeTillRiseSlider;
    [SerializeField] private EventTrigger _timeTillRiseSliderInfo;

    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private EventTrigger _woodInfo;
    [SerializeField] private TMP_Text _stoneText;
    [SerializeField] private EventTrigger _stoneInfo;

    [SerializeField] private TMP_Text _researchText;
    [SerializeField] private EventTrigger _researchInfo;

    [SerializeField] private Button _pauseButton;
    [SerializeField] private EventTrigger _pauseButtonInfo;

    private BaseUIAnimator _animator;

    void Start()
    {
        _mainMenuButton.onClick.AddListener(() => { UIManager.Instance.ShowPanel(PanelType.Main); });

        if (SaveManager.Instance.SaveData.CanPause)
        {
            _pauseButton.onClick.AddListener(() => { GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused; });
            UIManager.Instance.AddHoverEvent(_pauseButtonInfo, "option_title", "language_label", HoverDirection.TopLeft);
        }
        else
        {
            _pauseButton.interactable = false;
            UIManager.Instance.AddHoverEvent(_pauseButtonInfo, "option_title", "language_label", HoverDirection.TopLeft);
        }

        UIManager.Instance.AddHoverEvent(_currentDayInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_timeTillRiseSliderInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_woodInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_stoneInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_researchInfo, "option_title", "language_label", HoverDirection.TopLeft);

        _animator = GetComponent<BaseUIAnimator>();
    }

    private void Update()
    {
        _currentDayText.text = "DAY " + GameManager.Instance.CurrentDay;
        _timeTillRiseSlider.value = GameManager.Instance.TimeSinceOceanRise / SaveManager.Instance.SaveData.OceanRisePeriod;

        _woodText.text = GameManager.Instance.CurrentWoods.ToString();
        _stoneText.text = GameManager.Instance.CurrentStones.ToString();

        _researchText.text = (int)((float)GameManager.Instance.CurrentResearchPoint / GameManager.Instance.MaxResearchPoint * 100) + "%";
    }

    public override void Show(params object[] values)
    {
        _animator.Show();
    }

    public override void Hide()
    {
        _animator.Hide();
    }
}
