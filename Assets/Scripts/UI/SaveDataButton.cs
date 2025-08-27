using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 세이브 데이터 버튼 클래스
/// </summary>
public class SaveDataButton : MonoBehaviour
{
    [SerializeField] Button _mainButton;
    [SerializeField] Button _deleteButton;

    [SerializeField] Image _thumbnail;

    [SerializeField] TMP_Text _saveName;
    [SerializeField] TMP_Text _lastSavedTime;
    [SerializeField] TMP_Text _playTime;

    private bool _isLoadButton;

    private SaveData _saveData;
    private string _filePath;

    private void Start()
    {
        _mainButton.onClick.AddListener(() =>
        {
            // 불러오기 버튼인 경우
            if (_isLoadButton)
            {
                UIManager.Instance.ShowPanel(PanelType.Confirm, "Load this?", "Load this?", (UnityAction)(() =>
                {
                    SaveManager.Instance.LoadGame(_saveData);
                    SceneLoadManager.Instance.LoadScene(1);
                }), null);
            }
            // 저장 버튼인 경우
            else
            {
                UIManager.Instance.ShowPanel(PanelType.Confirm, "Overwrite?", "Overwrite?", (UnityAction)(() =>
                {
                    SaveManager.Instance.SaveGame(_filePath);
                }), null);
            }
        });

        _deleteButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Confirm, "Delete this?", "Delete this?", (UnityAction)(() =>
            {
                // 삭제 시도 및 성공 여부에 따라 알림 표시
                try
                {
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                        UIManager.Instance.ShowPanel(PanelType.Notification, "Error", "Error");
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    UIManager.Instance.ShowPanel(PanelType.Notification, "Error", "Error");
                }
            }), null);
        });
    }

    /// <summary>
    /// 주어진 세이브 파일을 바탕으로 버튼에 들어갈 내용을 갱신합니다.
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <param name="isLoadButton">불러오기 버튼 여부</param>
    public void UpdateButtonContent(string filePath, bool isLoadButton)
    {
        _filePath = filePath;
        _saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(_filePath));

        _saveName.text = _saveData.Name;
        _lastSavedTime.text = new FileInfo(filePath).LastWriteTime.ToString("yyyy/MM/dd hh:mm");

        // 플레이 시간 계산
        float playSeconds = _saveData.PlayTime;

        int playHours = (int)(playSeconds / 3600.0f);
        playSeconds -= (float)playHours;

        int playMinutes = (int)(playSeconds / 60.0f);
        playSeconds -= (float)playMinutes;

        StringBuilder playTimeString = new StringBuilder();

        if (playHours > 0)
        {
            playTimeString.Append(playHours + " Hours ");
        }

        if (playMinutes > 0)
        {
            playTimeString.Append(playMinutes + " Minutes ");
        }

        playTimeString.Append((int)playSeconds + " Seconds");

        _playTime.text = playTimeString.ToString();

        // 썸네일 불러오기
        Texture2D thumbnail = new Texture2D(0, 0);
        ImageConversion.LoadImage(thumbnail, _saveData.Thumbnail);
        _thumbnail.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
        
        _isLoadButton = isLoadButton;
    }
}
