using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityAction OnLanguageUpdate
    {
        get => _onLanguageUpdate;
        set => _onLanguageUpdate = value;
    }
    private UnityAction _onLanguageUpdate;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        _textDatabase = new Dictionary<string, string>();
        ChangeLanguage("ko");
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

    public string GetText(string key, params object[] parameters)
    {
        if (_textDatabase.ContainsKey(key))
        {
            string[] texts = new string[parameters.Length];

            for (int i = 0; i < texts.Length; i++)
            {
                if (parameters[i] is KeyWrapper)
                {
                    KeyWrapper wrapper = (KeyWrapper)parameters[i];
                    texts[i] = GetText(wrapper.key, wrapper.parameters);
                }
                else
                {
                    texts[i] = parameters[i].ToString();
                }
            }

            try
            {
                return string.Format(_textDatabase[key], texts);
            }
            catch
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"Failed to format string \"{_textDatabase[key]}\" with [");

                foreach (string text in texts)
                {
                    sb.Append($"{texts} ");
                }

                sb.Append("]");
                Debug.LogError(sb.ToString());

                return $"String Format Error";
            }
        }
        else
        {
            return $"No string found for {key}";
        }
    }
}
