using UnityEngine;
using System;

[System.Serializable]
public class GameConfig
{
    public Admob admob;

    [Header("")]
    public int adPeriod;
    public int rewardedVideoPeriod;
    public int rewardedVideoAmount;

    [Header("")]
    public string androidPackageID;
    public string iosAppID;
    public string facebookPageID;
    public string developerLinkGooglePlay;
    public string developerLinkItunes;

    [Header("")]
    public int[] rubyCostToContinue;
    public int continueTime = 30;
}

[System.Serializable]
public class Admob
{
    [Header("Interstitial")]
    public string androidInterstitial;
    public string iosInterstitial;
    [Header("Banner")]
    public string androidBanner;
    public string iosBanner;
    [Header("RewardedVideo")]
    public string androidRewarded;
    public string iosRewarded;
}
