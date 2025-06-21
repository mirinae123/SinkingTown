using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 현지화 텍스트 클래스
/// </summary>
public class LocalizedText : MonoBehaviour
{
    /// <summary>
    /// Key 값
    /// </summary>
    [SerializeField] private string _key;

    private TMP_Text _text;

    void Start()
    {
        if (!_text)
        {
            _text = GetComponent<TMP_Text>();
            LocalizationManager.Instance.AddToLanguageUpdateCallback(UpdateTextLanguage);
            UpdateTextLanguage();
        }
    }

    /// <summary>
    /// 텍스트 언어를 갱신한다.
    /// </summary>
    public void UpdateTextLanguage()
    {
        if (LocalizationManager.Instance.TextDatabase == null)
        {
            return;
        }

        if (LocalizationManager.Instance.TextDatabase.ContainsKey(_key))
        {
            if (!_text)
            {
                _text = GetComponent<TMP_Text>();
                LocalizationManager.Instance.AddToLanguageUpdateCallback(UpdateTextLanguage);
            }

            _text.font = LocalizationManager.Instance.CurrentFont;
            _text.text = LocalizationManager.Instance.TextDatabase[_key];
        }
    }

    // Key 값을 변경한다.
    public void ChangeKey(string newKey)
    {
        if (_key != newKey)
        {
            _key = newKey;
        }
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance?.RemoveFromLanguageUpdateCallback(UpdateTextLanguage);
    }
}
