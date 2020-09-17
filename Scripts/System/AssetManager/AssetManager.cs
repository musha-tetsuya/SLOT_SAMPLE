//#define ASSETBUNDLE_SIMULATION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// リソース管理クラス
/// </summary>
public class AssetManager : SingletonMonoBehaviour<AssetManager>
{
    /// <summary>
    /// 最大同時処理数
    /// </summary>
    private const int THREAD_MAX = 5;

    /// <summary>
    /// アセットバンドル情報一覧
    /// </summary>
    private static List<AssetBundleInfo> assetBundleInfoList = null;
    /// <summary>
    /// 読み込んだor読み込み中のハンドルリスト
    /// </summary>
    public static List<AssetLoadHandle> handles = new List<AssetLoadHandle>();
    /// <summary>
    /// 積まれているコールバック一覧
    /// </summary>
    private static List<Tuple<AssetLoadHandle, Action>> callbacks = new List<Tuple<AssetLoadHandle, Action>>();
    /// <summary>
    /// エラーしたハンドル
    /// </summary>
    private static AssetLoadHandle errorHandle = null;
    /// <summary>
    /// エラー時コールバック
    /// </summary>
    public static Action<AssetLoadHandle> onError = null;

    /// <summary>
    /// クリア（タイトル戻るときとか）
    /// </summary>
    public static void Clear()
    {
        //アセットバンドル情報一覧のクリア
        if (assetBundleInfoList != null)
        {
            assetBundleInfoList.Clear();
            assetBundleInfoList = null;
        }

        //エラーハンドル解放
        errorHandle = null;
    }

    /// <summary>
    /// サーバーから受け取ったアセットバンドル情報をセット
    /// </summary>
    public static void SetAssetBundleInfoList(List<AssetBundleInfo> infoList)
    {
        assetBundleInfoList = infoList;
    }

    /// <summary>
    /// アセットバンドル情報一覧取得
    /// </summary>
    private static List<AssetBundleInfo> GetAssetBundleInfoList()
    {
        if (assetBundleInfoList == null)
        {
#if UNITY_EDITOR && ASSETBUNDLE_SIMULATION
            string assetInfoListPath = GetAssetBundleInfoListPath();
            if (File.Exists(assetInfoListPath))
            {
                SetAssetBundleInfoList(File.ReadAllText(assetInfoListPath));
            }
            else
#endif
            {
                assetBundleInfoList = new List<AssetBundleInfo>();
            }
        }
        return assetBundleInfoList;
    }

    /// <summary>
    /// アセットバンドル情報の検索
    /// </summary>
    private static AssetBundleInfo FindAssetBundleInfo(string path, out bool isSubAsset)
    {
        var infoList = GetAssetBundleInfoList();
        string lowerPath = path.ToLower();

        for (int i = 0; i < infoList.Count; i++)
        {
            //パスとアセットバンドル名が一致してるなら
            if (string.Equals(path, infoList[i].assetBundleName, StringComparison.OrdinalIgnoreCase))
            {
                isSubAsset = false;
                return infoList[i];
            }
        }

        for (int i = 0; i < infoList.Count; i++)
        {
            //パスがアセットバンドル名を含んでいるなら
            if (lowerPath.Contains(infoList[i].assetBundleName))
            {
                isSubAsset = true;
                return infoList[i];
            }
        }

        isSubAsset = false;
        return null;
    }

    /// <summary>
    /// アンロード
    /// </summary>
    public static bool Unload(AssetLoadHandle handle)
    {
        if (handles.Contains(handle))
        {
            //参照カウンタ減少
            handle.referenceCount--;

            if (handle.IsUnloadable())
            {
                handle.Unload();
                handles.Remove(handle);
                callbacks.RemoveAll(x => x.Item1 == handle);
                return true;
            }

            //Debug.LogWarningFormat("破棄不可：{0}, status={1}, isDontDestroy={2}, referenceCount={3}", handle.path, handle.status, handle.isDontDestroy, handle.referenceCount);
        }
        return false;
    }

