using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// UI 종류
/// </summary>
public enum PanelType { None, Main, Build, Tile, Option, Confirm, End, Hover }

/// <summary>
/// UI를 관리하는 클래스
/// </summary>
public class UIManager : SingletonBehaviour<UIManager>
{
    /// <summary>
    /// 현재 UI 종류
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

    /// <summary>
    /// 등록된 모든 UI 창
    /// </summary>
    public IReadOnlyDictionary<PanelType, BaseUI> Panels
    {
        get => _panels;
    }
    private Dictionary<PanelType, BaseUI> _panels = new Dictionary<PanelType, BaseUI>();

    private Stack<PanelType> _panelStack = new Stack<PanelType>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += (scene, sceneLoadMode) =>
        {
            HideAllPanels();
            HideHoverPanel();

            _panels.Clear();
        };
    }

    /// <summary>
    /// UI를 표시한다.
    /// </summary>
    /// <param name="panelType">UI 종류</param>
    /// <param name="values">추가 매개변수</param>
    public void ShowPanel(PanelType panelType, params object[] values)
    {
        // 기존에 있던 UI 숨기기
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

        // 게임 상태 변경 및 호버 메뉴 숨기기
        switch (panelType)
        {
            case PanelType.Main:
            case PanelType.Option:
            case PanelType.Confirm:
            case PanelType.End:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeGameState(GameState.Menu);
                }
                HideHoverPanel();
                break;
        }

        _panels[panelType].Show(values);
        _panelStack.Push(panelType);
    }

    /// <summary>
    /// 가장 위에 있는 UI를 숨긴다.
    /// </summary>
    public void HidePanel()
    {
        if (_panelStack.Count == 0)
        {
            return;
        }

        _panels[_panelStack.Pop()].Hide();

        // 기존에 있던 UI 복구 및 게임 상태 변경
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
                    if (GameManager.Instance != null && GameManager.Instance.GameState != GameState.Build)
                    {
                        GameManager.Instance.ChangeGameState(GameState.None);
                    }
                    break;
            }
        }
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.GameState != GameState.Build)
            {
                GameManager.Instance.ChangeGameState(GameState.None);
            }
        }
    }

    /// <summary>
    /// 모든 UI를 숨긴다.
    /// </summary>
    public void HideAllPanels()
    {
        while (_panelStack.Count > 0)
        {
            _panels[_panelStack.Pop()].Hide();
        }
    }

    /// <summary>
    /// 호버 메뉴를 표시한다.
    /// </summary>
    /// <param name="caption">제목</param>
    /// <param name="description">설명</param>
    /// <param name="hoverDirection">위치</param>
    public void ShowHoverPanel(string caption, string description, HoverDirection hoverDirection)
    {
        _panels[PanelType.Hover].Show(caption, description, hoverDirection);
    }

    /// <summary>
    /// 호버 메뉴를 숨긴다.
    /// </summary>
    public void HideHoverPanel()
    {
        if (_panels.ContainsKey(PanelType.Hover) && _panels[PanelType.Hover] != null)
        {
            _panels[PanelType.Hover].Hide();
        }
    }

    /// <summary>
    /// UI 창을 UI Manager에 등록한다.
    /// </summary>
    /// <param name="panelType">UI 종류</param>
    /// <param name="panel">등록한 창</param>
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
