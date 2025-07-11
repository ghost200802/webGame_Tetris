//#define DEVELOPMENT
public class CommonConst
{
    public const iTween.DimensionMode ITWEEN_MODE = iTween.DimensionMode.mode2D;

#if DEVELOPMENT
    public const bool ENCRYPTION_PREFS = false;
#else
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
    public const bool ENCRYPTION_PREFS = true;
#else
    public const bool ENCRYPTION_PREFS = false;
#endif
#endif

    public const int NOTIFICATION_DAILY_GIFT = 0;
    public const int MAX_AUTO_SIGNIN = 2;
}