    /// <summary>
    /// 全てアンロード
    /// </summary>
    public static void UnloadAll(bool isForce = false)
    {
        for (int i = 0; i < handles.Count; i++)
        {
            if (isForce)
            {
                handles[i].isDontDestroy = false;
                handles[i].referenceCount = 0;
            }

            if (Unload(handles[i]))
            {
                i--;
            }
        }
    }

    /// <summary>
    /// キャンセル
    /// </summary>
    /// <para>
    /// キャンセルとは名ばかりで、コールバックは呼ばれなくなるがロードは完了まで行われる。
    /// そのため、キャンセルしたハンドルもちゃんとUnloadをしてやらないとメモリに残り続けてしまう。
    /// シーン遷移時とかにAssetManagerのロードが走っていないことを確認しつつUnloadAllで全消しする処理が必要。
    /// </para>
    public static void CancelHandle(AssetLoadHandle handle)
    {
        callbacks.RemoveAll(x => x.Item1 == handle);
    }

    /// <summary>
    /// ロード中のハンドルがあるかどうか
    /// </summary>
    public static bool IsLoading()
    {
        return handles.Exists(x => x.keepWaiting);
    }

    /// <summary>
    /// ロード済みorロード中のハンドルを検索
    /// </summary>
    public static AssetLoadHandle FindHandle<T>(string path) where T : UnityEngine.Object
    {
        var type = typeof(T);
        return handles.Find(x => x.path.Equals(path, StringComparison.OrdinalIgnoreCase) && (x.type == type || x.type.IsSubclassOf(type)));
    }

    /// <summary>
    /// ロード
    /// </summary>
    public static AssetLoadHandle Load<T>(string path, Action<T> onLoaded = null) where T : UnityEngine.Object
    {
        //既に読み込みがかかっているか検索
        AssetLoadHandle handle = FindHandle<T>(path);

        //ハンドルが既存だった場合
        if (handle != null)
        {
            //参照カウンタ増加
            handle.referenceCount++;

            //コールバックを積む
            callbacks.Add(new Tuple<AssetLoadHandle, Action>(handle, () =>
            {
                if (onLoaded != null)
                {
                    onLoaded((T)handle.asset);
                }
            }));

            //ハンドルがロード済みなら積まれているコールバックを順に消化
            if (!handle.keepWaiting)
            {
                GetInstance().StartCoroutine(InvokeCallbackNextFrame());//1フレ後に処理したい
            }
        }
        //ハンドルが存在しなかったら
        else
        {
            bool isSubAsset = false;
            var info = FindAssetBundleInfo(path, out isSubAsset);

            //AssetBundleの場合
            if (info != null)
            {
                //ハンドル作成 → ロード完了したら積まれているコールバックを順に消化
                handle = new AssetBundleLoadHandle(path, info, isSubAsset, typeof(T), InvokeCallback, OnError);
            }
            //Resourcesの場合
            else
            {
                //ハンドル作成 → ロード完了したら積まれているコールバックを順に消化
                handle = new ResourcesLoadHandle(path, typeof(T), InvokeCallback, OnError);
            }

            //参照カウンタ増加
            handle.referenceCount++;

            handles.Add(handle);

            //コールバックを積む
            callbacks.Add(new Tuple<AssetLoadHandle, Action>(handle, () =>
            {
                if (onLoaded != null)
                {
                    onLoaded((T)handle.asset);
                }
            }));

            //ロード開始
            LoadStartIfCan();
        }

        return handle;
    }

    /// <summary>
    /// 余裕があるならロードを開始する
    /// </summary>
    private static void LoadStartIfCan()
    {
        //エラーしているハンドルがあるときは受け付けない
        if (errorHandle != null)
        {
            return;
        }

        //ロード中のハンドル数
        int loadingCount = handles.Count(x => x.status == AssetLoadHandle.Status.Loading);

        //スレッドに余裕があるなら
        if (loadingCount < THREAD_MAX)
        {
            //未処理ハンドルのロードを開始
            var handle = handles.Find(x => x.status == AssetLoadHandle.Status.None);
            if (handle != null)
            {
                handle.LoadStart();
            }
        }
    }

