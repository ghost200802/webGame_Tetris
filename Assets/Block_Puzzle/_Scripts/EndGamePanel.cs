using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndGamePanel : MonoBehaviour {
    public Image diffImg;
    public Text scoreTxt, lineTxt, highScoreTxt, highLineTxt, timerTxt, timeTypeTxt;
    public GameObject highScoreObj, starObjs, replayBtn, nextBtn, leaderboardBtn, replaySmallBtn, fbBtn;
    public Image status;
    public RectTransform infor;
    public Sprite gameoverSprite, completeSprite;
    public Animator[] stars;
    
    public void UpdateInfo(int diffVal, int scoreVal, int lineVal, int time, bool isClassic, bool complete, int continueCount)
    {
        status.sprite = complete ? completeSprite : gameoverSprite;
        lineTxt.text = lineVal.ToString();
        diffImg.sprite = MonoUtils.instance.difficultySprites[diffVal - 1];
        highScoreObj.SetActive(isClassic);
        starObjs.SetActive(!isClassic);
        infor.anchoredPosition = new Vector2(0, isClassic ? 20 : -20);
        scoreTxt.text = scoreVal.ToString();
        //buttons
        replayBtn.SetActive(!complete);
        nextBtn.SetActive(complete);
        leaderboardBtn.SetActive(isClassic);
        replaySmallBtn.SetActive(complete);
        fbBtn.SetActive(!isClassic && !complete);
        //time
        timeTypeTxt.text = isClassic ? "TIME" : "REMAINING TIME";
        System.TimeSpan t = System.TimeSpan.FromSeconds(time);
        timerTxt.text = string.Format("{0:D1}:{1:D2}", t.Minutes, t.Seconds);
        if (isClassic)
        {
            int highScore = Superpow.Utils.GetHighScore();
            if(highScore < scoreVal)
            {
                highScore = scoreVal;
                Superpow.Utils.SetHighScore(scoreVal);
                JobWorker.instance.ReportScore(scoreVal);
            }
            highScoreTxt.text = highScore.ToString();
            int highLine = Superpow.Utils.GetHighLine();
            if (highLine < lineVal)
            {
                highLine = lineVal;
                Superpow.Utils.SetHighLine(highLine);
            }
            highLineTxt.text = highLine.ToString();
        }
        else
        {
            if (complete)
            {
                int currWorld = LevelController.GetCurrentWorld();
                int lvl = LevelController.GetCurrentLevel(currWorld);
                LevelController.SetUnlockLevel(currWorld, Mathf.Min(lvl + 1, Superpow.Utils.GetNumLevels(currWorld)));
                int numStar = continueCount == 0 ? 3 : continueCount == 1 ? 2 : 1;
                StartCoroutine(UpdateStars(numStar));
                LevelController.SetNumStar(currWorld, lvl, numStar);
                UpdateUnlockWorld();
            }
            else
            {
                StartCoroutine(UpdateStars(0));
            }
        }
    }

    private void UpdateUnlockWorld()
    {
        int total = Superpow.Utils.CalculateTotalStar();
        int unlockWorld = LevelController.GetUnlockWorld();

        if (unlockWorld <= Const.NUM_WORLD && total >= Const.UNLOCK_STARS[unlockWorld])
            LevelController.SetUnlockWorld(unlockWorld + 1);
    }

    public IEnumerator UpdateStars(int num)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Rebind();
            if (i < num)
            {
                stars[i].SetTrigger("show");
                int idx = i;
                PlaySound(idx);
            }
            else break;
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void PlaySound(int i)
    {
        Timer.Schedule(this, 0.6f, () =>
        {
            Sound.instance.Play(i == 0 ? Sound.Others.Star1 : (i == 1 ? Sound.Others.Star2 : Sound.Others.Star3));
        });
    }
}
