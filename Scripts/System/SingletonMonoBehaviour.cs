using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance = null;

    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<T>();

            if (instance == null)
            {
                var gobj = new GameObject(typeof(T).Name);
                instance = gobj.AddComponent<T>();
                DontDestroyOnLoad(gobj);
            }
        }

        return instance;
    }
}
