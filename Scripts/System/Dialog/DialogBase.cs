using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダイアログ基底
/// </summary>
public class DialogBase : MonoBehaviour
{
    /// <summary>
    /// BG
    /// </summary>
    [SerializeField]
    private Image backGround = null;
    /// <summary>
    /// ウィンドウ
    /// </summary>
    [SerializeField]
    protected RectTransform window = null;
    /// <summary>
    /// キャンバスグループ
    /// </summary>
    [SerializeField]
    private CanvasGroup canvasGroup = null;
    /// <summary>
    /// Close時スケール
    /// </summary>
    [SerializeField]
    private Vector2 closeScale = Vector2.zero;
    /// <summary>
    /// Open時スケール
    /// </summary>
    [SerializeField]
    private Vector2 openScale = Vector2.one;

    /// <summary>
    /// Close処理が呼ばれたかどうか
    /// </summary>
    public bool isClose { get; private set; }
    /// <summary>
    /// ダイアログが完全に開けた後のコールバック
    /// </summary>
    public Action onCompleteShow = null;
    /// <summary>
    /// Close時コールバック
    /// </summary>
    public Action onClose = null;

    /// <summary>
    /// Start
    /// </summary>
    protected virtual void Start()
    {
        this.Show();
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (this.isClose)
        {
            this.onClose?.Invoke();
        }
    }

    /// <summary>
    /// ダイアログ表示
    /// </summary>
    private void Show()
    {
        //ダイアログが開ききるまでダイアログ内のものには触れないようにする
        this.canvasGroup.blocksRaycasts = false;

        //ダイアログ開くアニメーション
        this.StartCoroutine(this.WindowAnimation(0f, 0.5f, this.closeScale, this.openScale, () =>
        {
            //ダイアログ内触れるように
            this.canvasGroup.blocksRaycasts = true;

            //コールバック実行
            this.onCompleteShow?.Invoke();
        }));
    }

    /// <summary>
    /// ダイアログ閉じる
    /// </summary>
    public virtual void Close()
    {
        this.isClose = true;

        //ダイアログ閉じてるアニメーション中にダイアログ内のものには触れないようにする
        this.canvasGroup.blocksRaycasts = false;

        //ダイアログ閉じるアニメーション
        this.StartCoroutine(this.WindowAnimation(0.5f, 0f, this.openScale, this.closeScale, () =>
        {
            //自身を破棄
            Destroy(this.gameObject);
        }));
    }

    /// <summary>
    /// ウィンドウアニメーション
    /// </summary>
    private IEnumerator WindowAnimation(
        float fromBgAlpha,
        float toBgAlpha,
        Vector2 fromScale,
        Vector2 toScale,
        Action onFinished)
    {
        //アニメーション時間
        const float ALPHA_DURATION = 0.5f;
        const float SCALE_DURATION = 0.5f;

        //αとスケールの初期値設定
        this.backGround.color = new Color(0f, 0f, 0f, fromBgAlpha);
        this.window.localScale = fromScale;

        bool isFinishedAlpha = false;
        bool isFinishedScale = false;
        float time = 0f;

        while (!isFinishedAlpha || !isFinishedScale)
        {
            //アニメーション終わったかどうか
            isFinishedAlpha = time >= ALPHA_DURATION;
            isFinishedScale = time >= SCALE_DURATION;

            //α値更新
            var bgColor = this.backGround.color;
            bgColor.a = Mathf.Lerp(fromBgAlpha, toBgAlpha, time / ALPHA_DURATION);
            this.backGround.color = bgColor;

            //スケール値更新
            this.window.localScale = fromScale + (toScale - fromScale) * (1f - Mathf.Pow(2, -10 * Mathf.Clamp01(time / SCALE_DURATION)));

            //時間カウント
            time += Time.deltaTime;

            yield return null;
        }

        //終了時コールバック
        onFinished?.Invoke();
    }
}
