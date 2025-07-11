using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PeriodAction : MonoBehaviour {
    public string actionName = "";
    public int periodInSeconds = 1;
    public int startInSeconds = 0;

    public UnityEvent onActionReached;

    private void Start()
    {
        if (!CPlayerPrefs.HasKey(actionName + "_time")) // First time.
        {
            CUtils.SetActionTime(actionName, CUtils.GetCurrentTime() - periodInSeconds + startInSeconds);
        }

        UpdateAction();
    }

    public void UpdateAction()
    {
        if (CUtils.IsActionAvailable(actionName, periodInSeconds))
        {
            CUtils.SetActionTime(actionName);
            if (onActionReached != null) onActionReached.Invoke();
        }
    }
}
