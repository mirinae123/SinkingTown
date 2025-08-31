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
    private object[] _parameters;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageUpdate -= UpdateText;
        }
    }

    private void Initialize()
    {
        if (!_text)
        {
            _text = GetComponent<TMP_Text>();
            _parameters = new object[0];

            LocalizationManager.Instance.OnLanguageUpdate += UpdateText;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        _text.font = LocalizationManager.Instance.CurrentFont;
        _text.text = LocalizationManager.Instance.GetText(_key, _parameters);
    }

    // Key 값을 변경한다.
    public void ChangeKey(string newKey, params object[] parameters)
    {
        Initialize();

        _key = newKey;
        _parameters = parameters;

        UpdateText();
    }
}
