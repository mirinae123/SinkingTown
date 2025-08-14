using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 타일 정보 클래스
/// </summary>
public class TileInfoUI : MonoBehaviour
{
    private const int RESEARCH_COST_WOODS = 10;
    private const int RESEARCH_COST_STONES = 10;

    [SerializeField] private GameObject _structureButtonPrefab;

    [Header("Title")]
    [SerializeField] private LocalizedText _titleText;
    [SerializeField] private Image _structureImage;

    [SerializeField] private EventTrigger _happinessInfo;
    [SerializeField] private EventTrigger _timeToProduceInfo;
    [SerializeField] private EventTrigger _researchInfo;
    [SerializeField] private EventTrigger _fortressInfo;
    [SerializeField] private Slider _gaugeSlider;
    [SerializeField] private TMP_Text _gaugeText;

    [SerializeField] private Button _destroyButton;
    [SerializeField] private EventTrigger _destroyButtonInfo;
    [SerializeField] private Button _researchButton;
    [SerializeField] private EventTrigger _researchButtonInfo;

    [Header("Needs")]
    [SerializeField] private EventTrigger _resourceInfo;
    [SerializeField] private EventTrigger _populationInfo;
    [SerializeField] private EventTrigger _fishInfo;
    [SerializeField] private EventTrigger _foodInfo;
    [SerializeField] private EventTrigger _cottonInfo;
    [SerializeField] private EventTrigger _clotheInfo;
    [SerializeField] private EventTrigger _rangeBonusInfo;

    [SerializeField] private TMP_Text _populationCount;
    [SerializeField] private TMP_Text _fishCount;
    [SerializeField] private TMP_Text _foodCount;
    [SerializeField] private TMP_Text _cottonCount;
    [SerializeField] private TMP_Text _clotheCount;
    [SerializeField] private TMP_Text _rangeBonus;

    [Header("Provides to")]
    [SerializeField] private EventTrigger _providesToInfo;
    [SerializeField] private GameObject _providesToContent;

    [Header("Provided from")]
    [SerializeField] private EventTrigger _providedFromInfo;
    [SerializeField] private GameObject _providedFromContent;

    [SerializeField] private Button _quitIcon;

    private Tile _currentTile;
    public Tile CurrentTile
    {
        get => _currentTile;
    }

