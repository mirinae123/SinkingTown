using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

class MainParser
{
    public string language;
    public string font;
    public TextParser[] texts;
}

[System.Serializable]
class TextParser
{
    public string key;
    public string text;
}

/// <summary>
/// 현지화를 관리하는 매니저 클래스
/// </summary>
public class LocalizationManager : SingletonBehaviour<LocalizationManager>
{
    /// <summary>
    /// 글꼴
    /// </summary>
    [SerializeField] private TMP_FontAsset[] fonts;

    /// <summary>
    /// 현재 언어
    /// </summary>
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set => _currentLanguage = value;
    }
    private string _currentLanguage;

    /// <summary>
    /// 현재 폰트
    /// </summary>
    public TMP_FontAsset CurrentFont
    {
        get => _currentFont;
    }
    private TMP_FontAsset _currentFont;

    /// <summary>
    /// Key, Value 데이터
    /// </summary>
    public Dictionary<string, string> TextDatabase
    {
        get => _textDatabase;
    }
    private Dictionary<string, string> _textDatabase;

    private OnLanguageUpdate _onLanguageUpdate;
    public delegate void OnLanguageUpdate();

    private void Start()
    {
        _textDatabase = new Dictionary<string, string>();
    }

    private void Update()
    {
        // !TEST
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_currentLanguage == "ko") ChangeLanguage("en");
            else ChangeLanguage("ko");
        }
    }

    /// <summary>
    /// 언어를 변경한다.
    /// </summary>
    /// <param name="newLanguage"></param>
    public void ChangeLanguage(string newLanguage)
    {
        if (newLanguage == _currentLanguage)
        {
            return;
        }

        string jsonText = Resources.Load<TextAsset>("Localization/" + newLanguage).text;

        MainParser mainParser = new MainParser();
        mainParser = JsonUtility.FromJson<MainParser>(jsonText);

        for (int i = 0; i < fonts.Length; i++)
        {
            if (fonts[i].name == mainParser.font)
            {
                _currentFont = fonts[i];
                break;
            }
        }

        _textDatabase.Clear();

        foreach(TextParser textParser in mainParser.texts)
        {
            _textDatabase.Add(textParser.key, textParser.text);
        }

        _currentLanguage = newLanguage;

        _onLanguageUpdate?.Invoke();
    }

    /// <summary>
    /// 언어 변경 이벤트에 함수를 등록한다.
    /// </summary>
    /// <param name="function">함수</param>
    public void AddToLanguageUpdateCallback(OnLanguageUpdate function)
    {
        _onLanguageUpdate += function;
    }

    /// <summary>
    /// 언어 변경 이벤트에서 함수를 등록 해제한다.
    /// </summary>
    /// <param name="function">함수</param>
    public void RemoveFromLanguageUpdateCallback(OnLanguageUpdate function)
    {
        _onLanguageUpdate -= function;
    }
}
