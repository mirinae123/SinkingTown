using System.Linq;
using System;
using UnityEngine;
using System.IO;

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

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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
    /// <returns>저장 성공 여부</returns>
    public bool SaveGame(string filePath = "")
    {
        try
        {
            _saveData.Thumbnail = ImageConversion.EncodeToPNG(ScreenCapture.CaptureScreenshotAsTexture());

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
            return false;
        }

        return true;
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
