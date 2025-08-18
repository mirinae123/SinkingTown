using UnityEngine;

/// <summary>
/// Singleton 패턴을 구현하는 클래스
/// </summary>
/// <typeparam name="T">클래스</typeparam>
public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T _instance;
    public static T Instance
    {
        get => _instance;
    }

    private bool _isMainInstance;

    private void Awake()
    {
        if (_instance != null)
        {
            _isMainInstance = false;
            Destroy(gameObject);
        }
        else
        {
            _isMainInstance = true;
            _instance = (T)this;
        }
    }

    private void OnDestroy()
    {
        if (_isMainInstance)
        {
            _instance = null;
        }
    }
}
