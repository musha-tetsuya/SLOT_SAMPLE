using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// オフライン時レスポンスデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/OfflineResponseData")]
public class OfflineResponseData : ScriptableObject
{
    /// <summary>
    /// Jsonデータ
    /// </summary>
    [SerializeField, TextArea(1, 50)]
    public string data = null;

#if UNITY_EDITOR
    /// <summary>
    /// オフラインモードのON/OFF切り替え
    /// </summary>
#if SHARK_OFFLINE
    [MenuItem("Tools/Set OfflineMode ON → OFF")]
#else
    [MenuItem("Tools/Set OfflineMode OFF → ON")]
#endif
    private static void SetOfflineMode()
    {
        //現在のDefine
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (defines.Contains("SHARK_OFFLINE"))
        {
            //すでにオフラインモードの場合はnullに置換
            defines = defines.Replace("SHARK_OFFLINE", null);
        }
        else
        {
            //オンラインモードの場合はオフラインを設定
            defines += ";SHARK_OFFLINE";
        }

        //先頭＆末尾の";"を削除する
        if (defines.StartsWith(";"))
        {
            defines = defines.Substring(1, defines.Length - 1);
        }
        if (defines.EndsWith(";"))
        {
            defines = defines.Substring(0, defines.Length - 1);
        }

        //Define設定
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
    }
#endif
}