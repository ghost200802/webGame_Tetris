//#define USE_FIREBASE

#if USE_FIREBASE && (UNITY_ANDROID || UNITY_IOS)
using Firebase.Analytics;
#endif

public class CFirebase {

	public static void LogEvent(string contentType, string itemID, double value)
    {
#if USE_FIREBASE && (UNITY_ANDROID || UNITY_IOS)
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectContent,
            new Parameter(FirebaseAnalytics.ParameterContentType, contentType),
            new Parameter(FirebaseAnalytics.ParameterItemId, itemID),
            new Parameter(FirebaseAnalytics.ParameterValue, value));
#endif
    }

    public static void LogEvent(string contentType, string itemID)
    {
#if USE_FIREBASE && (UNITY_ANDROID || UNITY_IOS)
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectContent,
            new Parameter(FirebaseAnalytics.ParameterContentType, contentType),
            new Parameter(FirebaseAnalytics.ParameterItemId, itemID));
#endif
    }

    public static void LogEvent(string contentType)
    {
#if USE_FIREBASE && (UNITY_ANDROID || UNITY_IOS)
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectContent,
            new Parameter(FirebaseAnalytics.ParameterContentType, contentType));
#endif
    }
}
