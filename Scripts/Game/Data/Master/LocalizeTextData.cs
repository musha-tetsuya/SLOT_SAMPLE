using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// ローカライズテキストデータ
    /// </summary>
    public class LocalizeTextData : ModelBase
    {
        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("text"), Localize]
        public string text { get; set; }
    }

    /// <summary>
    /// ローカライズテキストデータベース
    /// </summary>
    public class LocalizeTextDataBase : DataBase<LocalizeTextData>
    {
        /// <summary>
        /// 内蔵データベース
        /// </summary>
        private DataBase<LocalizeTextData> builtinDB = new DataBase<LocalizeTextData>("BuiltinLocalizeTextData");
        /// <summary>
        /// 内蔵データ辞書
        /// </summary>
        private Dictionary<string, string> builtinData = null;
        /// <summary>
        /// 外付けデータ辞書
        /// </summary>
        private Dictionary<string, string> externalData = null;
        /// <summary>
        /// テーブル名
        /// </summary>
        public override string tableName => "mLocalizeText";

        /// <summary>
        /// construct
        /// </summary>
        public LocalizeTextDataBase(string jsonName)
            : base(jsonName)
        {

        }

        /// <summary>
        /// keyでデータ取得
        /// </summary>
        public string Get(string key)
        {
            if (this.builtinData == null)
            {
                var dataList = this.builtinDB.GetList();
                if (dataList != null)
                {
                    this.builtinData = dataList.ToDictionary(
                        keySelector: x => x.key,
                        elementSelector: x => x.text
                    );
                }
            }

            //内蔵データに存在するなら内蔵データから返却
            if (this.builtinData != null && this.builtinData.ContainsKey(key))
            {
                return this.builtinData[key];
            }

            if (this.externalData == null)
            {
                var dataList = this.GetList();
                if (dataList != null)
                {
                    this.externalData = dataList.ToDictionary(
                        keySelector: x => x.key,
                        elementSelector: x => x.text
                    );
                }
            }

            //外付けデータに存在するなら外付けデータから返却
            if (this.externalData != null && this.externalData.ContainsKey(key))
            {
                return this.externalData[key];
            }

#if DEBUG
            //マスターデータ設定するのがめんどい時用の一時的なKeyと文言。あとで全部消す。
            switch (key)
            {
                case "DebugTest": return "てすと";
            }
#endif

            return null;
        }

        /// <summary>
        /// keyでデータ取得
        /// </summary>
        public string GetFormat(string key, params object[] args)
        {
            string format = this.Get(key);
            return string.IsNullOrEmpty(format) ? null : string.Format(format, args);
        }
    }
}