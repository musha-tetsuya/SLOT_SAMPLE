using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// アセットロードハンドル
/// </summary>
public abstract class AssetLoadHandle : CustomYieldInstruction
{
    /// <summary>
    /// ステータス
    /// </summary>
    public enum Status
    {
        None,           //未処理
        Loading,        //ロード中
        StopProcessing, //ロード停止処理中
        Stop,           //ロード停止中
        Completed,      //ロード済
    }

    /// <summary>
    /// エラーステータス
    /// </summary>
    public enum ErrorStatus
    {
        /// <summary>
        /// なし
        /// </summary>
        None,
        /// <summary>
        /// ファイルが無い
        /// </summary>
        /// <para>
        /// ファイルをDLすればリトライ可能
        /// </para>
        FileNotFound,
        /// <summary>
        /// アセットバンドル内に指定のアセットが内包されていない
        /// </summary>
        /// <para>
        /// ファイルを再DLすればリトライ可能かもしれないが、既に同じアセットバンドルからロードされているアセットがある場合、
        /// そのアセットにもろに影響が出るのでタイトルからやり直すのが無難
        /// </para>
        AssetNotContains,
        /// <summary>
        /// 何らかの原因でアセットがnull
        /// </summary>
        /// <para>
        /// リトライすれば解消するかもしれないが、ダメだったらタイトルに戻るしかない
        /// </para>
        AssetIsNull,
    }

    /// <summary>
    /// 破棄不可フラグ
    /// </summary>
    public bool isDontDestroy = false;
    /// <summary>
    /// 参照カウンタ
    /// </summary>
    public int referenceCount = 0;
    /// <summary>
    /// パス
    /// </summary>
    public string path { get; private set; }
    /// <summary>
    /// アセットの型
    /// </summary>
    public Type type { get; private set; }
    /// <summary>
    /// ステータス
    /// </summary>
    public Status status { get; private set; }
    /// <summary>
    /// エラーステータス
    /// </summary>
    public ErrorStatus errorStatus { get; private set; }
    /// <summary>
    /// リクエスト
    /// </summary>
    public AsyncOperation request { get; protected set; }
    /// <summary>
    /// アセット
    /// </summary>
    public abstract UnityEngine.Object asset { get; }
    /// <summary>
    /// ロード完了時コールバック
    /// </summary>
    private Action onLoaded = null;
    /// <summary>
    /// エラー時コールバック
    /// </summary>
    private Action<AssetLoadHandle> onError = null;
    /// <summary>
    /// 一時停止時コールバック
    /// </summary>
    private Action onStop = null;
    /// <summary>
    /// 再ロード時コールバック
    /// </summary>
    private Action onRestart = null;

    /// <summary>
    /// construct
    /// </summary>
    protected AssetLoadHandle(string path, Type type, Action onLoaded, Action<AssetLoadHandle> onError)
    {
        this.path = path;
        this.type = type;
        this.onLoaded = onLoaded;
        this.onError = onError;
    }

    /// <summary>
    /// ロード開始
    /// </summary>
    public virtual void LoadStart()
    {
        this.status = Status.Loading;
    }

    /// <summary>
    /// エラー時
    /// </summary>
    protected void OnError(ErrorStatus errorStatus, Action onRetry)
    {
        Debug.LogErrorFormat("エラー発生：{0}：{1}", errorStatus, this.path);
        this.status = Status.Stop;
        this.errorStatus = errorStatus;
        this.onRestart = onRetry;
        this.onError?.Invoke(this);
    }

    /// <summary>
    /// ロード停止命令（すぐには止まらない。止まったらコールバックで通知する）
    /// </summary>
    public void Stop(Action onStop)
    {
        if (this.status == Status.Loading)
        {
            this.status = Status.StopProcessing;
            this.onStop = onStop;
        }
    }

    /// <summary>
    /// ロード停止時
    /// </summary>
    protected void OnStop(Action onRestart)
    {
        Debug.LogFormat("ロード停止：{0}", this.path);
        this.status = Status.Stop;
        this.onRestart = onRestart;
        this.onStop?.Invoke();
    }

    /// <summary>
    /// ロード再開
    /// </summary>
    public void Restart()
    {
        if (this.status == Status.Stop)
        {
            Debug.LogFormat("ロード再開：{0}", this.path);
            this.status = Status.Loading;
            this.errorStatus = ErrorStatus.None;
            this.onRestart?.Invoke();
        }
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    protected virtual void OnLoaded()
    {
        this.status = Status.Completed;
        this.onLoaded?.Invoke();
    }

    /// <summary>
    /// ロード完了したかどうか
    /// </summary>
    public override bool keepWaiting
    {
        get { return this.status != Status.Completed; }
    }

    /// <summary>
    /// 破棄可能かどうか
    /// </summary>
    public bool IsUnloadable()
    {
        return !this.isDontDestroy
            && this.referenceCount <= 0
            && this.status != Status.Loading
            && this.status != Status.StopProcessing;
    }

    /// <summary>
    /// 破棄
    /// </summary>
    public virtual void Unload()
    {
        this.path = null;
        this.status = Status.None;
        this.errorStatus = ErrorStatus.None;
        this.request = null;
        this.onLoaded = null;
        this.onError = null;
        this.onStop = null;
        this.onRestart = null;
    }
}

/// <summary>
/// リソースロードハンドル
/// </summary>
public class ResourcesLoadHandle : AssetLoadHandle
{
    /// <summary>
    /// construct
    /// </summary>
    public ResourcesLoadHandle(string path, Type type, Action onLoaded, Action<AssetLoadHandle> onError)
        : base(path, type, onLoaded, onError)
    {
    }

