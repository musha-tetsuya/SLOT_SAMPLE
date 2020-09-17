using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slotにおける共通定義
/// </summary>
public static class SlotDefine
{
    public const int SCREEN_WIDTH = 1920;
    public const int SCREEN_HEIGHT = 1080;
    public const float SCREEN_ASPECT_WH = SCREEN_WIDTH / SCREEN_HEIGHT;
    public const float SCREEN_ASPECT_HW = SCREEN_HEIGHT / SCREEN_WIDTH;
    public const int FRAMERATE = 60;
    public const float DELTATIME = 1f / FRAMERATE;
    public const string YES_BTN_SPRITE_NAME = "CmBtn_010_0001";
    public const string NO_BTN_SPRITE_NAME = "CmBtn_010_0002";
#if UNITY_IOS && !UNITY_EDITOR
    public const int DEVICE_TYPE = 1;
#else
    public const int DEVICE_TYPE = 2;
#endif

    /// <summary>
    /// 砲弾サムネイルのパスを取得する
    /// </summary>
    public static string GetTestImagePath(string key) => string.Format("Textures/TestImages/{0}", key);

    // /// <summary>
    // /// SEクリップのパスを取得する
    // /// </summary>
    // public static string GetSeClipPath(string key) => string.Format("Sound/Se/{0}", key);
    // /// <summary>
    // /// BGMクリップのパスを取得する
    // /// </summary>
    // public static string GetBgmClipPath(string key) => string.Format("Sound/Bgm/{0}", key);
    // /// <summary>
    // /// シングルステージアイコンスプライトパスを取得する
    // /// </summary>
    // public static string GetSingleStageIconSpritePath(string key) => string.Format("Textures/Thumbnail/SStageIcon/{0}", key);
    // /// <summary>
    // /// ステージセレクト背景スプライトのパスを取得する
    // /// </summary>
    // public static string GetStageSelectBgSpritePath(string key) => string.Format("Textures/StageSelectBg/{0}", key);
    // /// <summary>
    // /// バトルWAVEデータのパスを取得する
    // /// </summary>
    // public static string GetFishWaveDataPath(string key) => string.Format("ScriptableObject/FishWaveData/{0}", key);
    // /// <summary>
    // /// マルチバトル用WAVEデータのパスを取得する
    // /// </summary>
    // public static string GetMultiFishWaveGroupDataPath(string key) => string.Format("ScriptableObject/MultiFishWaveGroupData/{0}", key);
    // /// <summary>
    // /// ランダム回遊ルートデータのパスを取得する
    // /// </summary>
    // public static string GetRandomFishRouteDataPath(string key) => string.Format("ScriptableObject/RandomFishRouteData/{0}", key);
    // /// <summary>
    // /// バトル背景スプライトのパスを取得する
    // /// </summary>
    // public static string GetBattleBgSpritePath(string key) => string.Format("Textures/BattleBg/{0}", key);
    // /// <summary>
    // /// 魚サムネイルスプライトのパスを取得する
    // /// </summary>
    // public static string GetFishThumbnailSpritePath(string key) => string.Format("Textures/Thumbnail/Fish/{0}", key);
    // /// <summary>
    // /// 魚図鑑の背景スプライトのパスを取得する
    // /// </summary>
    // public static string GetZukanBgSpritePath(string key) => string.Format("Textures/Thumbnail/ZukanBg/{0}", key);
    // /// <summary>
    // /// 砲台セットスプライトのパスを取得する
    // /// </summary>
    // public static string GetTurretSetSpritePath(string key) => string.Format("Textures/Thumbnail/TurretSet/{0}", key);
    // /// <summary>
    // /// 台座スプライトのパスを取得する
    // /// </summary>
    // public static string GetBatterySpritePath(string key) => string.Format("Textures/Thumbnail/Battery/{0}", key);
    // /// <summary>
    // /// 台座プレハブのパスを取得する
    // /// </summary>
    // public static string GetBatteryPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Battery", key);
    // /// <summary>
    // /// 砲身スプライトのパスを取得する
    // /// </summary>
    // public static string GetBarrelSpritePath(string key) => string.Format("Textures/Thumbnail/Barrel/{0}", key);
    // /// <summary>
    // /// 砲身プレハブのパスを取得する
    // /// </summary>
    // public static string GetBarrelPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Barrel", key);
    // /// <summary>
    // /// 砲弾サムネイルのパスを取得する
    // /// </summary>
    // public static string GetBulletThumbnailPath(string key) => string.Format("Textures/Thumbnail/Bullet/{0}", key);
    // /// <summary>
    // /// 砲弾プレハブのパスを取得する
    // /// </summary>
    // public static string GetBulletPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Bullet", key);
    // /// <summary>
    // /// アクセサリサムネイルのパスを取得する
    // /// </summary>
    // public static string GetAccessoryThumbnailPath(string key) => string.Format("Textures/Thumbnail/Acce/{0}", key);
    // /// <summary>
    // /// FVアタックプレハブのパス
    // /// </summary>
    // public static string GetFvAttackPrefabPath(FvAttackType fvAttackType) => string.Format("Prefabs/FvAttack/FvAttack{0}", fvAttackType);
    // /// <summary>
    // /// FVアタック用砲弾プレハブのパスを取得する
    // /// </summary>
    // public static string GetFvAttackBulletPrefabPath(string key) => string.Format("Prefabs/FVA/{0}/{0}", key);
    // /// <summary>
    // /// FVアタック用砲弾のチャージエフェクトプレハブのパスを取得する
    // /// </summary>
    // public static string GetFVAChargeEffectPrefabPath(string key) => string.Format("Prefabs/FVA/{0}/Particle/{0}_Charge", key);
    // /// <summary>
    // /// FVアタックタイプアイコンスプライトのパス
    // /// </summary>
    // public static string GetFvAttackTypeIconSpritePath(FvAttackType fvAttackType) => string.Format("Textures/FvAttackTypeIcon/{0}", fvAttackType);
    // /// <summary>
    // /// シリーズスキルアイコンスプライトのパス
    // /// </summary>
    // public static string GetSeriesSkillIconSpritePath(string key) => string.Format("Textures/SetSkillThumbnail/{0}", key);
    // /// <summary>
    // /// バトルアイテムアイコンスプライトのパスを取得
    // /// </summary>
    // public static string GetBattleItemIconSpritePath(string key) => string.Format("Textures/BattleItemIcon/{0}", key);
    // /// <summary>
    // /// ギアスプライトのパスを取得する
    // /// </summary>
    // public static string GetGearItemIconSpritePath(string key) => string.Format("Textures/Gear/{0}", key);
    // /// <summary>
    // /// 魚FBXのパス
    // /// </summary>
    // public static string GetFishFbxPath(string key) => string.Format("Models/Fish/{0}/{0}", key);
    // /// <summary>
    // /// 魚アニメーターコントローラのパス
    // /// </summary>
    // public static string GetFishAnimatorControllerPath(string key) => string.Format("Models/Fish/{0}/{0}", key);
    // /// <summary>
    // /// 魚コライダデータのパス
    // /// </summary>
    // public static string GetFishColliderDataPath(string key) => string.Format("Models/Fish/{0}/{0}-colliderdata", key);
    // /// <summary>
    // /// 魚パーティクルのパス
    // /// </summary>
    // public static string GetFishParticlePath(string key, string particleName) => string.Format("Models/Fish/{0}/Particle/{1}", key, particleName);
}

