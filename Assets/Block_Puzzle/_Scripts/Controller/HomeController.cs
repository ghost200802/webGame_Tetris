using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UomaWeb;

public class HomeController : BaseController {

    public static HomeController instance;

    public GameObject ControlPanel = null;
    
    protected override void Awake()
    {
        instance = this;
        base.Awake();
        this.ControlPanel.SetActive(false);
        
        
        StartCoroutine(UomaController.Instance.GetVirtualCurrency((int currency) =>
        {
            StartCoroutine(UomaController.Instance.GetCompleteLevel((int result) =>
            {
                this.ControlPanel.SetActive(true);
            }));
        }));
    }

    protected override void Start()
    {
        base.Start();
    }

    public void OnPlayClick()
    {
        Sound.instance.PlayButton();
        CUtils.LoadScene(2);
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
