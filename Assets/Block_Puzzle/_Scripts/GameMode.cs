using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Superpow;
public class GameMode : MonoBehaviour {
    private int selectedIndex = 0, diff = 0;
    public Sprite[] difficulties;
    public Image difficultyImage;
    public static readonly int CLASSIC_MODE = 0, CHALLENGE_MODE = 1;
    public void Start()
    {
        diff = Utils.GetDifficulty();
        int mode = Utils.GetGameMode();
        UpdateModes(mode);
    }

    public void OnSelectMode(int index)
    {
        Sound.instance.PlayButton();
        UpdateModes(index);
    }

    private void UpdateModes(int index)
    {
        selectedIndex = index;
        Utils.SetGameMode(index);
    }

    // public void OnNextClick()
    // {
    //     if (selectedIndex == 0 && diff < difficulties.Length)
    //     {
    //         Sound.instance.PlayButton();
    //         diff++;
    //         Utils.SetDifficulty(diff);
    //     }
    // }
    //
    //
    // public void OnPrevClick()
    // {
    //     if (selectedIndex == 0 && diff > 1)
    //     {
    //         Sound.instance.PlayButton();
    //         diff--;
    //         Utils.SetDifficulty(diff);
    //     }
    // }

    public static bool IsClassicMode()
    {
        return Utils.GetGameMode() == CLASSIC_MODE;
    }
}
