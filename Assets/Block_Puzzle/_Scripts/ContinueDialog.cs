using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ContinueDialog : YesNoDialog {
    public Text yesText;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(IECountDown(7));
    }

    public IEnumerator IECountDown(int time)
    {
        yesText.text = "YES (" + time + ")";
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            yesText.text = "YES (" + time + ")";
        }
        OnNoClick();
    }
}
