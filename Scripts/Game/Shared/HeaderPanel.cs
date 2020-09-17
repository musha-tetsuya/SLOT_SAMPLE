using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ヘッダー
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HeaderPanel : SafeAreaSupporter
{
    /// <summary>
    /// イベントリスナー
    /// </summary>
    public interface IEventListner
    {
        /// <summary>
        /// ユーザーアイコンクリック時
        /// </summary>
        void OnClickUserIcon();
        /// <summary>
        /// HOMEボタンクリック時
        /// </summary>
        void OnClickHomeButton();
    }

    //背景
    [SerializeField]
    private Image bg = null;

    /// <summary>
    /// HOMEボタン
    /// </summary>
    [SerializeField]
    private GameObject homeButton = null;
    /// <summary>
    /// ユーザーアイコン
    /// </summary>
    [SerializeField]
    private GameObject userIcon = null;

    /// <summary>
    /// ユーザー名テキスト
    /// </summary>
    [SerializeField]
    private Text userNameText = null;
    /// <summary>
    /// ユーザーレベルテキスト
    /// </summary>
    [SerializeField]
    private Text userLvText = null;
    /// <summary>
    /// コイン数テキスト
    /// </summary>
    [SerializeField]
    private Text coinText = null;

    /// <summary>
    /// キャンバスグループ
    /// </summary>
    public CanvasGroup canvasGroup { get; private set; }
    /// <summary>
    /// イベントリスナー
    /// </summary>
    private IEventListner eventListner = null;

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Start
    /// </summary>
    protected override void Start()
    {
        base.Start();
        this.SetBgRectSize();
    }

    /// <summary>
    /// SafeAreaを考慮してBGサイズを調整
    /// </summary>
    private void SetBgRectSize()
    {
        Vector2 size;
        Vector2 pos;

        size = this.bg.rectTransform.sizeDelta;
        size.x += this.rectTransform.offsetMin.x;
        this.bg.rectTransform.sizeDelta = size;

        pos = this.bg.rectTransform.anchoredPosition;
        pos.x -= this.rectTransform.offsetMin.x;
        this.bg.rectTransform.anchoredPosition = pos;
    }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(SceneBase scene)
    {
        //イベントリスナー登録
        this.eventListner = scene;

        //HOMEボタン←→ユーザーアイコン切り替え
        bool isHome = scene is HomeScene;
        this.homeButton.SetActive(!isHome);
        this.userIcon.SetActive(isHome);

        //表示情報の設定 TODO. SetInfo(UserData.Get());
        SetInfo(UserData.Get());
    }

    /// <summary>
    /// 表示する情報のみの更新 TODO. (UserData userData)
    /// </summary>
    public void SetInfo(UserData userData)
    {
        if (userData != null)
        {
            //ユーザー名
            this.userNameText.text = userData.name;

            //ユーザーレベル
            this.userLvText.text = string.Format("Lv.{0}", userData.lv);

            //コイン
            this.coinText.text = userData.coin.ToString();
        }
    }

    /// <summary>
    /// ユーザーアイコンクリック時
    /// </summary>
    public void OnClickUserIcon()
    {
        this.eventListner?.OnClickUserIcon();
    }

    /// <summary>
    /// HOMEボタンクリック時
    /// </summary>
    public void OnClickHomeButton()
    {
        this.eventListner?.OnClickHomeButton();
    }
}
