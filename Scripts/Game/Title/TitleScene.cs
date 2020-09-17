using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : SceneBase
{
    protected override void Awake()
    {
        base.Awake();

        UserData.Set(new UserData());
        UserData.Get().Load();
    }

    public void Start()
    {
        // TODO. 通信完了後の許容に変更
        SharedScene.instance.EnableTouch();
    }

    public void OnClickButton()
    {
        //TODO.
        this.Login();

    }

    public void OnClickOpenDialogButton()
    {
        var dialog = SharedScene.instance.ShowSimpleDialog();
        dialog.titleText.text = "Title";
        dialog.closeButtonEnabled = true;

        var content = dialog.AddText("Test");
        //content.buttonGroup.buttons[0].text.text = "OK";
        //content.buttonGroup.buttons[0].onClick = dialog.Close;
        dialog.AddYesNoButton();
    }

    private void Login()
    {
        var queue = new Queue<Action>();

        //ユーザーデータがある
        if (UserData.Get().userId > 0)
        {
            //ログイン
            queue.Enqueue(() => LoginApi.CallLoginApi(
                UserData.Get(),
                queue.Dequeue()
            ));
        }
        //ユーザーデータがない
        else
        {
            //ユーザーデータ作成
            queue.Enqueue(() => UserApi.CallCreateApi(
                "GuestUser",
                queue.Dequeue()
            ));
        }

        //ユーザー情報取得
        queue.Enqueue(() => FirstApi.CallFirstUserApi(
            UserData.Get(),
            queue.Dequeue()
        ));

        //マスター分割取得その１
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.LocalizeTextDB
        ));

        // //マスター分割取得その２
        // queue.Enqueue(() => MasterApi.CallGetMasterApi(
        //     queue.Dequeue(),
        //     Masters.FishCategoryDB,
        //     Masters.FishParticleDB,
        //     Masters.GearDB,
        //     Masters.BattleItemDB,
        //     Masters.ItemSellDB,
        //     Masters.LevelDB,
        //     Masters.BetDB,
        //     Masters.LocalizeTextDB,
        //     Masters.LoginBonusDB,
        //     Masters.LoginBonusSpecialDB
        // ));

        // //マスター分割取得その３
        // queue.Enqueue(() => MasterApi.CallGetMasterApi(
        //     queue.Dequeue(),
        //     Masters.PartsExpansionDB,
        //     Masters.CannonExpansionDB,
        //     Masters.GearExpansionDB,
        //     Masters.MessageDB,
        //     Masters.MissionTypeDB,
        //     Masters.MissionRewardDB,
        //     Masters.MultiWorldDB,
        //     Masters.MultiBallDropRateDB,
        //     Masters.MultiSoulDropRateDB,
        //     Masters.MultiStageFishDB
        // ));

        // //マスター分割取得その４
        // queue.Enqueue(() => MasterApi.CallGetMasterApi(
        //     queue.Dequeue(),
        //     Masters.SerieseSkillDB,
        //     Masters.SingleStageDB,
        //     Masters.SingleStageFishDB,
        //     Masters.SingleStageFirstRewardDB,
        //     Masters.SingleStageRewardDB,
        //     Masters.SingleStageRewardLotDB,
        //     Masters.SingleWorldDB,
        //     Masters.SkillDB,
        //     Masters.SkillGroupDB,
        //     Masters.VipBenefitDB
        // ));

        // //マスター分割取得その５
        // queue.Enqueue(() => MasterApi.CallGetMasterApi(
        //     queue.Dequeue(),
        //     Masters.VipBenefitTypeDB,
        //     Masters.VipLevelDB,
        //     Masters.VipRewardDB
        // ));

        // //ローカライズアトラスセット
        // queue.Enqueue(() =>
        // {
        //     var handle = AssetManager.Load<SpriteAtlas>(LocalizeImage.GetLocalizationAtlasPath(), (asset) =>
        //     {
        //         var atlas = new AtlasSpriteCache(asset);
        //         GlobalSpriteAtlas.SetAtlas(GlobalSpriteAtlas.AtlasType.Localization, atlas);
        //         queue.Dequeue().Invoke();
        //     });

        //     handle.isDontDestroy = true;
        // });

        //HOMEシーンへ
        queue.Enqueue(() =>
            SceneChanger.ChangeSceneAsync("Home")
        );

        //Queue実行
        queue.Dequeue().Invoke();
    }
}
