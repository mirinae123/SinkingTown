using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 새 게임 메뉴 클래스
/// </summary>
public class NewGameUI : BaseUI
{
    // 난이도 설정별 지정 값
    private readonly int[] MAP_SIZE = { 64, 128, 256 };
    private readonly float[] FLOOD_FREQUENCY = { 240.0f, 180.0f, 120.0f };
    private readonly float[] PIRATE_FREQUENCY = { 240.0f, 180.0f, 120.0f };

    [SerializeField] TMP_InputField _seed;
    [SerializeField] Button _seedGenerateButton;

    [SerializeField] Button[] _sizeButton;

    [SerializeField] Button _presetEasy;
    [SerializeField] Button _presetMedium;
    [SerializeField] Button _presetHard;

    [SerializeField] Button[] _floodFrequencyButton;

    [SerializeField] Button[] _pirateFrequencyButton;

    [SerializeField] Toggle _canPause;

    [SerializeField] Button _startNewGameButton;
    [SerializeField] Button _quitIcon;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.NewGame, this);

        // 시드
        _seed.onValueChanged.AddListener((seed) =>
        {
            RandomUtility.Seed = seed.GetHashCode();
        });

        _seedGenerateButton.onClick.AddListener(GenerateSeed);

        // 맵 크기
        for (int i = 0; i < _sizeButton.Length; i++)
        {
            int index = i;

            _sizeButton[index].onClick.AddListener(() =>
            {
                SelectSize(index);
            });
        }

        // 난이도 프리셋
        _presetEasy.onClick.AddListener(() =>
        {
            SelectFloodFrequency(0);
            SelectPirateFrequency(0);
            _canPause.isOn = true;
        });

        _presetMedium.onClick.AddListener(() =>
        {
            SelectFloodFrequency(1);
            SelectPirateFrequency(1);
            _canPause.isOn = true;
        });

        _presetHard.onClick.AddListener(() =>
        {
            SelectFloodFrequency(2);
            SelectPirateFrequency(2);
            _canPause.isOn = false;
        });

        // 해수면 상승 빈도
        for (int i = 0; i < _floodFrequencyButton.Length; i++)
        {
            int index = i;

            _floodFrequencyButton[index].onClick.AddListener(() =>
            {
                SelectFloodFrequency(index);
            });
        }

        // 해적 스폰 빈도
        for (int i = 0; i < _pirateFrequencyButton.Length; i++)
        {
            int index = i;

            _pirateFrequencyButton[index].onClick.AddListener(() =>
            {
                SelectPirateFrequency(index);
            });
        }

        // 일시정지 가능 여부
        _canPause.onValueChanged.AddListener((isOn) =>
        {
            SessionManager.Instance.CanPause = isOn;
        });

        _startNewGameButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Confirm, "New Game", "Create New Game?", (UnityAction)(() =>
            {
                UIManager.Instance.HidePanel();
                SceneManager.LoadScene(1);
            }), null);
        });

        _quitIcon.onClick.AddListener(UIManager.Instance.HidePanel);

        // 기본 설정 값
        GenerateSeed();

        SelectSize(1);
        SelectFloodFrequency(1);
        SelectPirateFrequency(1);

        _canPause.isOn = true;
        _canPause.onValueChanged.Invoke(_canPause.isOn);

        transform.parent.gameObject.SetActive(false);
    }

    public override void Show(params object[] values)
    {
        transform.parent.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 랜덤 시드 값을 생성한다.
    /// </summary>
    private void GenerateSeed()
    {
        _seed.text = Random.Range(int.MinValue, int.MaxValue).ToString();
    }

    /// <summary>
    /// 맵 크기를 선택한다.
    /// </summary>
    /// <param name="index">버튼 인덱스</param>
    private void SelectSize(int index)
    {
        foreach(Button sizeButton in _sizeButton)
        {
            sizeButton.interactable = true;
        }

        _sizeButton[index].interactable = false;
        SessionManager.Instance.MapSize = MAP_SIZE[index];
    }

    /// <summary>
    /// 해수면 상승 빈도를 선택한다.
    /// </summary>
    /// <param name="index">버튼 인덱스</param>
    private void SelectFloodFrequency(int index)
    {
        foreach (Button floodFrequencyButton in _floodFrequencyButton)
        {
            floodFrequencyButton.interactable = true;
        }

        _floodFrequencyButton[index].interactable = false;
        SessionManager.Instance.OceanRisePeriod = FLOOD_FREQUENCY[index];
    }

    /// <summary>
    /// 해적 스폰 빈도를 선택한다.
    /// </summary>
    /// <param name="index">버튼 인덱스</param>
    private void SelectPirateFrequency(int index)
    {
        foreach (Button pirateFrequencyButton in _pirateFrequencyButton)
        {
            pirateFrequencyButton.interactable = true;
        }

        _pirateFrequencyButton[index].interactable = false;
        SessionManager.Instance.PirateSpawnPeriod = PIRATE_FREQUENCY[index];
    }
}