    /// <summary>
    /// ロード開始
    /// </summary>
    public override void LoadStart()
    {
        base.LoadStart();
        this.request = Resources.LoadAsync(this.path, this.type);
        this.request.completed += (_) => this.OnLoaded();
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    protected override void OnLoaded()
    {
        //停止命令が来ていたら
        if (this.status == Status.StopProcessing)
        {
            //停止したことを通知
            this.OnStop(onRestart: () => this.OnLoaded());
            return;
        }

        if ((this.request as ResourceRequest).asset == null)
        {
            //エラー：アセットがnull
            this.OnError(ErrorStatus.AssetIsNull, onRetry: () => this.LoadStart());
            return;
        }

        //ロード完了を通知
        base.OnLoaded();
    }

    /// <summary>
    /// アセット
    /// </summary>
    public override UnityEngine.Object asset
    {
        get { return this.keepWaiting ? null : (this.request as ResourceRequest).asset; }
    }
}

/// <summary>
/// アセットバンドルロードハンドル
/// </summary>
public class AssetBundleLoadHandle : AssetLoadHandle
{
    /// <summary>
    /// アセットバンドル情報
    /// </summary>
    private AssetBundleInfo info = null;

    /// <summary>
    /// サブアセットかどうか
    /// </summary>
    private bool isSubAsset = false;

    /// <summary>
    /// MonoBehaviourを継承している型かどうか
    /// </summary>
    private bool isMonoBehaviour = false;

    /// <summary>
    /// アセット
    /// </summary>
    private UnityEngine.Object m_asset = null;

    /// <summary>
    /// construct
    /// </summary>
    public AssetBundleLoadHandle(
        string path,
        AssetBundleInfo info,
        bool isSubAsset,
        Type type,
        Action onLoaded,
        Action<AssetLoadHandle> onError)
        : base(path, type, onLoaded, onError)
    {
        this.info = info;
        this.isSubAsset = isSubAsset;
        this.isMonoBehaviour = this.type.IsSubclassOf(typeof(MonoBehaviour));

        //メインのアセットバンドルに参照ユーザー登録
        var item = ReferencedAssetBundle.Get(this.info.assetBundleName);
        item.AddReferenceUser(this);

        //依存関係のアセットバンドルに参照ユーザー登録
        foreach (var dependency in this.info.dependencies)
        {
            item = ReferencedAssetBundle.Get(dependency);
            item.AddReferenceUser(this);
        }
    }

    /// <summary>
    /// ロード開始
    /// </summary>
    public override void LoadStart()
    {
        base.LoadStart();
        this.LoadAssetBundle(0);
    }

    /// <summary>
    /// アセットバンドルのロード
    /// </summary>
    private void LoadAssetBundle(int i)
    {
        //停止命令が来ていたら
        if (this.status == Status.StopProcessing)
        {
            //停止したことを通知
            this.OnStop(onRestart: () => this.LoadAssetBundle(i));
            return;
        }

        bool isLast = (i == this.info.dependencies.Length);

        //ロードするアセットバンドル名（最後にメインのアセットバンドルをロードする）
        string assetBundleName = isLast ? this.info.assetBundleName : this.info.dependencies[i];

        var item = ReferencedAssetBundle.Get(assetBundleName);

        try
        {
            //ロード実行
            item.Load(assetBundleName);
        }
        catch
        {
            //エラー：ファイルが無い
            this.OnError(ErrorStatus.FileNotFound, onRetry: () => this.LoadAssetBundle(i));
            return;
        }

        //既にロード済みだった場合
        if (item.request.isDone)
        {
            if (isLast)
            {
                //メインのアセットバンドルロード完了後にアセットロードする
                this.LoadAsset(item.request);
            }
            else
            {
                //次のアセットバンドルのロードを開始
                this.LoadAssetBundle(i + 1);
            }
        }
        //まだロード完了してない場合
        else
        {
            if (isLast)
            {
                //メインのアセットバンドルロード完了後にアセットロードする
                item.request.completed += this.LoadAsset;
            }
            else
            {
                //完了後に次のアセットバンドルのロードを開始
                item.request.completed += (_) => this.LoadAssetBundle(i + 1);
            }
        }
    }

