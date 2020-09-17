using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マスターデータ取得API.
/// </summary>
public class FirstApi
{
    /// <summary>
    /// first/userのレスポンスデータ
    /// </summary>
    public class FirstUserResponseData
    {
        public TUsers tUsers;
        public List<UserItemData> tItem;
    }

    /// <summary>
    /// ユーザーデータ取得通信 (Login時のみ)
    /// </summary>
    public static void CallFirstUserApi(UserData userData, Action onCompleted)
    {
        //リクエスト作成
        var request = new SlotWebRequest<FirstUserResponseData>("first/user");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //通信で取得したデータを格納
            userData.Set(response);

            //通信完了
            onCompleted?.Invoke();
        };

        //通信開始
        request.Send();
    }
}
