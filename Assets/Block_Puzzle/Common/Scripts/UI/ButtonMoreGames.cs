using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonMoreGames : MyButton {

    public override void OnButtonClick()
    {
        base.OnButtonClick();
#if UNITY_ANDROID
        Application.OpenURL(ConfigController.Config.developerLinkGooglePlay);
#elif UNITY_IPHONE
        Application.OpenURL(ConfigController.Config.developerLinkItunes);
#endif
    }
}
