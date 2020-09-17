using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public static class JsonHelper
{
    /// <summary>
    /// Newtonsoft.Json.JsonConvert.DeserializeObjectするときの設定（よく分かんない。コピペ）
    /// </summary>
    public static readonly JsonSerializerSettings settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };

    public static List<T> ListFromJson<T>(string json)
    {
        var newJson = "{ \"list\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.list;
    }

    [Serializable]
    class Wrapper<T>
    {
        public List<T> list = null;
    }
}