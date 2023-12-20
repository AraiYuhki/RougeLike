using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T: MonoBehaviour
{
    protected static T instance = null;
    public static T Instance => instance;

    protected virtual void Awake()
    {
        if (instance == null) instance = GetComponent<T>();
        else if (instance != this) Destroy(gameObject);
    }
}
