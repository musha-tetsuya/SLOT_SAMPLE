using System.Collections;
using UnityEngine;

/// <summary>
/// 通信中マーク
/// </summary>
public class ConnectingIndicator : MonoBehaviour
{
    /// <summary>
    /// 破棄コルーチン
    /// </summary>
    private Coroutine destroyCoroutine = null;

    /// <summary>
    /// Play
    /// </summary>
    public void Play()
    {
        if (this.destroyCoroutine != null)
        {
            StopCoroutine(this.destroyCoroutine);
            this.destroyCoroutine = null;
        }
    }

    /// <summary>
    /// 破棄
    /// </summary>
    public void Destroy()
    {
        if (this.destroyCoroutine == null)
        {
            this.destroyCoroutine = StartCoroutine(this.DestroyCoroutine());
        }
    }

    /// <summary>
    /// 破棄処理
    /// </summary>
    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }
}