    /// <summary>
    /// アセットのロード
    /// </summary>
    private void LoadAsset(AsyncOperation operation)
    {
        //停止命令が来ていたら
        if (this.status == Status.StopProcessing)
        {
            //停止したことを通知
            this.OnStop(onRestart: () => this.LoadAsset(operation));
            return;
        }

        var assetBundle = (operation as AssetBundleCreateRequest).assetBundle;
        string assetName = Path.GetFileNameWithoutExtension(this.path);

        if (!assetBundle.Contains(assetName))
        {
            //エラー：指定のアセットが内包されていない
            this.OnError(ErrorStatus.AssetNotContains, onRetry: () => this.LoadAsset(operation));
            return;
        }

        //MonoBehaviourを継承しているアセットはGameObject型でロードする
        var assetType = this.isMonoBehaviour ? typeof(GameObject) : this.type;

        //アセットのロードを開始
        this.request = this.isSubAsset
            ? assetBundle.LoadAssetWithSubAssetsAsync(assetName, assetType)
            : assetBundle.LoadAssetAsync(assetName, assetType);
        this.request.completed += (_) => this.OnLoaded();
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    protected override void OnLoaded()
    {
        //停止命令が来ていたら
        if (this.status == Status.StopProcessing)
        {
            //停止したことを通知
            this.OnStop(onRestart: () => this.OnLoaded());
            return;
        }

        if ((this.request as AssetBundleRequest).asset == null)
        {
            //エラー：アセットがnull
            var item = ReferencedAssetBundle.Get(this.info.assetBundleName);
            this.OnError(ErrorStatus.AssetIsNull, onRetry: () => this.LoadAsset(item.request));
            return;
        }

        //ロード完了を通知
        base.OnLoaded();
    }

    /// <summary>
    /// アセット
    /// </summary>
    public override UnityEngine.Object asset
    {
        get
        {
            if (this.keepWaiting)
            {
                return null;
            }
            
            if (this.m_asset == null)
            {
                this.m_asset = (this.request as AssetBundleRequest).asset;

                if (this.isMonoBehaviour)
                {
                    this.m_asset = (this.m_asset as GameObject).GetComponent(this.type);
                }
            }

            return this.m_asset;
        }
    }

    /// <summary>
    /// 破棄
    /// </summary>
    public override void Unload()
    {
        //アセットバンドルへの参照を解除
        if (this.info != null)
        {
            var item = ReferencedAssetBundle.Get(this.info.assetBundleName);
            item.Unload(this);

            foreach (var dependency in this.info.dependencies)
            {
                item = ReferencedAssetBundle.Get(dependency);
                item.Unload(this);
            }

            this.info = null;
        }

        base.Unload();
    }

    /// <summary>
    /// 参照されるアセットバンドル
    /// </summary>
    private class ReferencedAssetBundle
    {
        /// <summary>
        /// ロード済みorロード中のアセットバンドル一覧
        /// </summary>
        private static List<ReferencedAssetBundle> assetBundles = new List<ReferencedAssetBundle>();

        /// <summary>
        /// アセットバンドル名
        /// </summary>
        private string assetBundleName = null;

        /// <summary>
        /// リクエスト
        /// </summary>
        public AssetBundleCreateRequest request { get; private set; }

        /// <summary>
        /// 自身を参照しているハンドル
        /// </summary>
        private List<AssetBundleLoadHandle> referenceUsers = new List<AssetBundleLoadHandle>();

        /// <summary>
        /// アセットバンドルの取得
        /// </summary>
        public static ReferencedAssetBundle Get(string assetBundleName)
        {
            var item = assetBundles.Find(x => x.assetBundleName == assetBundleName);

            if (item == null)
            {
                item = new ReferencedAssetBundle();
                item.assetBundleName = assetBundleName;
                assetBundles.Add(item);
            }

            return item;
        }

        /// <summary>
        /// 参照ユーザーの追加
        /// </summary>
        public void AddReferenceUser(AssetBundleLoadHandle user)
        {
            if (!this.referenceUsers.Contains(user))
            {
                this.referenceUsers.Add(user);
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public void Load(string assetBundleName)
        {
            if (this.request == null)
            {
                string path = Path.Combine(AssetManager.GetAssetBundleDirectoryPath(), assetBundleName.GetHashString());

                if (!File.Exists(path))
                {
                    throw new Exception();
                }

                this.request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path).Cryption());
            }
        }

        /// <summary>
        /// 破棄
        /// </summary>
        public void Unload(AssetBundleLoadHandle user)
        {
            //参照ユーザーの解除
            this.referenceUsers.Remove(user);

            //参照ユーザーがいなくなったら
            if (this.referenceUsers.Count == 0)
            {
                //アセットバンドルがロード済みならアセットバンドルを破棄
                if (this.request != null && this.request.isDone)
                {
                    this.request.assetBundle.Unload(true);
                    this.request = null;
                }

                //自身をリストから除去
                assetBundles.Remove(this);
            }
        }
    }
}