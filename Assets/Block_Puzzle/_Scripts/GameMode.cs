using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Superpow;
public class GameMode : MonoBehaviour {
    public GameObject classicSelected, challengeSelected;
    public CanvasGroup difficulty;
    private int selectedIndex = 0, diff = 0;
    public Sprite[] difficulties;
    public Image difficultyImage;
    public static readonly int CLASSIC_MODE = 0, CHALLENGE_MODE = 1;
    public void Start()
    {
        diff = Utils.GetDifficulty();
        int mode = Utils.GetGameMode();
        UpdateModes(mode);
        UpdateDifficultySprite();
    }

    public void OnSelectMode(int index)
    {
        Sound.instance.PlayButton();
        UpdateModes(index);
    }

    private void UpdateModes(int index)
    {
        selectedIndex = index;
        classicSelected.SetActive(index == 0);
        challengeSelected.SetActive(index == 1);
        difficulty.alpha = index == 0 ? 1 : 0.5f;
        Utils.SetGameMode(index);
    }

    public void OnNextClick()
    {
        if (selectedIndex == 0 && diff < difficulties.Length)
        {
            Sound.instance.PlayButton();
            diff++;
            Utils.SetDifficulty(diff);
            UpdateDifficultySprite();
        }
    }

    private void UpdateDifficultySprite()
    {
        difficultyImage.sprite = difficulties[diff - 1];
    }

    public void OnPrevClick()
    {
        if (selectedIndex == 0 && diff > 1)
        {
            Sound.instance.PlayButton();
            diff--;
            Utils.SetDifficulty(diff);
            UpdateDifficultySprite();
        }
    }

    public static bool IsClassicMode()
    {
        return Utils.GetGameMode() == CLASSIC_MODE;
    }
}
