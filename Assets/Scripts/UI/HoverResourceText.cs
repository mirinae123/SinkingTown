using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 호버 메뉴에 들어갈 자원 목록 클래스
/// </summary>
public class HoverResourceText : MonoBehaviour
{
    public int Icon
    {
        set => _iconImage.sprite = _iconSprites[value];
    }

    [SerializeField] public LocalizedText Label;
    [SerializeField] public LocalizedText Count;

    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite[] _iconSprites;
}
