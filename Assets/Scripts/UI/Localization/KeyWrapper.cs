/// <summary>
/// 현지화 텍스트의 키와 매개변수를 담는 Wrapper
/// </summary>
public struct KeyWrapper
{
    public string key;
    public object[] parameters;

    public KeyWrapper(string key, params object[] parameters)
    {
        this.key = key;
        this.parameters = parameters;
    }
}