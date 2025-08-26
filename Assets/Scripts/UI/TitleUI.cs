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

    private void Start()
    {
        InputHandler.Instance.OnEscapeInput += OnEscapeInput;

        _newGameButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.NewGame);
        });

        _loadButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Load);
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
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnEscapeInput -= OnEscapeInput;
        }
    }

    private void OnEscapeInput()
    {
        if (UIManager.Instance.CurrentPanelType != PanelType.Loading)
        {
            UIManager.Instance.HidePanel();
        }
    }
}
