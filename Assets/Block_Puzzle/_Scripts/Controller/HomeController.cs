using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomeController : BaseController {

    public static HomeController instance;
    protected override void Awake()
    {
        instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void OnPlayClick()
    {
        Sound.instance.PlayButton();
        int mode = Superpow.Utils.GetGameMode();
        CUtils.LoadScene(mode == 0 ? 2 : 1);
    }

    public void OnFacebookClick()
    {
        Sound.instance.PlayButton();
        CUtils.LikeFacebookPage(ConfigController.Config.facebookPageID);
    }

    public void OnRemoveAdsClick()
    {
#if IAP && UNITY_PURCHASING
        Purchaser.instance.BuyProduct(4);
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }
}
