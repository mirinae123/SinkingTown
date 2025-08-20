using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 타이틀 메뉴 클래스
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] Button _newGameButton;
    [SerializeField] Button _loadButton;
    [SerializeField] Button _recordButton;
    [SerializeField] Button _optionButton;
    [SerializeField] Button _quitButton;

    void Start()
    {
        _newGameButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.NewGame);
        });

        _loadButton.onClick.AddListener(() =>
        {
        });

        _recordButton.onClick.AddListener(() =>
        {
        });

        _optionButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Option);
        });

        _quitButton.onClick.AddListener(() =>
        {
        });
    }
}
