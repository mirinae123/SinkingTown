using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// UI 종류
/// </summary>
public enum PanelType { None, Main, Build, Tile, Option, Confirm, End, Hover, NewGame, Loading, Load, Save, Notification }

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

            // 로딩 UI만 남기고 나머지는 정리
            _panels.TryGetValue(PanelType.Loading, out BaseUI loadingPanel);
            _panels.Clear();

            if (loadingPanel != null)
            {
                _panels[PanelType.Loading] = loadingPanel;
            }
        };
    }

    /// <summary>
    /// UI를 표시한다.
    /// </summary>
    /// <param name="panelType">UI 종류</param>
    /// <param name="values">추가 매개변수</param>
    public void ShowPanel(PanelType panelType, params object[] values)
    {
        BaseUI panelToShow = _panels[panelType];

        if (panelToShow.HideAllPanels)
        {
            HideAllPanels();
        }
        else if (panelToShow.HidePreviousPanel && _panelStack.Count > 0)
        {
            _panels[_panelStack.Peek()].Hide();
        }

        if (panelToShow.HideHoverMenu)
        {
            HideHoverPanel();
        }

        if (panelToShow.SetGameState && GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Menu);
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

        BaseUI panelToHide = _panels[_panelStack.Pop()];
        panelToHide.Hide();

        if (_panelStack.Count > 0 && panelToHide.HidePreviousPanel)
        {
            _panels[_panelStack.Peek()].Show();
        }

        bool inBuildState = GameManager.Instance == null || GameManager.Instance.GameState == GameState.Build;

        if (_panelStack.Count == 0 && !inBuildState)
        {
            GameManager.Instance.ChangeGameState(GameState.None);
        }
        else if (_panelStack.Count > 0 && !_panels[_panelStack.Peek()].SetGameState && !inBuildState)
        {
            GameManager.Instance.ChangeGameState(GameState.None);
        }
    }

    /// <summary>
    /// 모든 UI를 숨긴다.
    /// </summary>
    public void HideAllPanels()
    {
        if (_panelStack.Count > 0 && _panelStack.Peek() == PanelType.Loading)
        {
            return;
        }

        while (_panelStack.Count > 0)
        {
            PanelType panelToHide = _panelStack.Pop();

            if (_panels[panelToHide] != null)
            {
                _panels[panelToHide].Hide();
            }
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
