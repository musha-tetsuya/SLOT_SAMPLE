using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// アセットバンドル情報
/// </summary>
public class AssetBundleInfo
{
    [JsonProperty("assetBundleName")]
    public string assetBundleName { get; set; }

    [JsonProperty("crc")]
    public uint crc { get; set; }

    [JsonProperty("dependencies")]
    public string[] dependencies { get; set; }

    [JsonProperty("fileSize")]
    public long fileSize { get; set; }
}