    void Start()
    {
        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HideTileInfo();
        });

        _destroyButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowConfirmMenu("language_label", "language_label", () =>
            {
                _currentTile.DestroyStructure();
            },
            null);
        });

        _researchButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowConfirmMenu("language_label", "language_label", () =>
            {
                GameManager.Instance.CurrentWoods -= RESEARCH_COST_WOODS;
                GameManager.Instance.CurrentStones -= RESEARCH_COST_STONES;

                GameManager.Instance.ChangeResearchPoint(4);
                GameManager.Instance.StartResearchCooldown();
            },
            null);
        });

        UIManager.Instance.AddHoverEvent(_happinessInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_timeToProduceInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_researchInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_fortressInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_researchButtonInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_destroyButtonInfo, "language_label", "language_label", HoverDirection.TopRight);

        UIManager.Instance.AddHoverEvent(_resourceInfo, "language_label", "language_label", HoverDirection.TopRight);

        UIManager.Instance.AddHoverEvent(_populationInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_fishInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_foodInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_cottonInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_clotheInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_rangeBonusInfo, "language_label", "language_label", HoverDirection.TopRight);

        UIManager.Instance.AddHoverEvent(_providesToInfo, "language_label", "language_label", HoverDirection.TopRight);
        UIManager.Instance.AddHoverEvent(_providedFromInfo, "language_label", "language_label", HoverDirection.TopRight);
    }

    private void Update()
    {
        // 행복도, 생산 시간 등 정보 갱신
        if (_currentTile != null)
        {
            if (_currentTile.Structure is ConsumerStructure)
            {
                _gaugeSlider.value = (_currentTile.Structure as ConsumerStructure).CurrentHappiness / _currentTile.Structure.StructureData.MaxHappiness;
                _gaugeText.text = (int)(_currentTile.Structure as ConsumerStructure).CurrentHappiness + " / " + (int)_currentTile.Structure.StructureData.MaxHappiness;
            }
            else if (_currentTile.Structure is ActiveProducerStructure)
            {
                if (_currentTile.Structure.StructureData.StructureType == StructureType.TownHall)
                {
                    bool costCheck = GameManager.Instance.CurrentWoods >= RESEARCH_COST_WOODS && GameManager.Instance.CurrentStones >= RESEARCH_COST_STONES;
                    _researchButton.interactable = costCheck && GameManager.Instance.IsResearchable;
                }

                _gaugeSlider.value = (_currentTile.Structure as ActiveProducerStructure).Elapsed / _currentTile.Structure.StructureData.TimeToProduce;
                _gaugeText.text = (int)(_currentTile.Structure as ActiveProducerStructure).Elapsed + " / " + (int)_currentTile.Structure.StructureData.TimeToProduce;
            }
        }
    }

    public void Show(Vector2Int tile)
    {
        UpdateTileInfo(tile);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 타일 정보를 갱신한다.
    /// </summary>
    /// <param name="tile">타일</param>
    public void UpdateTileInfo(Vector2Int tile)
    {
        _currentTile = MapManager.Instance.Tiles[tile.x, tile.y];

        foreach (Transform child in _providesToContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _providedFromContent.transform)
        {
            Destroy(child.gameObject);
        }

        if (_currentTile.Structure != null)
        {
            ProcesssStructureTile();
        }
        else
        {
            ProcessEmptyTile();
        }

        // 이웃 타일에 대한 정보 갱신
        foreach (Structure provider in _currentTile.Providers)
        {
            if (provider == _currentTile.Structure) continue;

            GameObject structureButton = Instantiate(_structureButtonPrefab);
            structureButton.transform.SetParent(_providedFromContent.transform, false);

            structureButton.GetComponentInChildren<LocalizedText>().ChangeKey(provider.StructureData.StructureNameKey);
            structureButton.GetComponentInChildren<LocalizedText>().UpdateTextLanguage();

            structureButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateTileInfo(provider.Tile.Coordinate);
            });
        }
    }

    /// <summary>
    /// 타일에 건물이 있는 경우를 처리한다.
    /// </summary>
    private void ProcesssStructureTile()
    {
        _titleText.ChangeKey(_currentTile.Structure.StructureData.StructureNameKey);
        _titleText.UpdateTextLanguage();

        // !TEST: 건물 상태 확인용
        if (_currentTile.Structure.IsEnabled)
        {
            _structureImage.color = Color.green;
        }
        else
        {
            _structureImage.color = Color.red;
        }

        // 소비형 건물인 경우
        if (_currentTile.Structure is ConsumerStructure)
        {
            _happinessInfo.gameObject.SetActive(true);
            _timeToProduceInfo.gameObject.SetActive(false);
            _researchInfo.gameObject.SetActive(false);

            _researchButton.gameObject.SetActive(false);

            _gaugeSlider.gameObject.SetActive(true);
            _gaugeText.gameObject.SetActive(true);
        }
        // 생산형 건물인 경우
        else if (_currentTile.Structure is ActiveProducerStructure)
        {
            // 시청인 경우
            if (_currentTile.Structure.StructureData.StructureType == StructureType.TownHall)
            {
                _happinessInfo.gameObject.SetActive(false);
                _timeToProduceInfo.gameObject.SetActive(false);
                _researchInfo.gameObject.SetActive(true);
                _fortressInfo.gameObject.SetActive(false);

                _researchButton.gameObject.SetActive(true);
            }
            // 요새인 경우
            else if (_currentTile.Structure.StructureData.StructureType == StructureType.Fortress)
            {
                _happinessInfo.gameObject.SetActive(false);
                _timeToProduceInfo.gameObject.SetActive(false);
                _researchInfo.gameObject.SetActive(false);
                _fortressInfo.gameObject.SetActive(true);

                _researchButton.gameObject.SetActive(false);
            }
            // 그 외 건물
            else
            {
                _happinessInfo.gameObject.SetActive(false);
                _timeToProduceInfo.gameObject.SetActive(true);
                _researchInfo.gameObject.SetActive(false);
                _fortressInfo.gameObject.SetActive(false);

                _researchButton.gameObject.SetActive(false);
            }

            _gaugeSlider.gameObject.SetActive(true);
            _gaugeText.gameObject.SetActive(true);
        }
        else
        {
            _happinessInfo.gameObject.SetActive(false);
            _timeToProduceInfo.gameObject.SetActive(false);
            _researchInfo.gameObject.SetActive(false);

            _researchButton.gameObject.SetActive(false);

            _gaugeSlider.gameObject.SetActive(false);
            _gaugeText.gameObject.SetActive(false);
        }

        _destroyButton.gameObject.SetActive(true);

        // 자원 정보 처리
        Resource needs = _currentTile.Structure.StructureData.Needs;
        Resource provided = _currentTile.Resource;

        _populationCount.text = provided.population + " / " + needs.population;
        _fishCount.text = provided.fish + " / " + needs.fish;
        _foodCount.text = provided.food + " / " + needs.food;
        _cottonCount.text = provided.cotton + " / " + needs.cotton;
        _clotheCount.text = provided.clothe + " / " + needs.clothe;
        _rangeBonus.text = provided.rangeBonus.ToString();

        // 이웃 타일에 대한 정보 갱신
        Tile[] neighbors = _currentTile.GetNeighbors(_currentTile.Structure.GetEffectiveRadius());

        foreach (Tile neighbor in neighbors)
        {
            if (neighbor.Structure == null) continue;
            if (neighbor == _currentTile) continue;
            if (!Resource.IsNeeded(neighbor.Structure.StructureData.Needs, _currentTile.Structure.GetEffectiveProduces())) continue;

            GameObject structureButton = Instantiate(_structureButtonPrefab);
            structureButton.transform.SetParent(_providesToContent.transform, false);

            structureButton.GetComponentInChildren<LocalizedText>().ChangeKey(neighbor.Structure.StructureData.StructureNameKey);
            structureButton.GetComponentInChildren<LocalizedText>().UpdateTextLanguage();

            structureButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateTileInfo(neighbor.Coordinate);
            });
        }

        MapRenderer.Instance.ShowRangeHighlight(_currentTile.Coordinate, _currentTile.Structure.GetEffectiveRadius());
    }

    /// <summary>
    /// 타일에 건물이 없는 경우를 처리한다.
    /// </summary>
    private void ProcessEmptyTile()
    {
        // 데크인 경우
        if (_currentTile.IsDecked)
        {
            _destroyButton.gameObject.SetActive(true);
            _titleText.ChangeKey(StructureManager.Instance.GetStructureData(StructureType.Deck).StructureNameKey);
        }
        // 그 외 경우
        else
        {
            _destroyButton.gameObject.SetActive(false);

            if (_currentTile.NaturalResource == NaturalResourceType.Woods)
            {
                _titleText.ChangeKey("woods_tile_title");
            }
            else if (_currentTile.NaturalResource == NaturalResourceType.Stone)
            {
                _titleText.ChangeKey("stone_tile_title");
            }
            else
            {
                _titleText.ChangeKey("empty_tile_title");
            }
        }

        _titleText.UpdateTextLanguage();

        _happinessInfo.gameObject.SetActive(false);
        _timeToProduceInfo.gameObject.SetActive(false);
        _researchInfo.gameObject.SetActive(false);

        _researchButton.gameObject.SetActive(false);

        _gaugeSlider.gameObject.SetActive(false);
        _gaugeText.gameObject.SetActive(false);

        Resource provided = _currentTile.Resource;

        _populationCount.text = provided.population + " / 0";
        _fishCount.text = provided.fish + " / 0";
        _foodCount.text = provided.food + " / 0";
        _cottonCount.text = provided.cotton + " / 0";
        _clotheCount.text = provided.clothe + " / 0";
        _rangeBonus.text = provided.rangeBonus.ToString();

        MapRenderer.Instance.HideRangeHighlight();
    }

    public void Hide()
    {
        MapRenderer.Instance.HideRangeHighlight();
        gameObject.SetActive(false);
    }
}
