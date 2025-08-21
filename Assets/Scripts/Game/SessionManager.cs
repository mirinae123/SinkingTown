using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

/// <summary>
/// 현재 게임의 세션 정보를 저장하는 클래스
/// </summary>
public class SessionManager : SingletonBehaviour<SessionManager>
{
    /// <summary>
    /// 맵 크기
    /// </summary>
    public int MapSize;

    /// <summary>
    /// 해수면 상승 간격
    /// </summary>
    public float OceanRisePeriod;

    /// <summary>
    /// 해적 스폰 간격
    /// </summary>
    public float PirateSpawnPeriod;

    /// <summary>
    /// 일시정지 가능 여부
    /// </summary>
    public bool CanPause;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 새로운 씬을 불러온다.
    /// </summary>
    /// <param name="sceneIndex">씬 인덱스</param>
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(CoLoadScene(sceneIndex));
    }

    private IEnumerator CoLoadScene(int sceneIndex)
    {
        UIManager.Instance.ShowPanel(PanelType.Loading);

        LoadingUI loadingUI = (LoadingUI)UIManager.Instance.Panels[PanelType.Loading];

        while (!loadingUI.AnimatorStateInfo.IsName("Loading"))
        {
            yield return null;
        }

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
        asyncOp.allowSceneActivation = false;

        while (!asyncOp.isDone)
        {
            yield return null;

            if (asyncOp.progress >= 0.9f)
            {
                asyncOp.allowSceneActivation = true;
            }
        }

        UIManager.Instance.HidePanel();
    }
}
