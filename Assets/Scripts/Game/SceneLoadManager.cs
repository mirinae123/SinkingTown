using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

/// <summary>
/// 현재 게임의 세션 정보를 저장하는 클래스
/// </summary>
public class SceneLoadManager : SingletonBehaviour<SceneLoadManager>
{
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
