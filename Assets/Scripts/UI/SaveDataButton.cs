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
    [SerializeField] LocalizedText _lastSavedTime;
    [SerializeField] LocalizedText _playTime;

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
                UIManager.Instance.ShowPanel(PanelType.Confirm, new KeyWrapper("load_confirm_caption"), new KeyWrapper("load_confirm_description", _saveData.Name), (UnityAction)(() =>
                {
                    SaveManager.Instance.LoadGame(_saveData);
                    SceneLoadManager.Instance.LoadScene(1);
                }), null);
            }
            // 저장 버튼인 경우
            else
            {
                UIManager.Instance.ShowPanel(PanelType.Confirm, new KeyWrapper("overwrite_confirm_caption"), new KeyWrapper("overwrite_confirm_description", _saveData.Name), (UnityAction)(() =>
                {
                    SaveManager.Instance.SaveGame(_filePath);
                }), null);
            }
        });

        _deleteButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel(PanelType.Confirm, new KeyWrapper("delete_confirm_caption"), new KeyWrapper("delete_confirm_description", _saveData.Name), (UnityAction)(() =>
            {
                // 삭제 시도 및 성공 여부에 따라 알림 표시
                try
                {
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);

                        UIManager.Instance.ShowPanel(PanelType.Notification, new KeyWrapper("delete_complete_notification_caption"), new KeyWrapper("delete_complete_notification_description"));

                        if (UIManager.Instance.Panels.TryGetValue(PanelType.Load, out BaseUI loadUI))
                        {
                            ((LoadUI)loadUI).UpdateSaveDataList();
                        }

                        if (UIManager.Instance.Panels.TryGetValue(PanelType.Save, out BaseUI saveUI))
                        {
                            ((SaveUI)saveUI).UpdateSaveDataList();
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    UIManager.Instance.ShowPanel(PanelType.Notification, new KeyWrapper("delete_fail_notification_caption"), new KeyWrapper("delete_fail_notification_description"));
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

        DateTime lastSavedTime = new FileInfo(filePath).LastWriteTime;
        _lastSavedTime.ChangeKey("last_saved_time", lastSavedTime.ToString("yyyy"), lastSavedTime.ToString("MM"), lastSavedTime.ToString("dd"), lastSavedTime.ToString("hh"), lastSavedTime.ToString("mm"));

        // 플레이 시간 계산
        float playSeconds = _saveData.PlayTime;

        int playHours = (int)(playSeconds / 3600.0f);
        playSeconds -= (float)(playHours * 3600);

        int playMinutes = (int)(playSeconds / 60.0f);
        playSeconds -= (float)(playMinutes * 60);

        if (playHours > 0)
        {
            _playTime.ChangeKey("hours_minutes_seconds", playHours.ToString(), playMinutes.ToString(), ((int)playSeconds).ToString());
        }
        else if (playMinutes > 0)
        {
            _playTime.ChangeKey("minutes_seconds", playMinutes.ToString(), ((int)playSeconds).ToString());
        }
        else
        {
            _playTime.ChangeKey("seconds", ((int)playSeconds).ToString());
        }

        // 썸네일 불러오기
        Texture2D thumbnail = new Texture2D(0, 0);
        ImageConversion.LoadImage(thumbnail, _saveData.Thumbnail);
        _thumbnail.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
        
        _isLoadButton = isLoadButton;
    }
}
