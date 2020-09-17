using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユーザー系API
/// </summary>
public class UserApi
{
    /// <summary>
    /// user/createのレスポンスデータ
    /// </summary>
    public class CreateResponseData
    {
        public TUsers tUsers;
        public string password;
        public UserData userData;
    }

    /// <summary>
    /// user/findのレスポンスデータ
    /// </summary>
    public class FindResponseData
    {
        public TUsers tUsers;
    }

    /// <summary>
    /// user/editのレスポンスデータ
    /// </summary>
    public class ChangeUserNameResponseData
    {
        public TUsers tUsers;
    }

    /// <summary>
    /// user/createCodeのレスポンスデータ
    /// </summary>
    public class TTakeOver
    {
        public string takeOverId;
        public string expiration;
        public uint usedFlg;
    }

    /// <summary>
    /// user/createCodeのレスポンスデータ
    /// </summary>
    public class CreateDeviceChangeCodeResponseData
    {
        public TTakeOver tTakeOver;
        public string takeOverPassword;
    }

    /// <summary>
    /// user/takeOverのレスポンスデータ
    /// </summary>
    public class TUsersLogin
    {
        public int userId;
    }

    /// <summary>
    /// user/takeOverのレスポンスデータ
    /// </summary>
    public class DeviceChangeCodeResponseData
    {
        public TUsersLogin tUsersLogin;
        public string password;
    }

    /// <summary>
    /// ユーザー作成通信
    /// </summary>
    public static void CallCreateApi(string userName, Action onCompleted)
    {
        //リクエスト作成
        var request = new SlotWebRequest<CreateResponseData>("user/create");

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "userName", userName },
            { "authType", 3 },
            { "deviceType", SlotDefine.DEVICE_TYPE },
        });

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //id,hash,passwordが揃った段階で一旦端末に保存する
            response.userData.password = response.password;
            response.userData.Save();

            //ユーザーデータセット
            response.userData.Set(response.tUsers);
            UserData.Set(response.userData);

            //通信完了
            onCompleted?.Invoke();
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// ユーザー情報取得通信
    /// </summary>
    public static void CallFindApi(UserData userData, Action onCompleted)
    {
        var request = new SlotWebRequest<FindResponseData>("user/find");

        request.SetRequestHeader("AccessToken", userData.hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "userId", userData.userId }
        });

        request.onSuccess = (response) =>
        {
            //ユーザーデータセット
            userData.Set(response.tUsers);
            UserData.Set(userData);

            //通信完了
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// ユーザー名変更通信
    /// </summary>
    public static void CallChangeUserNameApi(string changeuserName, Action onCompleted)
    {
        var request = new SlotWebRequest<ChangeUserNameResponseData>("user/edit");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "userName", changeuserName }
        });

        request.onSuccess = (response) =>
        {
            UserData.Get().Set(response.tUsers);
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// ユーザー機器移動情報取得通信
    /// </summary>
    public static void CallCreateDeviceChangeCode(Action<CreateDeviceChangeCodeResponseData> onCompleted)
    {
        var request = new SlotWebRequest<CreateDeviceChangeCodeResponseData>("user/createCode");
        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// 引継ぎコード入力後ユーザーID・PASS取得通信
    /// </summary>
    public static void CallDeviceChangeCode(
        string takeOverId,
        string takeOverPass,
        Action<DeviceChangeCodeResponseData> onCompleted,
        Action<int> onError)
    {
        var request = new SlotWebRequest<DeviceChangeCodeResponseData>("user/takeOver");
        
        request.SetRequestParameter(new Dictionary<string, object>
        {
            {"takeOverId", takeOverId},
            {"takeOverPassword", takeOverPass},
            {"authType", 3},
            {"deviceType", SlotDefine.DEVICE_TYPE}
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.onError = (errorCode) =>
        {
            onError?.Invoke(errorCode);
        };

        request.Send();
    }
}
