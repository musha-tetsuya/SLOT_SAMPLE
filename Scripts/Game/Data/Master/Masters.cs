using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Master;

/// <summary>
/// マスターデータ管理
/// </summary>
public static class Masters
{
    /// <summary>
    /// ミリ秒を秒にする係数
    /// </summary>
    public const float MilliSecToSecond = 0.001f;
    /// <summary>
    /// パーセントを小数にする係数
    /// </summary>
    public const float PercentToDecimal = 0.01f;

    public static readonly LocalizeTextDataBase LocalizeTextDB = new LocalizeTextDataBase("LocalizeTextData");
 
}
