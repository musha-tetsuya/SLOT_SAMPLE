using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シンプルダイアログ用ボタングループ
/// </summary>
public class SimpleDialogButtonGroup : MonoBehaviour
{
    /// <summary>
    /// ボタンプレハブ
    /// </summary>
    [SerializeField]
    private SimpleDialogButton buttonPrefab = null;

    /// <summary>
    /// ボタン
    /// </summary>
    public SimpleDialogButton[] buttons { get; private set; }

    /// <summary>
    /// ボタン追加
    /// </summary>
    public void AddButton(int size)
    {
        this.buttons = new SimpleDialogButton[size];

        for (int i = 0; i < size; i++)
        {
            this.buttons[i] = Instantiate(this.buttonPrefab, this.transform, false);
        }
    }
}
