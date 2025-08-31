using TMPro;
using UnityEngine;

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

    private HoverDirection _hoverDirection;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Hover, this);
        gameObject.SetActive(false);
    }

    private void Update()
    {
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

        _captionLocalizer.ChangeKey((string)values[0]);
        _descriptionLocalizer.ChangeKey((string)values[1]);

        _hoverDirection = (HoverDirection)values[2];

        _description.ForceMeshUpdate();
        _hoverTransform.sizeDelta = new Vector2(_hoverTransform.sizeDelta.x, 115f + (_description.textInfo.lineCount - 1) * 25f);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }
}
