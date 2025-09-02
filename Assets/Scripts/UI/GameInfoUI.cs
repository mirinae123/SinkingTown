using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 게임 정보 클래스
/// </summary>
public class GameInfoUI : BaseUI
{
    [SerializeField] private Button _mainMenuButton;

    [SerializeField] private LocalizedText _currentDayText;
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

        // 현재 날짜
        UIManager.Instance.AddHoverEvent(_currentDayInfo, new KeyWrapper("current_day_hover_caption"), new KeyWrapper("current_day_hover_description", ""), HoverDirection.TopLeft, (UnityAction)(() =>
        {
            HoverUI hoverUI = (HoverUI)UIManager.Instance.Panels[PanelType.Hover];
            
            if (PirateManager.Instance.Pirates.Count > 0)
            {
                hoverUI.UpdateHoverText(new KeyWrapper("current_day_hover_caption"), new KeyWrapper("current_day_invade_hover_description", GameManager.Instance.CurrentDay));
            }
            else
            {
                hoverUI.UpdateHoverText(new KeyWrapper("current_day_hover_caption"), new KeyWrapper("current_day_hover_description", GameManager.Instance.CurrentDay));
            }
        }));

        // 해수면 상승까지 남은 시간
        UIManager.Instance.AddHoverEvent(_timeTillRiseSliderInfo, new KeyWrapper("flood_time_hover_caption"), new KeyWrapper("flood_time_hover_description", ""), HoverDirection.TopLeft, (UnityAction)(() =>
        {
            HoverUI hoverUI = (HoverUI)UIManager.Instance.Panels[PanelType.Hover];

            if (MapRenderer.Instance.IsOceanRising)
            {
                hoverUI.UpdateHoverText(new KeyWrapper("flood_time_hover_caption"), new KeyWrapper("flood_time_increasing_hover_description"));
            }
            else if (MapManager.Instance.OceanLevel == MapGenerator.Instance.MaxHeight)
            {
                hoverUI.UpdateHoverText(new KeyWrapper("flood_time_hover_caption"), new KeyWrapper("flood_time_stop_hover_description"));
            }
            else
            {
                hoverUI.UpdateHoverText(new KeyWrapper("flood_time_hover_caption"), new KeyWrapper("flood_time_hover_description", (int)(SaveManager.Instance.SaveData.OceanRisePeriod - GameManager.Instance.TimeSinceOceanRise)));
            }
        }));

        // 현재 자원
        UIManager.Instance.AddHoverEvent(_woodInfo, new KeyWrapper("current_wood_hover_caption"), new KeyWrapper("current_wood_hover_description"), HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_stoneInfo, new KeyWrapper("current_stone_hover_caption"), new KeyWrapper("current_stone_hover_description"), HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_researchInfo, new KeyWrapper("current_research_point_hover_caption"), new KeyWrapper("current_research_point_hover_description"), HoverDirection.TopLeft);

        // 일시 정지
        if (SaveManager.Instance.SaveData.CanPause)
        {
            _pauseButton.onClick.AddListener(() => { GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused; });
            UIManager.Instance.AddHoverEvent(_pauseButtonInfo, new KeyWrapper("pause_button_hover_caption"), new KeyWrapper("pause_button_enabled_hover_description"), HoverDirection.TopLeft, (UnityAction)(() =>
            {
                HoverUI hoverUI = (HoverUI)UIManager.Instance.Panels[PanelType.Hover];

                if (GameManager.Instance.IsPaused)
                {
                    hoverUI.UpdateHoverText(new KeyWrapper("pause_button_hover_caption"), new KeyWrapper("pause_button_play_hover_description"));
                }
                else
                {
                    hoverUI.UpdateHoverText(new KeyWrapper("pause_button_hover_caption"), new KeyWrapper("pause_button_pause_hover_description"));
                }
            })
            );
        }
        else
        {
            _pauseButton.interactable = false;
            UIManager.Instance.AddHoverEvent(_pauseButtonInfo, new KeyWrapper("pause_button_hover_caption"), new KeyWrapper("pause_button_disabled_hover_description"), HoverDirection.TopLeft);
        }

        _animator = GetComponent<BaseUIAnimator>();
    }

    private void Update()
    {
        _currentDayText.ChangeKey("current_day", GameManager.Instance.CurrentDay);
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
