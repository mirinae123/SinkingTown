using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 상태
/// </summary>
public enum UIState { None, Build, Tile, MainMenu, Option }

/// <summary>
/// UI를 관리하는 클래스
/// </summary>
public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField] private MainMenuUI _mainMenu;
    [SerializeField] private OptionMenuUI _optionMenu;
    [SerializeField] private HoverMenuUI _hoverMenu;     
    [SerializeField] private BuildMenuUI _buildMenu;

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

    /// <summary>
    /// 메인 메뉴를 표시한다.
    /// </summary>
    public void ShowMainMenu()
    {
        if (_currentUIState != UIState.MainMenu && _currentUIState != UIState.Option)
        {
            _currentUIState = UIState.MainMenu;
            GameManager.Instance.ChangeGameState(GameState.Menu);

            _tileInfo.Hide();
            _buildMenu.Hide();
            _mainMenu.Show();
        }
    }

    /// <summary>
    /// 메인 메뉴를 숨긴다.
    /// </summary>
    public void HideMainMenu()
    {
        if (_currentUIState == UIState.MainMenu)
        {
            _currentUIState = UIState.None;
            GameManager.Instance.ChangeGameState(GameState.None);

            _mainMenu.Hide();
        }
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
        if (_currentUIState != UIState.MainMenu && _currentUIState != UIState.Option)
        {
            _hoverMenu.Show(caption, text, hoverDirection);
        }
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
        if (_currentUIState != UIState.MainMenu && _currentUIState != UIState.Option)
        {
            _currentUIState = UIState.Build;

            _tileInfo.Hide();
            _buildMenu.Show();
        }
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
    /// 타일 정보를 표시한다.
    /// </summary>
    public void ShowTileInfo(Vector2Int tile)
    {
        if (_currentUIState != UIState.MainMenu && _currentUIState != UIState.Option)
        {
            _currentUIState = UIState.Tile;

            _buildMenu.Hide();
            _tileInfo.Show(tile);
        }
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
        _tileInfo.Hide();
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
