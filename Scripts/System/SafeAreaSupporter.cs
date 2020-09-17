using UnityEngine;

/// <summary>
/// セーフエリア補助
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaSupporter : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, Header("チェックを入れるとEditor再生時にSafeAreaをシミュレートします")]
    private bool isSimulated = false;
#endif

    /// <summary>
    /// RectTransform
    /// </summary>
    protected RectTransform rectTransform { get; private set; }
    /// <summary>
    /// キャンバス
    /// </summary>
    private Canvas canvas = null;

    /// <summary>
    /// Awake
    /// </summary>
    protected virtual void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        this.canvas = this.GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Start
    /// </summary>
    protected virtual void Start()
    {
#if !UNITY_EDITOR && !UNITY_IOS
        //IOS以外はセーフエリア処理しない
        return;
#endif

        var screenSafeArea = Screen.safeArea;
#if UNITY_EDITOR
        if (this.isSimulated)
        {
            //適当にマージン設ける
            var min = screenSafeArea.min;
            var max = screenSafeArea.max;
            var res = UnityEditor.UnityStats.screenRes.Split('x');
            var realScreenSize = new Vector2(int.Parse(res[0]), int.Parse(res[1]));
            min.x += realScreenSize.x * 0.05f;
            min.y += realScreenSize.y * 0.05f;
            max.x -= realScreenSize.x * 0.05f;
            max.y -= realScreenSize.y * 0.05f;
            screenSafeArea.min = min;
            screenSafeArea.max = max;
        }
#endif
        //スクリーン座標系のセーフエリア矩形を、キャンバス座標系に変換
        Vector2 canvasSafeAreaMin;
        Vector2 canvasSafeAreaMax;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.canvas.transform as RectTransform, screenSafeArea.min, this.canvas.worldCamera, out canvasSafeAreaMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.canvas.transform as RectTransform, screenSafeArea.max, this.canvas.worldCamera, out canvasSafeAreaMax);

        //自身のキャンバス座標系の矩形
        Vector2 canvasMyAreaMin;
        Vector2 canvasMyAreaMax;
        canvasMyAreaMin = this.rectTransform.anchoredPosition + this.rectTransform.rect.min;
        canvasMyAreaMax = this.rectTransform.anchoredPosition + this.rectTransform.rect.max;

        //自身の矩形がセーフエリアからはみ出しているならオフセット調整
        Vector2 offsetMin = this.rectTransform.offsetMin;
        Vector2 offsetMax = this.rectTransform.offsetMax;
        offsetMin.x = Mathf.Max(offsetMin.x, canvasSafeAreaMin.x - canvasMyAreaMin.x);
        offsetMin.y = Mathf.Max(offsetMin.y, canvasSafeAreaMin.y - canvasMyAreaMin.y);
        offsetMax.x = Mathf.Min(offsetMax.x, canvasSafeAreaMax.x - canvasMyAreaMax.x);
        offsetMax.y = Mathf.Min(offsetMax.y, canvasSafeAreaMax.y - canvasMyAreaMax.y);
        this.rectTransform.offsetMin = offsetMin;
        this.rectTransform.offsetMax = offsetMax;
    }
}
