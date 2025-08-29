using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public bool HidePreviousPanel
    {
        get => _hidePreviousPanel;
    }
    [SerializeField] private bool _hidePreviousPanel;

    public bool HideAllPanels
    {
        get => _hideAllPanels;
    }
    [SerializeField] private bool _hideAllPanels;

    public bool HideHoverMenu
    {
        get => _hideHoverMenu;
    }
    [SerializeField] private bool _hideHoverMenu;

    public bool SetGameState
    {
        get => _setGameState;
    }
    [SerializeField] private bool _setGameState;

    public abstract void Show(params object[] values);
    public abstract void Hide();
}
