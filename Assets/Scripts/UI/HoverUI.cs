using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 호버 메뉴 방향
/// </summary>
public enum HoverDirection { TopLeft, TopRight, BottomLeft, BottomRight }

/// <summary>
/// 호버 메뉴 클래스
/// </summary>
public class HoverUI : BaseUI
{
    [SerializeField] private RectTransform _mainCanvasTransform;
    [SerializeField] private RectTransform _hoverTransform;

    [SerializeField] private TMP_Text _caption;
    [SerializeField] private TMP_Text _description;

    [SerializeField] private LocalizedText _captionLocalizer;
    [SerializeField] private LocalizedText _descriptionLocalizer;

    [SerializeField] private GameObject _buildCostArea;
    [SerializeField] private TMP_Text _buildWoodCost;
    [SerializeField] private TMP_Text _buildStoneCost;

    [SerializeField] private Transform _buildProduceArea;
    [SerializeField] private Transform _buildNeedArea;

    [SerializeField] private GameObject _hoverResourceTextPrefab;

    private RectTransform _rectTransform;

    private HoverDirection _hoverDirection;
    private UnityAction _onUpdate;

    private List<GameObject> _spawnedObjects = new List<GameObject>();

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Hover, this);

        _rectTransform = GetComponent<RectTransform>();

        Hide();
    }

    private void Update()
    {
        _onUpdate?.Invoke();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvasTransform, Input.mousePosition, null, out Vector2 newPosition);

        newPosition.x += _mainCanvasTransform.sizeDelta.x / 2;
        newPosition.y -= _mainCanvasTransform.sizeDelta.y / 2;

        switch (_hoverDirection)
        {
            case HoverDirection.TopLeft:
                newPosition.x += 10;
                newPosition.y -= 10;
                break;
            case HoverDirection.TopRight:
                newPosition.x -= _hoverTransform.sizeDelta.x + 10;
                newPosition.y -= 10;
                break;
            case HoverDirection.BottomLeft:
                newPosition.x += 10;
                newPosition.y += _hoverTransform.sizeDelta.y + 10;
                break;
            default:
                newPosition.x -= _hoverTransform.sizeDelta.x + 10;
                newPosition.y += _hoverTransform.sizeDelta.y + 10;
                break;
        }

        _hoverTransform.anchoredPosition = newPosition;
    }

    public override void Show(params object[] values)
    {
        gameObject.SetActive(true);

        UpdateHoverText((KeyWrapper)values[0], (KeyWrapper)values[1]);
        _hoverDirection = (HoverDirection)values[2];
    }

    public override void Hide()
    {
        _onUpdate = null;

        foreach (GameObject spawnedObject in _spawnedObjects)
        {
            Destroy(spawnedObject);
        }

        _spawnedObjects.Clear();

        _buildCostArea.SetActive(false);
        _buildProduceArea.gameObject.SetActive(false);
        _buildNeedArea.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 호버 메뉴의 내용을 갱신한다.
    /// </summary>
    /// <param name="caption">제목</param>
    /// <param name="description">설명</param>
    public void UpdateHoverText(KeyWrapper caption, KeyWrapper description)
    {
        _captionLocalizer.ChangeKey(caption.key, caption.parameters);
        _descriptionLocalizer.ChangeKey(description.key, description.parameters);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    /// <summary>
    /// 매 Update마다 호출될 함수를 등록한다.
    /// </summary>
    /// <param name="onUpdate"></param>
    public void SetOnUpdate(UnityAction onUpdate)
    {
        _onUpdate = null;
        _onUpdate += onUpdate;
    }

    /// <summary>
    /// 호버 메뉴가 나타낼 건물 정보를 등록한다.
    /// </summary>
    /// <param name="structureType">건물 유형</param>
    public void SetStructureData(StructureType structureType)
    {
        StructureData data = StructureManager.Instance.GetStructureData(structureType);
        Resource empty = new Resource();

        _buildCostArea.SetActive(true);
        _buildWoodCost.text = data.WoodCost.ToString();
        _buildStoneCost.text = data.StoneCost.ToString();

        if (data.Produces > empty || data.Radius > 0 || StructureManager.Instance.IsActiveProducer(structureType))
        {
            _buildProduceArea.gameObject.SetActive(true);
        }

        if (data.Radius > 0) PopulateResourceText(_buildProduceArea, 7, "range_label", "build_hover_resource_count", data.Radius);
        if (data.Produces.population > 0) PopulateResourceText(_buildProduceArea, 0, "population_label", "build_hover_resource_count", data.Produces.population);
        if (data.Produces.fish > 0) PopulateResourceText(_buildProduceArea, 1, "fish_label", "build_hover_resource_count", data.Produces.fish);
        if (structureType == StructureType.Restaurant) PopulateResourceText(_buildProduceArea, 2, "food_label", "build_hover_resource_count", "-");
        if (data.WoodProduce > 0) PopulateResourceText(_buildProduceArea, 3, "wood_label", "build_hover_produce_per_second", data.TimeToProduce, data.WoodProduce);
        if (data.StoneProduce > 0) PopulateResourceText(_buildProduceArea, 4, "stone_label", "build_hover_produce_per_second", data.TimeToProduce, data.StoneProduce);
        if (data.Produces.cotton > 0) PopulateResourceText(_buildProduceArea, 5, "cotton_label", "build_hover_resource_count", data.Produces.cotton);
        if (structureType == StructureType.TextileMill) PopulateResourceText(_buildProduceArea, 6, "clothe_label", "build_hover_resource_count", "-");
        if (data.Produces.rangeBonus) PopulateResourceText(_buildProduceArea, 24, "range_bonus_label", "build_hover_resource_count", data.Produces.rangeBonus ? 1 : 0);
        if (structureType == StructureType.TownHall) PopulateResourceText(_buildProduceArea, 16, "research_point_label", "build_hover_produce_per_second", data.TimeToProduce, data.ResearchPointProduce);
        if (structureType == StructureType.Fortress) PopulateResourceText(_buildProduceArea, 24, "attack_pirate_label", "build_hover_produce_per_second", data.TimeToProduce, 1);

        if (data.Needs > empty)
        {
            _buildNeedArea.gameObject.SetActive(true);
        }

        if (data.Needs.population > 0) PopulateResourceText(_buildNeedArea, 0, "population_label", "build_hover_resource_count", data.Needs.population);
        if (data.Needs.fish > 0) PopulateResourceText(_buildNeedArea, 1, "fish_label", "build_hover_resource_count", data.Needs.fish);
        if (data.Needs.food > 0) PopulateResourceText(_buildNeedArea, 2, "food_label", "build_hover_resource_count", data.Needs.food);
        if (data.Needs.cotton > 0) PopulateResourceText(_buildNeedArea, 5, "cotton_label", "build_hover_resource_count", data.Needs.cotton);
        if (data.Needs.clothe > 0) PopulateResourceText(_buildNeedArea, 6, "clothe_label", "build_hover_resource_count", data.Needs.clothe);
    }

    private void PopulateResourceText(Transform parent, int icon, string labelKey, string countKey, params object[] values)
    {
        GameObject spawnedObject = Instantiate(_hoverResourceTextPrefab, parent, false);
        _spawnedObjects.Add(spawnedObject);

        HoverResourceText resourceText = spawnedObject.GetComponent<HoverResourceText>();
        resourceText.Icon = icon;
        resourceText.Label.ChangeKey(labelKey);
        resourceText.Count.ChangeKey(countKey, values);
    }
}
