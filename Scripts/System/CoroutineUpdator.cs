using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 託されたコルーチンを処理後、自動で消滅するオブジェクト
/// </summary>
public class CoroutineUpdator : MonoBehaviour
{
    /// <summary>
    /// 処理
    /// </summary>
    private object obj = null;

    /// <summary>
    /// コールバック
    /// </summary>
    private Action callback = null;

    /// <summary>
    /// 生成
    /// </summary>
    public static void Create(object obj, Action callback = null)
    {
        var gobj = new GameObject("CoroutineUpdator");
        var component = gobj.AddComponent<CoroutineUpdator>();
        component.obj = obj;
        component.callback = callback;
    }

    /// <summary>
    /// コルーチン処理
    /// </summary>
    private IEnumerator Start()
    {
        yield return obj;
        this.callback?.Invoke();
        Destroy(this.gameObject);
    }
}
