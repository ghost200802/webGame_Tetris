using UnityEngine;

public class UICamera : CCamera
{
    public static UICamera instance;
    public Transform screenTransform, botPanel;
    protected override void Awake()
    {
        instance = this;
        base.Awake();
        UpdateScreen();
    }

    public void UpdateScreen()
    {
        float widthCam = UICamera.instance.virtualWidth;
        float heightCam = UICamera.instance.virtualHeight;
        float scale1 = (heightCam - 290) / 866;
        float scale2 = (widthCam - 40) / 580;
        float scale = Mathf.Min(scale1, scale2);
        screenTransform.localScale = new Vector3(scale, scale);
        botPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (heightCam - scale * 866) / 2);
    }
}
