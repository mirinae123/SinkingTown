using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 건설 메뉴 클래스
/// </summary>
public class BuildMenuUI : MonoBehaviour
{
    [SerializeField] private Button _buildIcon;
    [SerializeField] private Button _quitIcon;

    [SerializeField] private GameObject _content;

    [SerializeField] private Animator _animator;

    private Button[] _buttons;

    private void Start()
    {
        _buildIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowBuildMenu();
        });

        _quitIcon.onClick.AddListener(() =>
        {
            UIManager.Instance.HideBuildMenu();
        });

        _buttons = _content.GetComponentsInChildren<Button>();
    }

    public void Show()
    {
        _animator.SetBool("IsOpen", true);
        GameManager.Instance.ChangeGameState(GameState.None);

        foreach (Button button in _buttons)
        {
            button.enabled = true;
        }
    }

    public void Hide()
    {
        _animator.SetBool("IsOpen", false);

        foreach (Button button in _buttons)
        {
            button.enabled = false;
        }
    }
}