/// <summary>
/// 言語
/// </summary>
public enum Language
{
    Ja,
    Zh,
    Tw,
    En,
}

/// <summary>
/// アイテムタイプ
/// </summary>
public enum ItemType
{
    Coin        = 1,    //コイン
}

/// <summary>
/// テキストのカラータイプ
/// </summary>
public enum TextColorType
{
    None            = 0,
    IncreaseParam   = 1,    //パラメータの増加時のカラー
    DecreaseParam   = 2,    //パラメータの減少時のカラー
    Alert           = 3,    //警告文のカラー
}

/// <summary>
/// エラーコード
/// </summary>
public enum ErrorCode
{
    BillingFatalError       = 40005,    //レシートは正しいが、マスターかクライアントが原因っぽいエラー
    BillingTimeError        = 40006,    //レシートは正しいが、マスターかクライアントが原因っぽいエラー
    ProductIdNotFound       = 40007,    //レシートは正しいが、プロダクトIDが見つからない（マスターかクライアントが原因っぽいエラー）
    AlreadyCheckReceiptId   = 40008,    //レシートは正しいが、すでに検証済みのレシート

    PresentBoxError         = 70002,    //プレゼントBox関連のとりあえずエラー
    PresentBoxNotFound      = 70003,    //プレゼントBoxから受け取るアイテムのIDに対応するものが見つからない (不正なアイテム等)
    PresentBoxClosed        = 70004,    //プレゼントBoxから受け取るアイテムの期限切れてしまっている
    PresentBoxReceived      = 70005,    //プレゼントBoxから受け取るアイテムが既に受取済み
    PresentBoxEmpty         = 70006,    //プレゼントBoxが空っぽの状態で受け取ろうとしている
    PresentBoxMaxPossession = 70007     //プレゼントBoxの所持数が上限を超えている
}