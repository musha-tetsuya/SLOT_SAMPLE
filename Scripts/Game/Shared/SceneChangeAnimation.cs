using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン遷移アニメーション
/// </summary>
public class SceneChangeAnimation : MonoBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator animator = null;

    /// <summary>
    /// inアニメ終了時コールバック
    /// </summary>
    public Action onFinishedIn = null;
    /// <summary>
    /// outアニメ終了時コールバック
    /// </summary>
    public Action onFinishedOut = null;

    /// <summary>
    /// inアニメ終了時
    /// </summary>
    private void OnFinishedIn()
    {
        this.onFinishedIn?.Invoke();
    }

    /// <summary>
    /// outアニメ終了時
    /// </summary>
    private void OnFinishedOut()
    {
        this.onFinishedOut?.Invoke();
    }

    /// <summary>
    /// loopからoutに遷移させる
    /// </summary>
    public void SetOut()
    {
        this.animator.SetBool("out", true);
    }
}
