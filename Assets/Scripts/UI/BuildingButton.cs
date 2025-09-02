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
        StructureData structureData = StructureManager.Instance.GetStructureData(_structureType);

        _buildingImage.sprite = structureData.StructureImage;
        _buildingText.ChangeKey(structureData.StructureNameKey);

        _buildingButton.onClick.AddListener(() => {
            GameManager.Instance.ChangeGameState(GameState.Build, _structureType);

            UIManager.Instance.HidePanel();
        });

        _woodCost = structureData.WoodCost;
        _stoneCost = structureData.StoneCost;

        UIManager.Instance.AddHoverEvent(_buttonEvent, new KeyWrapper(structureData.StructureNameKey), new KeyWrapper(structureData.StructureDescriptionKey), HoverDirection.BottomLeft, _structureType);
    }

    void Update()
    {
        // 건설 자원이 충분할 때만 활성화한다.
        if (_woodCost <= GameManager.Instance.CurrentWoods && _stoneCost <= GameManager.Instance.CurrentStones)
        {
            if (_structureType == StructureType.TownHall)
            {
                _buildingButton.interactable = !GameManager.Instance.HasTownHall;
            }
            else
            {
                _buildingButton.interactable = true;
            }
        }
        else
        {
            _buildingButton.interactable = false;
        }
    }
}
