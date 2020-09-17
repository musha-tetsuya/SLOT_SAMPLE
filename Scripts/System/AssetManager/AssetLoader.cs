using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IAssetLoader
{
    /// <summary>
    /// パス
    /// </summary>
    string path { get; }
    /// <summary>
    /// ハンドル
    /// </summary>
    AssetLoadHandle handle { get; }
    /// <summary>
    /// ロード
    /// </summary>
    AssetLoadHandle Load(Action onLoaded = null);
    /// <summary>
    /// アンロード
    /// </summary>
    void Unload();
}

/// <summary>
/// アセットローダー
/// </summary>
public class AssetLoader<T> : IAssetLoader where T : UnityEngine.Object
{
    /// <summary>
    /// パス
    /// </summary>
    public string path { get; private set; }
    /// <summary>
    /// ハンドル
    /// </summary>
    public AssetLoadHandle handle { get; private set; }
    /// <summary>
    /// construct
    /// </summary>
    public AssetLoader(string path)
    {
        this.path = path;
    }
    /// <summary>
    /// ロード
    /// </summary>
    public AssetLoadHandle Load(Action onLoaded = null)
    {
        return this.handle = AssetManager.Load<T>(path, (_) => onLoaded?.Invoke());
    }
    /// <summary>
    /// アンロード
    /// </summary>
    public void Unload()
    {
        AssetManager.Unload(this.handle);
    }
}

public class AssetListLoader : List<IAssetLoader>
{
    /// <summary>
    /// ステータス
    /// </summary>
    public enum Status
    {
        Empty,
        NeedLoad,
        Loading,
        Loaded,
    }
    /// <summary>
    /// ロード完了時コールバック
    /// </summary>
    private Action onLoaded = null;
    /// <summary>
    /// construct
    /// </summary>
    public AssetListLoader() : base() {}
    public AssetListLoader(IEnumerable<IAssetLoader> collection) : base(collection) {}
    /// <summary>
    /// パスでアクセス
    /// </summary>
    public IAssetLoader this[string path]
    {
        get { return this.Find(x => x.path.Equals(path, StringComparison.OrdinalIgnoreCase)); }
    }
    /// <summary>
    /// ロード対象の追加
    /// </summary>
    public IAssetLoader Add<T>(string path) where T : UnityEngine.Object
    {
        var loader = new AssetLoader<T>(path);
        this.Add(loader);
        return loader;
    }
    /// <summary>
    /// クリア
    /// </summary>
    public new void Clear()
    {
        this.onLoaded = null;
        base.Clear();
    }
    /// <summary>
    /// ロード実行
    /// </summary>
    public void Load(Action onLoaded = null)
    {
        this.onLoaded = onLoaded;

        if(this.Count == 0)
        {
            CoroutineUpdator.Create(null, this.onLoaded);
            return;
        }

        for (int i = 0, imax = this.Count; i < imax; i++)
        {
            if (i < imax - 1)
            {
                this[i].Load();
            }
            else
            {
                this[i].Load(this.onLoaded);
            }
        }
    }
    /// <summary>
    /// アンロード
    /// </summary>
    public void Unload()
    {
        for (int i = 0, imax = this.Count; i < imax; i++)
        {
            this[i].Unload();
        }
    }
    /// <summary>
    /// ハンドル配列への変換
    /// </summary>
    public AssetLoadHandle[] ToAssetLoadHandles()
    {
        return this.Select(item => item.handle).ToArray();
    }
    /// <summary>
    /// 状態取得
    /// </summary>
    public Status GetStatus()
    {
        return this.Count == 0 ? Status.Empty
             : this.Exists(x => x.handle == null) ? Status.NeedLoad
             : this.Exists(x => x.handle.keepWaiting) ? Status.Loading
             : Status.Loaded;
    }
}
