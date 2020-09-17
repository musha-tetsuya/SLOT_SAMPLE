using UnityEngine;
using UnityEngine.UI;

public class HomeScene : SceneBase
{
    // TODO. AssetLoader Test
    private AssetListLoader assetLoader = new AssetListLoader();
    // TODO. Test Data
    private string[] key = new string[] {"T0014_BlackBird_00", "T0014_BlackBird_01", "T0014_BlackBird_02"};
    // TODO. Test Image
    [SerializeField]
    private Image testImage = null;
    [SerializeField]
    private Text userIdText = null;

    /// <summary>
    /// TODO. OnDestroy
    /// </summary>
    protected override void OnDestroy() 
    {
        this.assetLoader.Unload();
    }

    private void Start()
    {
        this.Load();
    }

    /// <summary>
    /// TODO. Sprite Load
    /// </summary>
    private void Load()
    {
        // string key
        foreach(string key in this.key)
        {
            this.assetLoader.Add<Sprite>(SlotDefine.GetTestImagePath(key));
        }

        // Sprite Load
        this.assetLoader.Load(this.OnLoaded);

        this.userIdText.text = "UserID : " + UserData.Get().userId.ToString();
    }

    /// <summary>
    /// TODO. After Sprite Load
    /// </summary>
    private void OnLoaded()
    {
        var path = this.assetLoader[0].path;
        var handle = AssetManager.FindHandle<Sprite>(path).asset as Sprite;
        this.testImage.sprite = handle;
    }

    // TODO. Change Test
    public void OnClickAssetLoadButton()
    {
        int num = Random.Range(0, 3);
        var path = this.assetLoader[num].path;
        var handle = AssetManager.FindHandle<Sprite>(path).asset as Sprite;
        this.testImage.sprite = handle;
    }

    public void OnClickButton()
    {
        SceneChanger.ChangeSceneAsync("Title");
    }

    public void OnClickSNSButton()
    {
        SceneChanger.ChangeSceneAsync("Test_SNSLinkage");
    }

    public void OnClickCoinTextButton()
    {
        SceneChanger.ChangeSceneAsync("NumberCounterTestScene");
    }
}
