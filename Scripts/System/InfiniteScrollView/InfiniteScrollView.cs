using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 無限スクロールビュー
/// </summary>
public class InfiniteScrollView : MonoBehaviour
{
    /// <summary>
    /// 無限かどうか
    /// </summary>
    [SerializeField]
    private bool isInfinite = false;
    /// <summary>
    /// 縦スクロールか横スクロールか
    /// </summary>
    [SerializeField]
    private bool isVertical = false;
    /// <summary>
    /// ページ単位のスクロールかどうか
    /// </summary>
    [SerializeField]
    private bool isPageScroll = false;
    /// <summary>
    /// マルチカラムかどうか
    /// </summary>
    [SerializeField]
    private bool isMultiple = false;
    /// <summary>
    /// コンテンツ終了位置補正をしない
    /// </summary>
    [SerializeField]
    private bool dontCorrectEndPosition = false;
    /// <summary>
    /// カラム間スペース
    /// </summary>
    [SerializeField]
    private float space = 0f;
    /// <summary>
    /// カラム内要素間スペース
    /// </summary>
    [SerializeField]
    private float groupSpace = 0f;
    /// <summary>
    /// 開始位置オフセット
    /// </summary>
    [SerializeField]
    private float offsetMin = 0f;
    /// <summary>
    /// 終了位置オフセット
    /// </summary>
    [SerializeField]
    private float offsetMax = 0f;
    /// <summary>
    /// アンカー
    /// </summary>
    [SerializeField]
    private TextAnchor anchor = TextAnchor.UpperLeft;
    /// <summary>
    /// スクロール感度
    /// </summary>
    [SerializeField]
    private float scrollSensitivity = 1f;
    /// <summary>
    /// ビューポート
    /// </summary>
    [SerializeField]
    private RectTransform viewport = null;
    /// <summary>
    /// スクロールのイベントトリガー
    /// </summary>
    [SerializeField]
    private EventTrigger eventTrigger = null;
    /// <summary>
    /// スクロールバー
    /// </summary>
    [SerializeField]
    public Scrollbar scrollbar = null;
    /// <summary>
    /// スクロールバー表示タイプ
    /// </summary>
    [SerializeField]
    private ScrollRect.ScrollbarVisibility scrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

    /// <summary>
    /// 要素プレハブ
    /// </summary>
    private GameObject elementPrefab = null;
    /// <summary>
    /// スクロール方向別インターフェース
    /// </summary>
    private IDirection direction = null;
    /// <summary>
    /// レイアウトグループタイプ
    /// </summary>
    private Type layoutGroupType = null;
    /// <summary>
    /// 軸に対しての整列方向
    /// </summary>
    private int alignmentDirection = 0;
    /// <summary>
    /// 全要素数
    /// </summary>
    private int maxElementCount = 0;
    /// <summary>
    /// 全カラム数
    /// </summary>
    private int maxGroupCount = 0;
    /// <summary>
    /// カラム内要素数
    /// </summary>
    private int groupSize = 1;
    /// <summary>
    /// コンテンツ現在位置
    /// </summary>
    private float contentPosition = 0f;
    /// <summary>
    /// コンテンツ位置範囲
    /// </summary>
    private Range contentPositionRange = new Range();
    /// <summary>
    /// カラム
    /// </summary>
    private RectTransform[] groups = null;
    /// <summary>
    /// カラムID
    /// </summary>
    private Dictionary<RectTransform, int> groupIds = new Dictionary<RectTransform, int>();
    /// <summary>
    /// 現在のコンテンツ位置の時のページ番号
    /// </summary>
    private int pageNo = 0;
    /// <summary>
    /// SetScrollbarValue関数が呼ばれたかどうか
    /// </summary>
    private bool isSetScrollbarValue = false;
    /// <summary>
    /// 自動スクロール時間
    /// </summary>
    private float autoScrollTime = 0.1f;
    /// <summary>
    /// 自動スクロールコルーチン
    /// </summary>
    private Coroutine autoScrollCoroutine = null;
    /// <summary>
    /// 要素更新時コールバック
    /// </summary>
    private Action<GameObject, int> onUpdateElement = null;
    /// <summary>
    /// ページ切り替え時コールバック
    /// </summary>
    public Action<int> onPageChange = null;
    
