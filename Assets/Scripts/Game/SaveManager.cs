using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 저장, 불러오기를 관리하는 클래스
/// </summary>
public class SaveManager : SingletonBehaviour<SaveManager>
{
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
}
