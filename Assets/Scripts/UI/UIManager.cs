using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.EventSystems;

/// <summary>
/// UI 상태
/// </summary>
public enum PanelType { None, Main, Build, Tile, Option, Confirm, End, Hover }

/// <summary>
/// UI를 관리하는 클래스
/// </summary>
public class UIManager : SingletonBehaviour<UIManager>
{
    /// <summary>
    /// 현재 UI 상태
    /// </summary>
    public PanelType CurrentPanelType
    {
        get
        {
            if (_panelStack.Count > 0)
            {
                return _panelStack.Peek();
            }
            else
            {
                return PanelType.None;
            }
        }
    }

    public IReadOnlyDictionary<PanelType, BaseUI> Panels
    {
        get => _panels;
    }
    private Dictionary<PanelType, BaseUI> _panels = new Dictionary<PanelType, BaseUI>();


    private Stack<PanelType> _panelStack = new Stack<PanelType>();

    public void ShowPanel(PanelType panelType, params object[] values)
    {
        switch (panelType)
        {
            case PanelType.Main:
            case PanelType.Build:
            case PanelType.Tile:
            case PanelType.End:
                HideAllPanels();
                break;
            default:
                if (_panelStack.Count > 0)
                {
                    _panels[_panelStack.Peek()].Hide();
                }
                break;
        }

        switch (panelType)
        {
            case PanelType.Main:
            case PanelType.Option:
            case PanelType.Confirm:
            case PanelType.End:
                GameManager.Instance.ChangeGameState(GameState.Menu);
                HideHoverPanel();
                break;
        }

        _panels[panelType].Show(values);
        _panelStack.Push(panelType);
    }

    public void HidePanel()
    {
        if (_panelStack.Count == 0)
        {
            return;
        }

        _panels[_panelStack.Pop()].Hide();

        if (_panelStack.Count > 0)
        {
            _panels[_panelStack.Peek()].Show();

            switch (_panelStack.Peek())
            {
                case PanelType.Main:
                case PanelType.Option:
                case PanelType.Confirm:
                case PanelType.End:
                    break;
                default:
                    if (GameManager.Instance.GameState != GameState.Build)
                    {
                        GameManager.Instance.ChangeGameState(GameState.None);
                    }
                    break;
            }
        }
        else
        {
            if (GameManager.Instance.GameState != GameState.Build)
            {
                GameManager.Instance.ChangeGameState(GameState.None);
            }
        }
    }

    public void HideAllPanels()
    {
        while (_panelStack.Count > 0)
        {
            _panels[_panelStack.Pop()].Hide();
        }
    }

    public void ShowHoverPanel(string caption, string description, HoverDirection hoverDirection)
    {
        _panels[PanelType.Hover].Show(caption, description, hoverDirection);
    }

    public void HideHoverPanel()
    {
        _panels[PanelType.Hover].Hide();
    }

    public void RegisterPanel(PanelType panelType, BaseUI panel)
    {
        _panels[panelType] = panel;
    }

    /// <summary>
    /// 마우스 호버 시, 호버 메뉴가 나타나는 이벤트를 추가한다.
    /// </summary>
    public void AddHoverEvent(EventTrigger eventTrigger, string caption, string description, HoverDirection hoverDirection)
    {
        EventTrigger.Entry entryEvent = new EventTrigger.Entry();
        entryEvent.eventID = EventTriggerType.PointerEnter;
        entryEvent.callback.AddListener((data) => { ShowHoverPanel(caption, description, hoverDirection); });
        eventTrigger.triggers.Add(entryEvent);

        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { HideHoverPanel(); });
        eventTrigger.triggers.Add(exitEvent);
    }
}
