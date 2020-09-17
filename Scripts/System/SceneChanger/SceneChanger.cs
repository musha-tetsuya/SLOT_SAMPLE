using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public static class SceneChanger
{
    /// <summary>
    /// 現在のシーン
    /// </summary>
    private static string currentSceneName = null;
    /// <summary>
    /// 現在のシーン
    /// </summary>
    public static SceneBase currentScene { get; private set; }
    /// <summary>
    /// ロード中かどうか
    /// </summary>
    private static bool IsLoading = false;
    /// <summary>
    /// 自動でロード中表示を消すかどうか
    /// </summary>
    public static bool IsAutoHideLoading = true;
    /// <summary>
    /// シーン遷移アニメーション表示イベント
    /// </summary>
    public static Action<Action> onShowSceneChangeAnimation = null;
    /// <summary>
    /// シーン遷移アニメーション非表示イベント
    /// </summary>
    public static Action<Action> onHideSceneChangeAnimation = null;

    /// <summary>
    /// シーン切り替え
    /// </summary>
    public static void ChangeSceneAsync(string nextSceneName, SceneDataPackBase dataPack = null)
    {
        if (currentSceneName == nextSceneName) return;
        if (IsLoading) return;

        //ロード中フラグON
        IsLoading = true;

        //デフォルトは自動でロード中表示を消す
        IsAutoHideLoading = true;

        if (dataPack == null)
        {
            dataPack = new SceneDataPackBase();
        }
        dataPack.toSceneName = nextSceneName;

        if (string.IsNullOrEmpty(currentSceneName))
        {
            //場面転換アニメなし
            LoadSceneAsync(dataPack);
        }
        else
        {
            dataPack.fromSceneName = currentSceneName;

            //場面転換アニメあり
            ShowSceneChangeAnimation(() =>
            {
                //空シーンに遷移することで現在のシーンをアンロード
                SceneManager.LoadSceneAsync("Empty").completed += (op1) =>
                {
                    //リソースアンロード
                    Resources.UnloadUnusedAssets().completed += (op2) =>
                    {
                        //GC整理
                        GC.Collect();
                        //次のシーンをロード
                        LoadSceneAsync(dataPack);
                    };
                };
            });
        }
    }

    /// <summary>
    /// シーンロード
    /// </summary>
    private static void LoadSceneAsync(SceneDataPackBase dataPack)
    {
        SharedScene.instance.HideHeader();

        //ロード開始
        SceneManager.LoadSceneAsync(dataPack.toSceneName).completed += (op) =>
        {
            //シーン取得
            currentSceneName = dataPack.toSceneName;
            currentScene = SceneManager
                .GetSceneByName(currentSceneName)
                .GetRootGameObjects()
                .Select(g => g.GetComponent<SceneBase>())
                .First(s => s != null);

            if (IsAutoHideLoading)
            {
                //シーン移動アニメーション終了
                HideSceneChangeAnimation();
            }

            //ロード完了通知
            IsLoading = false;
            currentScene.OnSceneLoaded(dataPack);
        };
    }

    /// <summary>
    /// シーン遷移アニメーション表示
    /// </summary>
    public static void ShowSceneChangeAnimation(Action onFinishedIn)
    {
        if (onShowSceneChangeAnimation != null)
        {
            onShowSceneChangeAnimation(onFinishedIn);
        }
        else
        {
            onFinishedIn?.Invoke();
        }
    }

    /// <summary>
    /// シーン遷移アニメーション非表示
    /// </summary>
    public static void HideSceneChangeAnimation(Action onFinished = null)
    {
        onHideSceneChangeAnimation?.Invoke(onFinished);
    }
}
