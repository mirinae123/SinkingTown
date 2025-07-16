using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 건설 버튼 클래스
/// </summary>
public class BuildingButton : MonoBehaviour
{
    [SerializeField] private StructureType _structureType;

    [SerializeField] private Button _buildingButton;
    [SerializeField] private EventTrigger _buttonEvent;
    [SerializeField] private Image _buildingImage;
    [SerializeField] private LocalizedText _buildingText;

    private int _woodCost;
    private int _stoneCost;

    void Start()
    {
        UIManager.Instance.AddHoverEvent(_buttonEvent, "option_title", "language_label", HoverDirection.BottomLeft);

        _buildingImage.sprite = StructureManager.Instance.GetStructureData(_structureType)?.StructureImage;
        _buildingText.ChangeKey(StructureManager.Instance.GetStructureData(_structureType).StructureNameKey);

        _buildingButton.onClick.AddListener(() => {
            GameManager.Instance.ChangeGameState(GameState.Build, _structureType);

            UIManager.Instance.HideBuildMenu();
        });

        StructureData structureData = StructureManager.Instance.GetStructureData(_structureType);
        _woodCost = structureData.WoodCost;
        _stoneCost = structureData.StoneCost;
    }

    void Update()
    {
        // 건설 자원이 충분할 때만 활성화한다.
        if (_woodCost <= GameManager.Instance.CurrentWoods && _stoneCost <= GameManager.Instance.CurrentStones)
        {
            _buildingButton.interactable = true;
        }
        else
        {
            _buildingButton.interactable = false;
        }
    }
}