    /// <summary>
    /// 積まれているコールバックの消化
    /// </summary>
    private static void InvokeCallback()
    {
        if (errorHandle != null || callbacks.Count == 0)
        {
            return;
        }

        //スレッドに余裕があるなら未処理ハンドルのロードを開始
        LoadStartIfCan();

        //先頭に積まれているコールバックに対応するハンドルがロード済みなら
        if (!callbacks[0].Item1.keepWaiting)
        {
            //コールバック実行
            callbacks[0].Item2.Invoke();
            //実行したコールバックをリストから除去
            callbacks.RemoveAt(0);
            //再帰で次のコールバックを消化
            InvokeCallback();
        }
    }

    /// <summary>
    /// 次のフレームでコールバックを消化する
    /// </summary>
    private static IEnumerator InvokeCallbackNextFrame()
    {
        yield return null;
        InvokeCallback();
    }

    /// <summary>
    /// ハンドルエラー時
    /// </summary>
    private static void OnError(AssetLoadHandle handle)
    {
        if (errorHandle == null)
        {
            errorHandle = handle;

            for (int i = 0; i < handles.Count; i++)
            {
                //ロード中のハンドルを一時停止させる
                handles[i].Stop(ErrorNotificationIfCan);
            }

            //可能ならエラー通知
            ErrorNotificationIfCan();
        }
    }

    /// <summary>
    /// 可能ならエラー通知
    /// </summary>
    private static void ErrorNotificationIfCan()
    {
        //処理中のハンドルが残っているかどうか
        bool isBusy = handles.Exists(x =>
        {
            return x.status == AssetLoadHandle.Status.Loading
                || x.status == AssetLoadHandle.Status.StopProcessing;
        });

        //処理中のハンドルが無くなったらエラー通知
        if (!isBusy)
        {
            Debug.LogFormat("エラー通知：{0}：{1}", errorHandle.errorStatus, errorHandle.path);
            onError?.Invoke(errorHandle);
        }
    }

    /// <summary>
    /// リトライ
    /// </summary>
    public static void Retry()
    {
        if (errorHandle != null)
        {
            //エラーしたハンドルのリトライ
            var tmpErrorHandle = errorHandle;
            errorHandle = null;
            tmpErrorHandle.Restart();

            //Restartしたハンドルがエラーした場合、OnErrorが呼ばれてerrorHandleに値が入るので
            //errorHandleがnullじゃなくなった時点でRestart処理を中止する
            for (int i = 0; i < handles.Count && errorHandle == null; i++)
            {
                handles[i].Restart();
            }

            //スレッドに余裕があるなら未処理ハンドルのロードを開始
            for (int i = 0; i < THREAD_MAX && errorHandle == null; i++)
            {
                LoadStartIfCan();
            }
        }
    }

    /// <summary>
    /// アセットバンドル保存先ディレクトリ名取得
    /// </summary>
    public static string GetAssetBundleDirectoryPath()
    {
#if UNITY_EDITOR
        return Application.dataPath.Replace("Assets", "DownloadFiles");
#else
        return Application.persistentDataPath + "/DownloadFiles";
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// アセットバンドルリソースディレクトリパス
    /// </summary>
    public const string AssetBundleResourcesDirectory = "Assets/Sunchoi/AssetbundleResources/Resources";

    /// <summary>
    /// アセットバンドル名を付与する
    /// </summary>
    /// <para>editor only</para>
    [MenuItem("Assets/Sunchoi/Set AssetBundleName")]
    private static void SetAssetBundleName()
    {
        foreach (int instanceId in Selection.instanceIDs)
        {
            //選択しているアセットのパス
            string assetPath = AssetDatabase.GetAssetPath(instanceId);

            //選択しているアセットがアセットバンドル用のアセットなら
            if (assetPath.Contains(AssetBundleResourcesDirectory))
            {
                //アセットバンドル名を設定（基本的には基礎ディレクトリからの相対パス）
                var importer = AssetImporter.GetAtPath(assetPath);

                var assetBundleName = assetPath.Replace(AssetBundleResourcesDirectory + "/", null);

                var extension = Path.GetExtension(assetBundleName);

                if (!string.IsNullOrEmpty(extension))
                {
                    assetBundleName = assetBundleName.Replace(extension, null);
                }

                importer.assetBundleName = assetBundleName;
            }
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
    }
#endif
}