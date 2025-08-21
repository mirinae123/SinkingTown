using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로딩 메뉴 클래스
/// </summary>
public class LoadingUI : BaseUI
{
    public AnimatorStateInfo AnimatorStateInfo
    {
        get => _animator.GetCurrentAnimatorStateInfo(0);
    }

    private Animator _animator;

    private void Start()
    {
        // 한 씬에 하나의 로딩 UI만 있도록 강제
        if (UIManager.Instance.Panels.ContainsKey(PanelType.Loading))
        {
            Destroy(gameObject);
            return;
        }

        UIManager.Instance.RegisterPanel(PanelType.Loading, this);

        _animator = GetComponent<Animator>();

        DontDestroyOnLoad(gameObject);
    }

    public override void Show(params object[] values)
    {
        _animator.SetBool("isLoading", true);
    }

    public override void Hide()
    {
        _animator.SetBool("isLoading", false);
    }
}
