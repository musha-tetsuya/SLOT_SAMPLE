using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン基底
/// </summary>
public class SceneBase : MonoBehaviour, HeaderPanel.IEventListner
{
    /// <summary>
    /// ヘッダーを使うかどうか
    /// </summary>
    [SerializeField]
    public bool useHeader = false;
    /// <summary>
    /// シーンアトラス
    /// </summary>
    [SerializeField]
    public AtlasSpriteCache sceneAtlas = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected virtual void OnDestroy()
    {
        //TODO:ヘッダーの非表示処理とか
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected virtual void Awake()
    {
        //TODO:ヘッダーの表示処理とか
        //マルチタッチを許可しない
        Input.multiTouchEnabled = false;
        if (this.useHeader)
        {
            SharedScene.instance.ShowHeader();
            SharedScene.instance.header.Set(this);
        }
    }

        /// <summary>
    /// ユーザーアイコンクリック時
    /// </summary>
    public virtual void OnClickUserIcon()
    {
        Debug.Log("User Icon !");
    }

    /// <summary>
    /// HOMEボタンクリック時
    /// </summary>
    public virtual void OnClickHomeButton()
    {
        SceneChanger.ChangeSceneAsync("Home");
    }

    /// <summary>
    /// ①Awake ②OnSceneLoaded ③Startの順で呼ばれます
    /// </summary>
    public virtual void OnSceneLoaded(SceneDataPackBase dataPack)
    {
    }
}
