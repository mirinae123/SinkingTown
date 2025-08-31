using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 건설 메뉴 클래스
/// </summary>
public class BuildUI : BaseUI
{
    [SerializeField] private Button _buildIcon;
    [SerializeField] private ScrollRect _scrollRect;

    private float _buttonCooldown = 0.3f;
    private bool _isOpen = false;

    private BuildUIAnimator _animator;

    private void Start()
    {
        UIManager.Instance.RegisterPanel(PanelType.Build, this);

        _buildIcon.onClick.AddListener(() =>
        {
            if (_buttonCooldown < 0.3f)
            {
                return;
            }

            if (_isOpen)
            {
                UIManager.Instance.HidePanel();
            }
            else
            {
                UIManager.Instance.ShowPanel(PanelType.Build);
            }
        });

        _animator = GetComponent<BuildUIAnimator>();
    }

    private void Update()
    {
        if (_buttonCooldown < 0.3f)
        {
            _buttonCooldown += Time.deltaTime;

            if (_buttonCooldown >= 0.3f)
            {
                _buildIcon.interactable = true;
            }
        }
    }

    public override void Show(params object[] values)
    {
        _isOpen = true;
        _animator.Open();

        _buttonCooldown = 0.0f;
        _buildIcon.interactable = false;

        _scrollRect.horizontalNormalizedPosition = 0.0f;

        if (GameManager.Instance.GameState == GameState.Build)
        {
            GameManager.Instance.ChangeGameState(GameState.None);
            MapRenderer.Instance.HideRangeHighlight();
        }
    }

    public override void Hide()
    {
        _isOpen = false;
        _animator.Close();

        _buttonCooldown = 0.0f;
        _buildIcon.interactable = false;
    }

    public void ShowToScreen()
    {
        _animator.Show();
    }

    public void HideFromScreen()
    {
        Hide();
        _animator.Hide();

        if (GameManager.Instance.GameState == GameState.Build)
        {
            GameManager.Instance.ChangeGameState(GameState.None);
            MapRenderer.Instance.HideRangeHighlight();
        }
    }
}
