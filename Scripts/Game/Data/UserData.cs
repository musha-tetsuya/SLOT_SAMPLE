using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ユーザーデータ
/// </summary>
public class UserData
{
    private static UserData instance = null;

    /// <summary>
    /// ユーザーID
    /// </summary>
    public int userId;
    /// <summary>
    /// ハッシュ
    /// </summary>
    public string hash;
    /// <summary>
    /// パスワード
    /// </summary>
    public string password;
    /// <summary>
    /// 名前
    /// </summary>
    public string name;
    /// <summary>
    /// レベル
    /// </summary>
    public uint lv;
    /// <summary>
    /// EXP
    /// </summary>
    public uint exp;
    /// <summary>
    /// コイン
    /// </summary>
    public ulong coin;
    /// <summary>
    /// 無償ジェム
    /// </summary>
    public ulong freeGem;
    /// <summary>
    /// 有償ジェム
    /// </summary>
    public ulong chargeGem;
    /// <summary>
    /// FVポイント
    /// </summary>
    public int fvPoint;
    /// <summary>
    /// VIPレベル
    /// </summary>
    public uint vipLevel;
    /// <summary>
    /// Vipレベル経験値
    /// </summary>
    public uint vipExp;
    /// <summary>
    /// 所持アイテムデータ
    /// </summary>
    public List<UserItemData> itemData;

    /// <summary>
    /// データ取得
    /// </summary>
    public static UserData Get()
    {
        return instance;
    }

    /// <summary>
    /// データ設定：ログイン時に
    /// </summary>
    public static void Set(UserData userData)
    {
        instance = userData;
    }

    /// <summary>
    /// ログインに必要な情報の端末への保存
    /// </summary>
    public void Save()
    {
#if !SHARK_OFFLINE
        PlayerPrefs.SetInt("userId", this.userId);
        PlayerPrefs.SetString("hash", this.hash);
        PlayerPrefs.SetString("password", this.password);
#endif
    }

    /// <summary>
    /// ログインに必要な情報の端末からの読込
    /// </summary>
    public void Load()
    {
        if (!PlayerPrefs.HasKey("userId"))   return;
        if (!PlayerPrefs.HasKey("hash"))     return;
        if (!PlayerPrefs.HasKey("password")) return;

        this.userId = PlayerPrefs.GetInt("userId");
        this.hash = PlayerPrefs.GetString("hash");
        this.password = PlayerPrefs.GetString("password");
    }

#if UNITY_EDITOR
    /// <summary>
    /// ユーザーデータ削除
    /// </summary>
    [UnityEditor.MenuItem("Tools/Delete UserData")]
    private static void DeleteUserData()
    {
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("hash");
        PlayerPrefs.DeleteKey("password");
    }
#endif

    /// <summary>
    /// サーバーの情報で更新
    /// </summary>
    public void Set(TUsers tUsers)
    {
        this.name = tUsers.userName;
        this.lv = tUsers.level;
        this.exp = tUsers.exp;
        this.coin = tUsers.coin;
    }

    /// <summary>
    /// ログイン時のuserIDとhashをサーバーの情報で更新
    /// </summary>
    public void Set(LoginApi.LoginUserData loginUserData)
    {
        this.userId = loginUserData.userId;
        this.hash = loginUserData.hash;
    }

    /// <summary>
    /// ログイン時のユーザーデータを更新
    /// </summary>
    public void Set(FirstApi.FirstUserResponseData firstUserData)
    {
        Set(firstUserData.tUsers);
        itemData = firstUserData.tItem;
    }

    /// <summary>
    /// アイテム付与
    /// </summary>
    public void AddItem(ItemType itemType, uint itemId, ulong amount)
    {
        switch (itemType)
        {
            case ItemType.Coin:
                this.coin += amount;
                break;
            default:
                Debug.LogErrorFormat("未対応のアイテムタイプ: {0}", itemType);
                break;
        }
    }

    /// <summary>
    /// アイテムセット
    /// </summary>
    public void SetItem(UserItemData tItem)
    {
        var item = this.itemData.Find(x => x.itemId == tItem.itemId);
        if (item == null)
        {
            this.itemData.Add(tItem);
        }
        else
        {
            item.stockCount = tItem.stockCount;
        }
    }

    /// <summary>
    /// 言語取得
    /// </summary>
    public static Language GetLanguage()
    {
#if LANGUAGE_ZH
        Language language = Language.Zh;
#elif LANGUAGE_TW
        Language language = Language.Tw;
#elif LANGUAGE_EN
        Language language = Language.En;
#else
        Language language = Language.Ja;
#endif
        if (PlayerPrefs.HasKey("language"))
        {
            language = (Language)PlayerPrefs.GetInt("language");
        }
        else
        {
            PlayerPrefs.SetInt("language", (int)language);
        }

        return language;
    }

    /// <summary>
    /// BGM音量
    /// </summary>
    public static int bgmVolume
    {
        get => PlayerPrefs.GetInt("bgmValue", 4);
        set => PlayerPrefs.SetInt("bgmValue", value);
    }

    /// <summary>
    /// SE音量
    /// </summary>
    public static int seVolume
    {
        get => PlayerPrefs.GetInt("seValue", 4);
        set => PlayerPrefs.SetInt("seValue", value);
    }
}
