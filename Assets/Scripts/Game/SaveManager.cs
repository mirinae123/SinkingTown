using System.Linq;
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Globalization;

/// <summary>
/// 저장, 불러오기를 관리하는 클래스
/// </summary>
public class SaveManager : SingletonBehaviour<SaveManager>
{
    [SerializeField] GameObject _saveDataButtonPrefab;

    /// <summary>
    /// 현재 불러온 세이브 데이터
    /// </summary>
    public SaveData SaveData
    {
        get => _saveData;
    }
    private SaveData _saveData;

    private Canvas[] _canvases;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        _canvases = FindObjectsOfType<Canvas>();
        SceneManager.sceneLoaded += (scene, loadSceneMode) =>
        {
            _canvases = FindObjectsOfType<Canvas>();
        };
    }

    public void ClearSaveData()
    {
        _saveData = new SaveData();
    }

    /// <summary>
    /// 세이브 데이터를 게임에 사용하도록 불러옵니다.
    /// </summary>
    /// <param name="saveData">불러올 데이터</param>
    public void LoadGame(SaveData saveData)
    {
        _saveData = saveData;
        RandomUtility.Seed = _saveData.Seed;
    }

    /// <summary>
    /// 세이브 데이터를 파일로 저장합니다.
    /// </summary>
    /// <param name="filePath">저장 위치</param>
    public void SaveGame(string filePath = "")
    {
        // 저장 중 알림 표시
        UIManager.Instance.ShowPanel(PanelType.Notification, "Try", "Try", false);

        StartCoroutine(CoSaveGame(filePath));
    }

    private IEnumerator CoSaveGame(string filePath)
    {
        // UI 잠시 비활성화
        foreach (Canvas canvas in _canvases)
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

        yield return new WaitForEndOfFrame();

        try
        {
            // 현재 화면 캡처
            float width = Screen.width;
            float height = Screen.height;

            float centerX = Screen.width / 2.0f;
            float centerY = Screen.height / 2.0f;

            float k = Mathf.Min(width / 2.0f , height / 1.6f);

            width = k * 2.0f;
            height = k * 1.6f;

            Texture2D screenshotTexture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(centerX - width / 2.0f, centerY - height / 2.0f, width, (int)height);
            
            screenshotTexture.ReadPixels(rect, 0, 0);
            screenshotTexture.Apply();

            _saveData.Thumbnail = ImageConversion.EncodeToPNG(screenshotTexture);

            // UI 다시 활성화
            foreach (Canvas canvas in _canvases)
            {
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }

            GameManager.Instance.PopulateSaveData(_saveData);
            MapManager.Instance.PopulateSaveData(_saveData);
            PirateManager.Instance.PopulateSaveData(_saveData);
            CameraManager.Instance.PopulateSaveData(_saveData);

            string dataToSave = JsonUtility.ToJson(_saveData);

            if (filePath == "")
            {
                filePath = Application.persistentDataPath + "\\" + _saveData.Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            File.WriteAllText(filePath, dataToSave);
        }
        catch (Exception)
        {
            // 저장 실패 시 알림 표시
            UIManager.Instance.HidePanel();
            UIManager.Instance.ShowPanel(PanelType.Notification, "Fail", "Fail", true);

            yield break;
        }

        // 저장 성공 시 알림 표시
        UIManager.Instance.HidePanel();
        UIManager.Instance.ShowPanel(PanelType.Notification, "Done", "Done", true);
    }

    /// <summary>
    /// UI에 쓸 세이브 데이터 버튼을 생성합니다.
    /// </summary>
    /// <param name="parent">버튼의 부모 오브젝트</param>
    /// <param name="isLoadMenu">불러오기 메뉴 여부</param>
    /// <returns>생성된 버튼 수</returns>
    public int PopulateSaveDataButtons(Transform parent, bool isLoadMenu)
    {
        string[] files = new string[] { };
        int saveDataButtonCount = 0;

        try
        {
            files = Directory.GetFiles(Application.persistentDataPath);
            files = files.OrderByDescending((x) => { return new FileInfo(x).LastWriteTime; }).ToArray();
        }
        catch (Exception) { }

        foreach (string file in files)
        {
            GameObject saveDataButtonObject = null;

            try
            {
                saveDataButtonObject = Instantiate(_saveDataButtonPrefab, parent, false);
                saveDataButtonObject.GetComponent<SaveDataButton>().UpdateButtonContent(file, isLoadMenu);

                saveDataButtonCount++;
            }
            catch (Exception)
            {
                if (saveDataButtonObject != null)
                {
                    Destroy(saveDataButtonObject);
                }
            }
        }

        return saveDataButtonCount;
    }
}
