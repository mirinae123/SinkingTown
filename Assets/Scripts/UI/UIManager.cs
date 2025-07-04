using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// UI 상태
/// </summary>
public enum UIState { None, Build, Tile, MainMenu, Option, Confirm }

/// <summary>
/// UI를 관리하는 클래스
/// </summary>
public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField] private MainMenuUI _mainMenu;
    [SerializeField] private OptionMenuUI _optionMenu;
    [SerializeField] private HoverMenuUI _hoverMenu;     
    [SerializeField] private BuildMenuUI _buildMenu;
    [SerializeField] private ConfirmMenuUI _confirmMenu;

    [SerializeField] private GameObject _gameInfo;
    [SerializeField] private TileInfoUI _tileInfo;

    /// <summary>
    /// 현재 UI 상태
    /// </summary>
    public UIState CurrentUIState
    {
        get => _currentUIState;
        set => _currentUIState = value;
    }
    private UIState _currentUIState = UIState.None;

    private UIState _previousUIState = UIState.None;

    /// <summary>
    /// 메인 메뉴를 표시한다.
    /// </summary>
    public void ShowMainMenu()
    {
        _currentUIState = UIState.MainMenu;

        GameManager.Instance.ChangeGameState(GameState.Menu);

        _tileInfo.Hide();
        _buildMenu.Hide();
        _hoverMenu.Hide();

        _mainMenu.Show();
    }

    /// <summary>
    /// 메인 메뉴를 숨긴다.
    /// </summary>
    public void HideMainMenu()
    {
        _currentUIState = UIState.None;

        GameManager.Instance.ChangeGameState(GameState.None);

        _mainMenu.Hide();
    }

    /// <summary>
    /// 설정 메뉴를 표시한다.
    /// </summary>
    public void ShowOptionMenu()
    {
        _currentUIState = UIState.Option;

        _mainMenu.Hide();
        _optionMenu.Show();
    }

    /// <summary>
    /// 설정 메뉴를 숨긴다.
    /// </summary>
    public void HideOptionMenu()
    {
        _currentUIState = UIState.MainMenu;

        _mainMenu.Show();
        _optionMenu.Hide();
    }

    /// <summary>
    /// 호버 메뉴를 표시한다.
    /// </summary>
    /// <param name="caption">제목</param>
    /// <param name="text">내용</param>
    /// <param name="hoverDirection">방향</param>
    public void ShowHoverMenu(string caption, string text, HoverDirection hoverDirection)
    {
        _hoverMenu.Show(caption, text, hoverDirection);
    }

    /// <summary>
    /// 호버 메뉴를 숨긴다.
    /// </summary>
    public void HideHoverMenu()
    {
        _hoverMenu.Hide();
    }

    /// <summary>
    /// 건설 메뉴를 표시한다.
    /// </summary>
    public void ShowBuildMenu()
    {
        _currentUIState = UIState.Build;

        _tileInfo.Hide();
        _buildMenu.Show();
    }

    /// <summary>
    /// 건설 메뉴를 숨긴다.
    /// </summary>
    public void HideBuildMenu()
    {
        _currentUIState = UIState.None;

        _buildMenu.Hide();
    }

    /// <summary>
    /// 확인 메뉴를 표시한다.
    /// </summary>
    /// <param name="caption"></param>
    /// <param name="description"></param>
    /// <param name="onConfirm"></param>
    /// <param name="onCancel"></param>
    public void ShowConfirmMenu(string caption, string description, UnityAction onConfirm, UnityAction onCancel)
    {
        _previousUIState = _currentUIState;
        _currentUIState = UIState.Confirm;

        GameManager.Instance.ChangeGameState(GameState.Menu);

        _confirmMenu.Show(caption, description, onConfirm, onCancel);
    }

    /// <summary>
    /// 확인 메뉴를 숨긴다.
    /// </summary>
    public void HideConfirmMenu(bool isConfirmed)
    {
        _currentUIState = _previousUIState;

        if (_currentUIState == UIState.Tile)
        {
            GameManager.Instance.ChangeGameState(GameState.None);
        }

        _confirmMenu.Hide(isConfirmed);
    }

    /// <summary>
    /// 타일 정보를 표시한다.
    /// </summary>
    public void ShowTileInfo(Vector2Int tile)
    {
        _currentUIState = UIState.Tile;

        _buildMenu.Hide();
        _tileInfo.Show(tile);
    }

    /// <summary>
    /// 타일 정보를 갱신한다.
    /// </summary>
    public void UpdateTileInfo(Vector2Int tile)
    {
        if (_tileInfo.enabled && tile == _tileInfo.CurrentTile?.Coordinate)
        {
            _tileInfo.UpdateTileInfo(tile);
        }
    }

    /// <summary>
    /// 타일 정보를 숨긴다.
    /// </summary>
    public void HideTileInfo()
    {
        _currentUIState = UIState.None;

        _hoverMenu.Hide();
        _tileInfo.Hide();
    }

    public void ProcessEscapeInput()
    {
        switch(_currentUIState)
        {
            case UIState.Build:
                HideBuildMenu();
                break;
            case UIState.Tile:
                HideTileInfo();
                break;
            case UIState.MainMenu:
                HideMainMenu();
                break;
            case UIState.Option:
                HideOptionMenu();
                break;
            case UIState.Confirm:
                HideConfirmMenu(false);
                break;
            default:
                ShowMainMenu();
                break;
        }
    }

    /// <summary>
    /// 마우스 호버 시, 호버 메뉴가 나타나는 이벤트를 추가한다.
    /// </summary>
    public void AddHoverEvent(EventTrigger eventTrigger, string caption, string description, HoverDirection hoverDirection)
    {
        EventTrigger.Entry entryEvent = new EventTrigger.Entry();
        entryEvent.eventID = EventTriggerType.PointerEnter;
        entryEvent.callback.AddListener((data) => {
            UIManager.Instance.ShowHoverMenu(caption, description, hoverDirection);
        });
        eventTrigger.triggers.Add(entryEvent);

        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { UIManager.Instance.HideHoverMenu(); });
        eventTrigger.triggers.Add(exitEvent);
    }
}
