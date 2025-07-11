using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectLevelController : BaseController {
    public Text worldName, collectTxt, totalStarTxt;
    public GameObject lockedContent;
    private int currWorld;
    public Transform levelContent;
    public GameObject levelButtonPrefab;


    protected override void Start()
    {
        base.Start();
        currWorld = LevelController.GetUnlockWorld();
        currWorld = Mathf.Clamp(currWorld, 1, Const.NUM_WORLD);

        UpdateInfor();
        UpdateTotalStar();
    }

    public void UpdateTotalStar()
    {
        int total = Superpow.Utils.CalculateTotalStar();
        totalStarTxt.text = total + "/" + Superpow.Utils.GetTotalLevels() * 3;
    }

    public void UpdateInfor()
    {
        for(int i = levelContent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(levelContent.GetChild(i).gameObject);
        }

        int numLevels = Superpow.Utils.GetNumLevels(currWorld);
        for (int i = 0; i < numLevels; i++)
        {
            GameObject levelButton = Instantiate(levelButtonPrefab);
            levelButton.transform.SetParent(levelContent);
            levelButton.transform.localScale = Vector3.one;
        }

        int unlockWorld = LevelController.GetUnlockWorld();
        lockedContent.SetActive(currWorld > unlockWorld);
        collectTxt.text = "Collect " + Const.UNLOCK_STARS[currWorld - 1];
        LevelController.SetCurrentWorld(currWorld);
        worldName.text = Const.WORLD_NAMES[currWorld - 1];

        foreach (Transform tf in levelContent)
        {
            tf.GetComponent<LevelButton>().UpdateLevelState();
        }
    }

    public void OnNextWorld()
    {
        if(currWorld < Const.NUM_WORLD)
        {
            Sound.instance.PlayButton();
            currWorld++;
            UpdateInfor();
        }
    }

    public void OnPrevWorld()
    {
        if (currWorld > 1)
        {
            Sound.instance.PlayButton();
            currWorld--;
            UpdateInfor();
        }
    }

    public void OnPlayClick()
    {
        Sound.instance.PlayButton();
        Superpow.Utils.SetGameMode(GameMode.CHALLENGE_MODE);
        int unlockLvl = LevelController.GetUnlockLevel(currWorld);
        LevelController.SetCurrentLevel(currWorld, unlockLvl);
        CUtils.LoadScene(2);
    }

    public void OnShopClick()
    {
        DialogController.instance.ShowDialog(DialogType.Shop);
    }
}
