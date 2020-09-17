using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// SharedScene
/// </summary>
public class SharedScene : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// SharedScene経由後に開くシーン名のEditorPrefsキー
    /// </summary>
    private const string NEXT_SCENENAME_KEY = "SharedScene.NextSceneName";

    /// <summary>
    /// 再生時開始シーンをセットする
    /// </summary>
    /// <para>
    /// エディタ起動時やスクリプトコンパイル時に呼ばれる
    /// </para>
    [InitializeOnLoadMethod]
    private static void SetPlayModeStartScene()
    {
        //エディタ上でシーン開いたときのイベントをセット
        EditorSceneManager.activeSceneChangedInEditMode += (closedScene, openedScene) =>
        {
            EditorSceneManager.playModeStartScene = null;

            //指定ディレクトリ外のシーンの場合は再生時にSharedSceneを経由したくないのでスルーする
            if (!openedScene.path.Contains("Assets/Sunchoi/Scenes")) return;

            //再生時開始シーンをSharedSceneに設定
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Sunchoi/Scenes/Shared.unity");

            //SharedScene経由後に開くシーンとして、今開いたシーンの名前を保存
            EditorPrefs.SetString(NEXT_SCENENAME_KEY, openedScene.name);

            Debug.LogFormat("SetPlayModeStartScene: {0}", openedScene.name);
        };
    }
#endif

    /// <summary>
    /// インスタンス
    /// </summary>
    public static SharedScene instance { get; private set; }

    [Header("Header Panel")]
    /// <summary>
    /// ヘッダー生成先
    /// </summary>
    [SerializeField]
    private RectTransform headerParent = null;
    /// <summary>
    /// ヘッダープレハブ
    /// </summary>
    [SerializeField]
    private HeaderPanel headerPrefab = null;
    /// <summary>
    /// ヘッダー
    /// </summary>
    public HeaderPanel header { get; private set; }


    // Simple Dialo
    [Header("Simple Dialog")]
    /// <summary>
    /// ダイアログ生成先
    /// </summary>
    [SerializeField]
    private RectTransform dialogRoot = null;
        /// <summary>
    /// システムダイアログ生成先
    /// </summary>
    [SerializeField]
    private RectTransform systemDialogRoot = null;
    /// <summary>
    /// ダイアログプレハブ
    /// </summary>
    [SerializeField]
    private SimpleDialog simpleDialogPrefab = null;

    // Scene Change Animation
    [Header("Scene Change Animation")]
    /// <summary>
    /// シーン遷移時アニメ生成先
    /// </summary>
    [SerializeField]
    private RectTransform sceneChangeAnimationRoot = null;
    /// <summary>
    /// シーン遷移アニメーションプレハブ
    /// </summary>
    [SerializeField]
    private SceneChangeAnimation sceneChangeAnimationPrefab = null;
    /// <summary>
    /// シーン遷移アニメーション
    /// </summary>
    private SceneChangeAnimation sceneChangeAnimation = null;

    // ConnectingIndicator
    [Header("Connecting Indicator")]
    /// <summary>
    /// 通信中アニメ生成先
    /// </summary>
    [SerializeField]
    private RectTransform connectingRoot = null;
    /// <summary>
    /// 通信中アニメプレハブ
    /// </summary>
    [SerializeField]
    private ConnectingIndicator connectingIndicatorPrefab = null;
    /// <summary>
    /// 通信中アニメ
    /// </summary>
    private ConnectingIndicator connectingIndicator = null;

    // touchDisabler
    [Header("Touch Disabler")]
    /// <summary>
    /// タッチ制御
    /// </summary>
    [SerializeField]
    private Image touchDisabler = null;

    [Header("Gray Scale")]
    /// <summary>
    /// グレースケール用マテリアル
    /// </summary>
    [SerializeField]
    public Material grayScaleMaterial = null;

    [Header("Common Atlas")]
    /// <summary>
    /// Commonアトラス
    /// </summary>
    [SerializeField]
    public AtlasSpriteCache commonAtlas = null;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        instance = this;

        //SharedSceneは破棄されないようにする
        DontDestroyOnLoad(this.gameObject);

        //シーン遷移アニメーションイベント登録
        SceneChanger.onShowSceneChangeAnimation = this.ShowSceneChangeAnimation;
        SceneChanger.onHideSceneChangeAnimation = this.HideSceneChangeAnimation;

#if UNITY_EDITOR
        if (EditorPrefs.HasKey(NEXT_SCENENAME_KEY))
        {
            //SharedSceneに来る前のシーンを開く（前のシーンもSharedSceneだったらスルー）
            string nextSceneName = EditorPrefs.GetString(NEXT_SCENENAME_KEY);
            if (nextSceneName != EditorSceneManager.GetActiveScene().name)
            {
                SceneChanger.ChangeSceneAsync(nextSceneName);
                return;
            }
        }
#endif

        //SharedSceneの次のシーンはTitle？に遷移
        SceneChanger.ChangeSceneAsync("Title");
    }

    /// <summary>
    /// 通信中マークを表示（タッチブロックされる）
    /// </summary>
    public void ShowConnectingIndicator()
    {
        if (this.connectingIndicator == null)
        {
            this.connectingIndicator = Instantiate(this.connectingIndicatorPrefab, this.connectingRoot, false);
        }

        this.connectingIndicator.Play();
    }

    /// <summary>
    /// 通信中マークを非表示
    /// </summary>
    public void HideConnectingIndicator()
    {
        if (this.connectingIndicator != null)
        {
            this.connectingIndicator.Destroy();
        }
    }



    

    /// <summary>
    /// シーン遷移アニメーション表示
    /// </summary>
    private void ShowSceneChangeAnimation(Action onFinishedIn)
    {
        if (this.sceneChangeAnimation == null)
        {
            this.sceneChangeAnimation = Instantiate(this.sceneChangeAnimationPrefab, this.sceneChangeAnimationRoot, false);
            this.sceneChangeAnimation.onFinishedIn = onFinishedIn;
        }
        else
        {
            onFinishedIn?.Invoke();
        }
    }

    /// <summary>
    /// シーン遷移アニメーション非表示
    /// </summary>
    private void HideSceneChangeAnimation(Action onFinished = null)
    {
        if (this.sceneChangeAnimation != null)
        {
            this.sceneChangeAnimation.SetOut();
            this.sceneChangeAnimation.onFinishedOut = () =>
            {
                Destroy(this.sceneChangeAnimation.gameObject);
                this.sceneChangeAnimation = null;
                onFinished?.Invoke();
            };
        }
    }

    /// <summary>
    /// シンプルダイアログ表示
    /// </summary>
    public SimpleDialog ShowSimpleDialog(bool isSystem = false)
    {
        if (isSystem)
        {
            return Instantiate(this.simpleDialogPrefab, this.systemDialogRoot, false);
        }
        else
        {
            return Instantiate(this.simpleDialogPrefab, this.dialogRoot, false);
        }
    }

    /// <summary>
    /// タッチ禁止
    /// </summary>
    public void DisableTouch()
    {
        touchDisabler.enabled = true;
    }

    /// <summary>
    /// タッチ許容
    /// </summary>
    public void EnableTouch()
    {
        touchDisabler.enabled = false;
    }

    public void ShowHeader()
    {
        if (this.header == null)
        {
            this.header = Instantiate(this.headerPrefab, this.headerParent, false);
        }

        this.header.gameObject.SetActive(true);
    }

    public void HideHeader()
    {
        if (this.header != null)
        {
            this.header.gameObject.SetActive(false);
        }
    }
}
