using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// ユーザーアイテムデータ
/// </summary>
public class UserItemData
{
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    [JsonProperty("itemType")]
    public ItemType itemType;
    /// <summary>
    /// アイテムID
    /// </summary>
    [JsonProperty("itemId")]
    public uint itemId;
    /// <summary>
    /// 所持数
    /// </summary>
    [JsonProperty("itemNum")]
    public uint stockCount;
}
