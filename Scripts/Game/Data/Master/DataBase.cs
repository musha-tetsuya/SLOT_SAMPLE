using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Master
{
    /// <summary>
    /// データモデル基底
    /// </summary>
    public abstract class ModelBase
    {
        [JsonProperty("id")]
        public uint id { get; set; }
    }

    /// <summary>
    /// データベース基底
    /// </summary>
    public abstract class  IDataBase
    {
        /// <summary>
        /// Jsonファイル名
        /// </summary>
        protected string jsonName { get; private set; }
        /// <summary>
        /// テーブル名
        /// </summary>
        public virtual string tableName => this.jsonName;
        /// <summary>
        /// データリストセット
        /// </summary>
        public abstract void SetList(string json);

        /// <summary>
        /// construct
        /// </summary>
        protected IDataBase(string jsonName)
        {
            this.jsonName = jsonName;
        }
    }

    /// <summary>
    /// ローカライズ対象にするAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LocalizeAttribute : PropertyAttribute
    {

    }

    /// <summary>
    /// マスターDB
    /// </summary>
    public class DataBase<T> : IDataBase where T : ModelBase
    {
        /// <summary>
        /// データリスト
        /// </summary>
        private List<T> dataList = null;

        /// <summary>
        /// construct
        /// </summary>
        public DataBase(string jsonName) : base(jsonName){}

        /// <summary>
        /// Jsonからデータリストへ変換
        /// </summary>
        private List<T> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json, JsonHelper.settings);
        }

        /// <summary>
        /// データリスト取得
        /// </summary>
        public List<T> GetList()
        {
//TODO: 今はまだサーバーからDL出来ないのでローカルから読み込む。
            if (this.dataList == null)
            {
                string path = string.Format("Json/{0}", this.jsonName);
                var json = Resources.Load<TextAsset>(path);
                if (json == null)
                {
                    this.dataList = new List<T>();
                }
                else
                {
                    this.SetList(json.text);
                }
            }

            return this.dataList;
        }

        /// <summary>
        /// データリストセット
        /// </summary>
        public override void SetList(string json)
        {
            this.dataList = this.FromJson(json);
            this.Localize(json);
        }

        /// <summary>
        /// ローカライズ
        /// </summary>
        private void Localize(string json)
        {
            var language = UserData.GetLanguage();
            var type =typeof(T);

            //ローカライズが必要なメンバー
            var localizeTargetMembers = type
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetCustomAttribute<LocalizeAttribute>() != null)
                .ToArray();

            if (localizeTargetMembers == null || localizeTargetMembers.Length == 0)
            {
                //ローカライズが必要なメンバーがいないのでreturn
                return;
            }

            //keyとobjectにしたJsonデータ
            var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json, JsonHelper.settings);
            if (jsonData.Count == 0)
            {
                Debug.LogErrorFormat("{0} is empty.", this.GetType());
                return;
            }

            for (int i = 0; i < localizeTargetMembers.Length; i++)
            {
                //メンバー名
                string name = localizeTargetMembers[i].Name;
                var jsonProperty = localizeTargetMembers[i].GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonProperty != null)
                {
                    //JsonProperty名があるなら優先
                    name = jsonProperty.PropertyName;
                }

                string key = name;
                if (language != Language.Ja)
                {
                    key += language;
                }

                //ローカライズデータが存在するなら
                if (jsonData[0].ContainsKey(key))
                {
                    Action<object, object> setValue = null;

                    if (localizeTargetMembers[i] is FieldInfo)
                    {
                        setValue = (localizeTargetMembers[i] as FieldInfo).SetValue;
                    }
                    else if (localizeTargetMembers[i] is PropertyInfo)
                    {
                        setValue = (localizeTargetMembers[i] as PropertyInfo).SetValue;
                    }

                    if (setValue != null)
                    {
                        for (int j = 0; j < this.dataList.Count; j++)
                        {
                            //ローカライズ
                            string value = (string)jsonData[j][key];
                            setValue(this.dataList[j], value.Replace("\\n", "\n"));
                        }
                    }
                }
                else
                {
                    Debug.LogWarningFormat("{0}「{1}」のローカライズデータ「{2}」が存在しません。", this.jsonName, name, key);
                }
            }
        }

        /// <summary>
        /// IDで検索
        /// </summary>
        public T FindById(uint id)
        {
            return this.GetList().Find(x => x.id == id);
        }
    }
}
