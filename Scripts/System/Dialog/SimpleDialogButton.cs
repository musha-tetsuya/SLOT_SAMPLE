using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シンプルダイアログ用ボタン
/// </summary>
public class SimpleDialogButton : MonoBehaviour
{
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    public Button button = null;
    /// <summary>
    /// イメージ
    /// </summary>
    [SerializeField]
    public Image image = null;
    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField]
    public Text text = null;
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    public Action onClick = null;

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke();
    }
}
