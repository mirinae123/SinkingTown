using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 클래스
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private Button _quitIcon;

    private void Start()
    {
        _optionButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowOptionMenu();
        });

        _resumeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.HideMainMenu();
        });

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HideMainMenu();
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
