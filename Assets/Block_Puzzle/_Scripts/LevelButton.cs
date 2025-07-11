using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

    public Text levelTxt;
    public Image image;
    public Sprite lockSprite, currentSprite, passSprite;
    private int level, unlockLvl, currWorld;
    public GameObject[] stars;

    public void UpdateLevelState()
    {
        level = transform.GetSiblingIndex() + 1;
        currWorld = LevelController.GetCurrentWorld();
        unlockLvl = LevelController.GetUnlockLevel(currWorld);
        bool isLock = currWorld > LevelController.GetUnlockWorld() || level > unlockLvl;
        levelTxt.gameObject.SetActive(!isLock);
        levelTxt.text = level.ToString();
        image.sprite = isLock ? lockSprite : (level == unlockLvl ? currentSprite : passSprite);
        int star = LevelController.GetNumStar(currWorld, level);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(i < star);
        }
    }

    public void OnClick()
    {
        if (level <= unlockLvl)
        {
            Sound.instance.PlayButton();
            Superpow.Utils.SetGameMode(GameMode.CHALLENGE_MODE);
            LevelController.SetCurrentLevel(currWorld, level);
            CUtils.LoadScene(2);
        }
    }
}
