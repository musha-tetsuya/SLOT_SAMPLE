using System;
using System.Collections.Generic;

/// <summary>
/// ログイン系API.
/// </summary>
public class LoginApi
{
    /// <summary>
    /// user/loginのレスポンスデータ
    /// </summary>
    public class LoginResponseData
    {
        public bool loginSuccess;
        public LoginUserData userData;
        public bool isMaxPossession;
    }

    /// <summary>
    /// ログイン時に取得できるのIDとhashのレスポンスデータ
    /// </summary>
    public class LoginUserData
    {
        public int userId;
        public string hash;
    }


    /// <summary>
    /// ログイン通信
    /// </summary>
    public static void CallLoginApi(UserData userData, Action onCompleted)
    {
        //リクエスト作成
        var request = new SlotWebRequest<LoginResponseData>("user/login");

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "userId",     userData.userId },
            { "password",   userData.password },
            { "authType",   3 },
            { "deviceType", SlotDefine.DEVICE_TYPE },
        });
        
        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //ログインの合否を判定
            // ToDo 自身の端末内のパスワードが消失した場合にはアカウントの再発行となるので
            //      アカウント作り直しのフローを作る必要がある
            /*if (!response.loginSuccess)
            {
                //リトライ用のダイアログを設定
                SimpleDialog dialog = SharedUI.Instance.ShowSimpleDialog();
                var content = dialog.SetAsMessageDialog("ログインに失敗しました" + "\n" + "login_success = " + response.loginSuccess);
                content.buttonGroup.buttons[0].onClick = dialog.Close;
                dialog.onClose = onRetry;
                return;
            }*/

            //ログイン時に取得するuserIDとhashの設定
            userData.Set(response.userData);
            userData.Save();

            //通信完了
            onCompleted?.Invoke();
        };

        //通信開始
        request.Send();
    }
}
