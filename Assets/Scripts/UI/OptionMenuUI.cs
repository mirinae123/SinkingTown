using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 메뉴 클래스
/// </summary>
public class OptionMenuUI : MonoBehaviour
{
    [SerializeField] private Button _quitIcon;

    private void Start()
    {
        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HideOptionMenu();
        });
    }

    public void Show()
    {
        transform.parent.gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
