using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シンプルダイアログ
/// </summary>
public class SimpleDialog : DialogBase
{
    /// <summary>
    /// 余白
    /// </summary>
    [Serializable]
    public struct Margin
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    /// <summary>
    /// Yes/Noボタングループ
    /// </summary>
    public class YesNoButtonGroup
    {
        /// <summary>
        /// Yesボタン
        /// </summary>
        public SimpleDialogButton yes = null;
        /// <summary>
        /// Noボタン
        /// </summary>
        public SimpleDialogButton no = null;
    }

    /// <summary>
    /// 最小ウィンドウサイズ
    /// </summary>
    [SerializeField]
    private Vector2 minWindowSize = new Vector2(1920f, 600f);
    /// <summary>
    /// 最大ウィンドウサイズ
    /// </summary>
    [SerializeField]
    private Vector2 maxWindowSize = new Vector2(1920f, 1080f);
    /// <summary>
    /// 余白
    /// </summary>
    [SerializeField]
    private Margin margin = new Margin();
    /// <summary>
    /// タイトルテキスト
    /// </summary>
    [SerializeField]
    public Text titleText = null;
    /// <summary>
    /// シンプルテキストプレハブ
    /// </summary>
    [SerializeField]
    private Text simpleTextPrefab = null;
    /// <summary>
    /// シンプルボタングループプレハブ
    /// </summary>
    [SerializeField]
    private SimpleDialogButtonGroup buttonGroupPrefab = null;
    /// <summary>
    /// Closeボタン
    /// </summary>
    [SerializeField]
    private Button closeButton = null;

    /// <summary>
    /// 最大コンテンツ幅
    /// </summary>
    private float maxContentWidth = 0f;
    /// <summary>
    /// Closeボタン有効無効
    /// </summary>
    [NonSerialized]
    public bool closeButtonEnabled = false;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.maxContentWidth = this.maxWindowSize.x - (this.margin.left + this.margin.right);
    }

    /// <summary>
    /// Start
    /// </summary>
    protected override void Start()
    {
        //コンテンツサイズからウィンドウサイズを計算
        var size = this.window.sizeDelta;
        size.y = this.margin.top + this.GetContentTotalHeight() + this.margin.bottom;

        //ウィンドウサイズが最小値より小さくならないように
        if (size.y < this.minWindowSize.y)
        {
            size.y = this.minWindowSize.y;
        }

        this.window.sizeDelta = size;

        //Closeボタン表示切り替え
        this.closeButton?.gameObject.SetActive(this.closeButtonEnabled);

        base.Start();
    }

    /// <summary>
    /// 総コンテンツ高を取得
    /// </summary>
    private float GetContentTotalHeight()
    {
        float height = 0f;

        for (int i = 2; i < this.window.childCount; i++)
        {
            height += (this.window.GetChild(i) as RectTransform).rect.height;
        }

        return height;
    }

    /// <summary>
    /// コンテンツ追加
    /// </summary>
    public T AddContent<T>(T prefab) where T : Component
    {
        //コンテンツ位置
        float posY = (this.margin.top + this.GetContentTotalHeight()) * -1;

        //コンテンツ生成
        var content = Instantiate(prefab, this.window, false);
        var contentRect = content.transform as RectTransform;
        contentRect.anchorMin =
        contentRect.anchorMax =
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = new Vector2(0f, posY);

        return content;
    }

    /// <summary>
    /// コンテンツ追加
    /// </summary>
    public GameObject AddContent(GameObject prefab)
    {
        return this.AddContent(prefab.transform).gameObject;
    }

    /// <summary>
    /// テキストコンテンツ追加
    /// </summary>
    public Text AddText(string msg)
    {
        var content = this.AddContent(this.simpleTextPrefab);
        content.text = msg;

        //サイズ調整
        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.SetLayoutHorizontal();

        var size = content.rectTransform.sizeDelta;

        //コンテンツ幅を範囲内に収める
        size.x = Mathf.Min(size.x, this.maxContentWidth);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        content.rectTransform.sizeDelta = size;

        //コンテンツ幅を範囲内に収めた結果のコンテンツ高さに50pxほど余白を持たせる
        fitter.SetLayoutVertical();
        size = content.rectTransform.sizeDelta;
        size.y += 50f;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        content.rectTransform.sizeDelta = size;

        //サイズ調整終了
        Destroy(fitter);

        return content;
    }

    /// <summary>
    /// ボタンコンテンツ追加
    /// </summary>
    public SimpleDialogButtonGroup AddButton(int size)
    {
        var buttonGroup = this.AddContent(this.buttonGroupPrefab);
        buttonGroup.AddButton(size);

        //サイズ調整
        var fitter = buttonGroup.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonGroup.transform as RectTransform);
        Destroy(fitter);

        return buttonGroup;
    }

    /// <summary>
    /// OKボタン追加
    /// </summary>
    public SimpleDialogButton AddOKButton()
    {
        var buttonGroup = this.AddButton(1);
        buttonGroup.buttons[0].text.text = Masters.LocalizeTextDB.Get("OK");
        return buttonGroup.buttons[0];
    }

    /// <summary>
    /// Yes/Noボタンコンテンツ追加
    /// </summary>
    public YesNoButtonGroup AddYesNoButton()
    {
        var buttonGroup = this.AddButton(2);
        var yesNo = new YesNoButtonGroup();

        yesNo.yes = buttonGroup.buttons[1];
        yesNo.yes.image.sprite = SharedScene.instance.commonAtlas.GetSprite(SlotDefine.YES_BTN_SPRITE_NAME);
        yesNo.yes.text.text = Masters.LocalizeTextDB.Get("Yes");

        yesNo.no = buttonGroup.buttons[0];
        yesNo.no.image.sprite = SharedScene.instance.commonAtlas.GetSprite(SlotDefine.NO_BTN_SPRITE_NAME);
        yesNo.no.text.text = Masters.LocalizeTextDB.Get("No");
        //yesNo.no.button.GetComponent<ButtonSe>().seName = SeName.NO;

        return yesNo;
    }

    /// <summary>
    /// メッセージダイアログとしてセット
    /// </summary>
    public (Text text, SimpleDialogButtonGroup buttonGroup) SetAsMessageDialog(string msg, int btnSize = 1)
    {
        var text = this.AddText("\n" + msg + "\n");
        var buttonGroup = this.AddButton(btnSize);

        if (btnSize > 0)
        {
            //buttonGroup.buttons[0].text.text = Masters.LocalizeTextDB.Get("OK");
        }

        return (text, buttonGroup);
    }

    /// <summary>
    /// YesNoメッセージダイアログとしてセット
    /// </summary>
    public (Text text, YesNoButtonGroup yesNo) SetAsYesNoMessageDialog(string msg)
    {
        return (this.AddText("\n" + msg + "\n"), this.AddYesNoButton());
    }
}
