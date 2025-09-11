// JSON 파일에서 현지화 정보를 불러오기 위한 파서

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