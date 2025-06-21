using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = (T)this;
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
