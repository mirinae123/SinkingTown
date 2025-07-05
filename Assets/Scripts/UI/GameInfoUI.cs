using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 게임 정보 클래스
/// </summary>
public class GameInfoUI : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private TMP_Text _currentDayText;
    [SerializeField] private EventTrigger _currentDayInfo;
    [SerializeField] private Slider _timeTillRiseSlider;
    [SerializeField] private EventTrigger _timeTillRiseSliderInfo;
    [SerializeField] private EventTrigger _woodInfo;
    [SerializeField] private EventTrigger _stoneInfo;
    [SerializeField] private EventTrigger _researchInfo;
    [SerializeField] private TMP_Text _researchText;

    void Start()
    {
        _mainMenuButton.onClick.AddListener(() => { OnMainMenuOpen(); });

        UIManager.Instance.AddHoverEvent(_currentDayInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_timeTillRiseSliderInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_woodInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_stoneInfo, "option_title", "language_label", HoverDirection.TopLeft);
        UIManager.Instance.AddHoverEvent(_researchInfo, "option_title", "language_label", HoverDirection.TopLeft);
    }

    private void Update()
    {
        _currentDayText.text = "DAY " + GameManager.Instance.CurrentDay;
        _timeTillRiseSlider.value = GameManager.Instance.TimeSinceOceanRise / GameManager.Instance.OceanRisePeriod;
        _researchText.text = (int)((float)GameManager.Instance.CurrentResearchPoint / GameManager.Instance.MaxResearchPoint * 100) + "%";
    }

    void OnMainMenuOpen()
    {
        UIManager.Instance.ShowMainMenu();
    }
}
