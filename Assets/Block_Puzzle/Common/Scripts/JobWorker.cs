using UnityEngine;
#if USE_GPGS
using GooglePlayGames;
#endif
using System;

public class JobWorker : MonoBehaviour
{
    public Action<string> onEnterScene;
    public Action onLink2Store;
    public Action onDailyGiftReceived;
    public Action onShowBanner;
    public Action onCloseBanner;
    public Action onShowFixedBanner;
    public Action onShowInterstitial;

    public static JobWorker instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CurrencyController.onBallanceIncreased += OnBallanceIncreased;
#if USE_GPGS
        PlayGamesPlatform.Activate();
#endif
    }

    private void OnBallanceIncreased(int value)
    {
        int currentScore = CUtils.GetLeaderboardScore();
        CUtils.SetLeaderboardScore(currentScore + value);
    }

    public void ShowLeaderboard()
    {
#if USE_GPGS
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                ReportScore(Superpow.Utils.GetHighScore());
                PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_high_score);
            }
        });
#endif
    }

    public void ReportScore(int score)
    {
        if (Social.localUser.authenticated)
        {
            Social.ReportScore(score, GPGSIds.leaderboard_high_score, (bool postSuccess) => {
                // handle success or failure
            });
        }
    }
}