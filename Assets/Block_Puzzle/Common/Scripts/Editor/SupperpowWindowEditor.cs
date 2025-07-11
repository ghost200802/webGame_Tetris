using UnityEngine;
using UnityEditor;

public class SuperpowWindowEditor
{
    [MenuItem("Superpow/Clear all playerprefs")]
    static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Superpow/Unlock all levels")]
    static void UnlockAllLevel()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
        LevelController.SetUnlockWorld(Const.NUM_WORLD + 1);

        for (int i = 1; i <= Const.NUM_WORLD; i++)
        {
            LevelController.SetUnlockLevel(i, Superpow.Utils.GetNumLevels(i) + 1);
        }
    }

    [MenuItem("Superpow/Credit balance (ruby, coin..)")]
    static void AddRuby()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
        CurrencyController.CreditBalance(1000);
    }

    [MenuItem("Superpow/Set balance to 0")]
    static void SetBalanceZero()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
        CurrencyController.SetBalance(0);
    }
}