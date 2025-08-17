using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public abstract void Show(params object[] values);
    public abstract void Hide();
}