    //アンカー関連
    private float pivotValue = 0f;
    private Vector2Int anchorVecInt = Vector2Int.zero;
    private Vector2 anchorVec = Vector2.zero;

    //要素サイズ
    private float elementSizeWithSpace = 0f;
    private float contentPositionOffset = 0f;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        if (this.direction != null)
        {
            return;
        }

        this.anchorVecInt.x = (int)this.anchor % 3;//012,012,012
        this.anchorVecInt.y = (int)this.anchor / 3;//000,111,222
        this.anchorVec.x = this.anchorVecInt.x * 0.5f;
        this.anchorVec.y = (2 - this.anchorVecInt.y) * 0.5f;

        if (this.isVertical)
        {
            this.direction = new VerticalDirection();
            this.layoutGroupType = typeof(HorizontalLayoutGroup);
            this.alignmentDirection = -1;
            this.pivotValue = this.anchorVec.y;
        }
        else
        {
            this.direction = new HorizontalDirection();
            this.layoutGroupType = typeof(VerticalLayoutGroup);
            this.alignmentDirection = 1;
            this.pivotValue = this.anchorVec.x;
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(GameObject elementPrefab, int count, Action<GameObject, int> onUpdateElement)
    {
        this.Awake();

        //クリア
        this.StopAutoScroll();
        this.contentPosition = 0f;
        this.groupIds.Clear();
        this.pageNo = 0;
        while (this.viewport.childCount > 0)
        {
            var child = this.viewport.GetChild(0);
            DestroyImmediate(child.gameObject);
        }

        this.elementPrefab = elementPrefab;
        this.onUpdateElement = (count <= 0) ? null : onUpdateElement;

        //要素サイズ
        Vector2 elementRectSize = (this.elementPrefab.transform as RectTransform).sizeDelta;
        elementRectSize.x *= this.elementPrefab.transform.localScale.x;
        elementRectSize.y *= this.elementPrefab.transform.localScale.y;
        float elementRectSizeValue = this.direction.GetValue(elementRectSize);
        this.elementSizeWithSpace = elementRectSizeValue + this.space;
        this.contentPositionOffset = this.space * 0.5f 
                                   + elementRectSizeValue * (1 - this.pivotValue)
                                   + this.elementSizeWithSpace * (this.pivotValue - 0.5f);
        float viewportRectSizeValue = this.direction.GetValue(this.viewport.rect.size);
        int groupLength = (int)(viewportRectSizeValue / this.elementSizeWithSpace) + 4;
        this.groups = new RectTransform[groupLength];
        int centerNo = (int)((groupLength - 4) * this.direction.GetValue(this.anchorVecInt) + 4) / 2;

        //カラムサイズ
        Vector2 groupRectSize = elementRectSize;
        if (this.isVertical)
        {
            this.groupSize = this.isMultiple ? this.GetGroupSize(this.viewport.rect.size.x, elementRectSize.x, this.groupSpace) : 1;
            groupRectSize.x = (elementRectSize.x + this.groupSpace) * this.groupSize - this.groupSpace;
        }
        else
        {
            this.groupSize = this.isMultiple ? this.GetGroupSize(this.viewport.rect.size.y, elementRectSize.y, this.groupSpace) : 1;
            groupRectSize.y = (elementRectSize.y + this.groupSpace) * this.groupSize - this.groupSpace;
        }

        //要素数、カラム数
        this.maxElementCount = count;
        this.maxGroupCount = Mathf.CeilToInt((float)this.maxElementCount / this.groupSize);

        //コンテンツ位置範囲
        float contentEndPosition = this.elementSizeWithSpace * (this.maxGroupCount - 1) * this.alignmentDirection * -1;
        float scrollbarSize = viewportRectSizeValue / Mathf.Abs(contentEndPosition - this.elementSizeWithSpace * this.alignmentDirection);
        if (this.direction.GetValue(this.anchorVecInt) == 1)
        {
            //アンカーがセンターの場合、コンテンツ終了位置にビューポートサイズの半分を足してやらないといけない
            float tmpContentEndPosition = Mathf.Abs(contentEndPosition);
            tmpContentEndPosition += viewportRectSizeValue * this.direction.GetValue(this.anchorVec);
            scrollbarSize = viewportRectSizeValue / (tmpContentEndPosition - this.elementSizeWithSpace);
        }
        this.SetScrollbarSize(scrollbarSize);
        this.SetScrollbarValue(0f);
        if (!this.isInfinite && !this.dontCorrectEndPosition)
        {
            if (1 < this.maxGroupCount)
            {
                contentEndPosition -= (viewportRectSizeValue - this.offsetMin - this.offsetMax - elementRectSizeValue) * this.alignmentDirection * -1;
            }
        }
        this.contentPositionRange.Set(0f, contentEndPosition);

        for (int y = 0; y < groupLength; y++)
        {
            //カラム作成
            int uncorrectGroupId = y - centerNo;
            int correctGroupId = (int)Mathf.Repeat(uncorrectGroupId, this.maxGroupCount);
            var pos = Vector2.zero;
            this.direction.SetValue(ref pos, this.GetGroupPositionValue(uncorrectGroupId));

            this.groups[y] = new GameObject().AddComponent<RectTransform>();
            this.groups[y].SetParent(this.viewport);
            this.groups[y].localPosition = Vector3.zero;
            this.groups[y].localEulerAngles = Vector3.zero;
            this.groups[y].localScale = Vector3.one;
            this.groups[y].sizeDelta = groupRectSize;
            this.groups[y].anchorMin =
            this.groups[y].anchorMax =
            this.groups[y].pivot = anchorVec;
            this.groups[y].anchoredPosition = pos;
#if UNITY_EDITOR
            this.groups[y].name = correctGroupId.ToString();
#endif
            this.groupIds.Add(this.groups[y], uncorrectGroupId);

            if (this.groupSize > 1)
            {
                var layoutGroup = this.groups[y].gameObject.AddComponent(this.layoutGroupType) as HorizontalOrVerticalLayoutGroup;
                layoutGroup.spacing = this.groupSpace;
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childControlWidth =
                layoutGroup.childControlHeight =
                layoutGroup.childForceExpandWidth =
                layoutGroup.childForceExpandHeight = false;
            }

            for (int x = 0; x < this.groupSize; x++)
            {
                //要素作成
                int uncorrectElementId = uncorrectGroupId * this.groupSize + x;
                int correctElementId = (int)Mathf.Repeat(uncorrectElementId, this.maxElementCount);
                bool isInRange = 0 <= uncorrectElementId && uncorrectElementId < this.maxElementCount;

                var element = Instantiate(this.elementPrefab, this.groups[y], false);
                element.SetActive(this.isInfinite || isInRange);
#if UNITY_EDITOR
                element.name = correctElementId.ToString();
#endif
                this.onUpdateElement?.Invoke(element.gameObject, correctElementId);

            }
        }

        //ページ番号更新通知
        this.onPageChange?.Invoke(this.pageNo);
    }

    public void UpdateElement()
    {
        for (int i = 0; i < this.groups.Length; i++)
        {
            for (int j = 0; j < this.groupSize; j++)
            {
                int uncorrectGroupId = this.groupIds[this.groups[i]];
                int correctGroupId = (int)Mathf.Repeat(uncorrectGroupId, this.maxGroupCount);

                var gobj = this.groups[i].GetChild(j).gameObject;
                if (gobj.activeSelf)
                {
                    int uncorrectElementId = uncorrectGroupId * this.groupSize + j;
                    int correctElementId = (int)Mathf.Repeat(uncorrectElementId, this.maxElementCount);
                    this.onUpdateElement?.Invoke(gobj, correctElementId);
                }
            }
        }
    }

    /// <summary>
    /// 指定ID要素にフォーカス
    /// </summary>
    public void SetFocus(int elementId, bool withAutoScroll = false)
    {
        this.StopAutoScroll();

        if (!this.isInfinite)
        {
            elementId = (int)Mathf.Repeat(elementId, this.maxElementCount);
        }

        int groupId = Mathf.FloorToInt((float)elementId / this.groupSize);

        if (withAutoScroll)
        {
            this.autoScrollCoroutine = StartCoroutine(this.AutoPageScroll(groupId));
        }
        else
        {
            float newContentPosition = this.GetTargetPosition(groupId);
            float delta = newContentPosition - this.contentPosition;
            this.OnDrag(delta);
        }
    }

    /// <summary>
    /// カラム内要素数の計算
    /// </summary>
    private int GetGroupSize(float viewportRectSize, float elementRectSize, float space)
    {
        return Mathf.Max(1, (int)((viewportRectSize + space) / (elementRectSize + space)));
    }

    /// <summary>
    /// 指定IDカラムの座標を取得
    /// </summary>
    private float GetGroupPositionValue(int groupId)
    {
        return (this.elementSizeWithSpace * groupId + this.offsetMin) * this.alignmentDirection;
    }

    /// <summary>
    /// 指定IDカラムにフォーカスする際のコンテンツ座標を取得
    /// </summary>
    private float GetTargetPosition(int groupId)
    {
        float targetPosition = -this.GetGroupPositionValue(groupId) + (this.offsetMin * this.alignmentDirection);

        if (!this.isInfinite)
        {
            targetPosition = this.contentPositionRange.Clamp(targetPosition);
        }

        return targetPosition;
    }

    /// <summary>
    /// スクロールバーサイズのセット
    /// </summary>
    private void SetScrollbarSize(float size)
    {
        eventTrigger.enabled = size < 1.0f;

        if (this.scrollbar != null)
        {
            this.scrollbar.size = size;

            if (this.isInfinite)
            {
                this.scrollbar.gameObject.SetActive(false);
            }
            else if (this.scrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent)
            {
                this.scrollbar.gameObject.SetActive(true);
            }
            else
            {
                this.scrollbar.gameObject.SetActive(size < 1f);
            }
        }
    }

    /// <summary>
    /// スクロールバー値のセット
    /// </summary>
    private void SetScrollbarValue(float value)
    {
        if (this.scrollbar != null)
        {
            this.isSetScrollbarValue = true;
            this.scrollbar.value = Mathf.Clamp01(value);
            this.isSetScrollbarValue = false;
        }
    }

    /// <summary>
    /// スクロールバー操作時
    /// </summary>
    public void OnScrollbarValueChanged(float value)
    {
        //SetScrollbarValueで呼ばれた時は何もしない
        if (this.isSetScrollbarValue)
        {
            return;
        }

        float newContentPosition = this.contentPositionRange.end * value;
        float delta = newContentPosition - this.contentPosition;
        this.OnDrag(delta);
    }

    /// <summary>
    /// ドラッグ開始時 = OnPointerDown
    /// </summary>
    public void OnBeginDrag(BaseEventData eventData)
    {
        this.StopAutoScroll();
    }

    /// <summary>
    /// ドラッグ時
    /// </summary>
    public void OnDrag(BaseEventData eventData)
    {
        var data = (eventData as PointerEventData);
        var delta = this.direction.GetValue(data.delta) * this.scrollSensitivity;
        this.OnDrag(delta);
    }

    /// <summary>
    /// ドラッグ時
    /// </summary>
    private void OnDrag(float delta)
    {
        if (this.groupIds.Count == 0)
        {
            return;
        }

        this.RepackGroups();

        //コンテンツ位置移動
        this.contentPosition += delta;
        float scrollbarValue = (this.contentPositionRange.end == 0f) ? 0f : this.contentPosition / this.contentPositionRange.end;
        this.SetScrollbarValue(scrollbarValue);

        //コンテンツ位置に合わせてカラム移動
        foreach (var group in this.groups)
        {
            Vector2 pos = Vector2.zero;
            float groupPositionValue = this.GetGroupPositionValue(this.groupIds[group]);
            this.direction.SetValue(ref pos, this.contentPosition + groupPositionValue);
            group.anchoredPosition = pos;
        }

        //コンテンツ位置移動した時の新しいページ番号
        int newPageNo = Mathf.FloorToInt((this.contentPosition + this.contentPositionOffset) / this.elementSizeWithSpace) * this.alignmentDirection * -1;
        int oldPageNo = this.pageNo;

        //ページ切り替えて要素の更新
        while (newPageNo != this.pageNo)
        {
            RectTransform groupA = null;
            RectTransform groupB = null;
            int sign = 1;
            Action setSibling = null;

            if (newPageNo < this.pageNo)
            {
                groupA = this.viewport.GetChild(this.viewport.childCount - 1) as RectTransform;
                groupB = this.viewport.GetChild(0) as RectTransform;
                sign = -1;
                setSibling = groupA.SetAsFirstSibling;
            }
            else
            {
                groupA = this.viewport.GetChild(0) as RectTransform;
                groupB = this.viewport.GetChild(this.viewport.childCount - 1) as RectTransform;
                sign = 1;
                setSibling = groupA.SetAsLastSibling;
            }

            //カラムのループ移動
            var pos = groupB.anchoredPosition;
            this.direction.AddValue(ref pos, this.elementSizeWithSpace * this.alignmentDirection * sign);
            setSibling?.Invoke();
            groupA.anchoredPosition = pos;

            //カラムIDの更新
            int uncorrectGroupId = this.groupIds[groupB] + sign;
            int correctGroupId = (int)Mathf.Repeat(uncorrectGroupId, this.maxGroupCount);
            this.groupIds[groupA] = uncorrectGroupId;
#if UNITY_EDITOR
            groupA.name = correctGroupId.ToString();
#endif
            //カラム内要素の更新
            for (int i = 0; i < this.groupSize; i++)
            {
                int uncorrectElementId = uncorrectGroupId * this.groupSize + i;
                int correctElementId = (int)Mathf.Repeat(uncorrectElementId, this.maxElementCount);
                bool isInRange = 0 <= uncorrectElementId && uncorrectElementId < this.maxElementCount;

                var element = groupA.GetChild(i);
                element.gameObject.SetActive(this.isInfinite || isInRange);
#if UNITY_EDITOR
                element.name = correctElementId.ToString();
#endif
                this.onUpdateElement?.Invoke(element.gameObject, correctElementId);
            }

            //ページ番号更新
            this.pageNo += sign;
        }

        if (newPageNo != oldPageNo)
        {
            //ページ番号更新通知
            this.onPageChange?.Invoke(newPageNo);
        }
    }

    /// <summary>
    /// ドラッグ終了時 = OnPointerUp
    /// </summary>
    public void OnEndDrag(BaseEventData eventData)
    {
        if (this.autoScrollCoroutine != null)
        {
            return;
        }

        //ページ単位のスクロールの場合
        if (this.isPageScroll)
        {
            int groupId = this.isInfinite ? this.pageNo : Mathf.Clamp(this.pageNo, 0, this.maxGroupCount - 1);
            this.autoScrollCoroutine = StartCoroutine(this.AutoPageScroll(groupId));
        }
        //通常スクロール
        else
        {
            //無限スクロールじゃないときに、コンテンツ位置が範囲外の場合
            if (!this.isInfinite && !this.contentPositionRange.InRange(this.contentPosition))
            {
                int groupId = Mathf.Clamp(this.pageNo, 0, this.maxGroupCount - 1);
                this.autoScrollCoroutine = StartCoroutine(this.AutoPageScroll(groupId));
            }
            else
            {
                var data = (eventData as PointerEventData);
                var delta = this.direction.GetValue(data.delta);
                this.autoScrollCoroutine = StartCoroutine(this.AutoScroll(delta));
            }
        }
    }

    /// <summary>
    /// 指定カラムIDの位置まで自動スクロールする
    /// </summary>
    private IEnumerator AutoPageScroll(int groupId)
    {
        float targetPosition = this.GetTargetPosition(groupId);
        float distance = targetPosition - this.contentPosition;
        float delta = distance * (Time.deltaTime / this.autoScrollTime);
        float time = this.autoScrollTime;

        do {
            this.OnDrag(delta);
            time -= Time.deltaTime;
            yield return null;
        } while (time > 0f);

        this.OnDrag(targetPosition - this.contentPosition);
    }

    /// <summary>
    /// 自動スクロール
    /// </summary>
    private IEnumerator AutoScroll(float delta)
    {
        var deltaRange = new Range();
        deltaRange.Set(0f, delta);

        while (deltaRange.InRange(delta))
        {
            //コンテンツ移動予定値
            float newContentPosition = this.contentPosition + delta;

            //無限スクロールじゃない場合
            if (!this.isInfinite)
            {
                //コンテンツ位置が範囲外にならないようにする
                if (newContentPosition < this.contentPositionRange.min)
                {
                    this.OnDrag(this.contentPositionRange.min - this.contentPosition);
                    break;
                }
                else if (newContentPosition > this.contentPositionRange.max)
                {
                    this.OnDrag(this.contentPositionRange.max - this.contentPosition);
                    break;
                }
            }

            //予定地まで移動
            this.OnDrag(delta);

            //移動量を少しずつ減らす
            delta -= delta * Time.deltaTime * this.scrollSensitivity;

            yield return null;
        }
    }

    /// <summary>
    /// 自動スクロール終了
    /// </summary>
    private void StopAutoScroll()
    {
        if (this.autoScrollCoroutine != null)
        {
            StopCoroutine(this.autoScrollCoroutine);
            this.autoScrollCoroutine = null;
        }
    }

    /// <summary>
    /// カラムをSiblingIndex順に詰め直す
    /// </summary>
    private void RepackGroups()
    {
        for (int i = 0; i < this.groups.Length; i++)
        {
            this.groups[i] = this.viewport.GetChild(i) as RectTransform;
        }
    }

    /// <summary>
    /// 方向インターフェース
    /// </summary>
    public interface IDirection
    {
        void SetValue(ref Vector2 v, float value);
        void AddValue(ref Vector2 v, float value);
        float GetValue(Vector2 v);
        int GetValue(Vector2Int v);
    }

    /// <summary>
    /// 縦方向
    /// </summary>
    private class VerticalDirection : IDirection
    {
        void IDirection.SetValue(ref Vector2 v, float value) { v.y = value; }
        void IDirection.AddValue(ref Vector2 v, float value) { v.y += value; }
        float IDirection.GetValue(Vector2 v) { return v.y; }
        int IDirection.GetValue(Vector2Int v) { return v.y; }
    }

    /// <summary>
    /// 横方向
    /// </summary>
    private class HorizontalDirection : IDirection
    {
        void IDirection.SetValue(ref Vector2 v, float value) { v.x = value; }
        void IDirection.AddValue(ref Vector2 v, float value) { v.x += value; }
        float IDirection.GetValue(Vector2 v) { return v.x; }
        int IDirection.GetValue(Vector2Int v) { return v.x; }
    }

    /// <summary>
    /// 範囲
    /// </summary>
    private class Range
    {
        /// <summary>
        /// 最小値
        /// </summary>
        public float min = 0f;
        /// <summary>
        /// 最大値
        /// </summary>
        public float max = 0f;
        /// <summary>
        /// 開始値
        /// </summary>
        public float start = 0f;
        /// <summary>
        /// 終了値
        /// </summary>
        public float end = 0f;

        /// <summary>
        /// セット
        /// </summary>
        public void Set(float start, float end)
        {
            this.start = start;
            this.end = end;
            this.min = Mathf.Min(start, end);
            this.max = Mathf.Max(start, end);
        }

        /// <summary>
        /// 範囲内かどうか
        /// </summary>
        public bool InRange(float value)
        {
            return this.min <= value && value <= this.max;
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, this.min, this.max);
        }
    }
}
