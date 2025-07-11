using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using System.Text.RegularExpressions;

public class MainController : BaseController {

    public Brick brickPrefab;
    public Transform brickRegion;
    private List<Brick> bricks = new List<Brick>();
    private Brick currentBrick, staticBrick;
    public Text scoreText, levelText, timerTxt;
    public Brick nextBrick, next2Brick, shadowBrick, holdBrick;
    public RectTransform progressBar;
    public GameObject pausePanel, pauseBotPanel, playBotPanel, endGameBotPanel, touchHereTxt, goalRegion, timerRegion;
    public EndGamePanel endGamePanel;
    public GameObject pauseBtn, menuBtn;
    public enum GAMESTATE { PLAYING, PAUSED, ENDGAME }
    [HideInInspector]
    public GAMESTATE gameState;
    public static MainController instance;
    public static readonly int NUM_COL = 10, LIFT_ROW = 12, CONTINUE_COST = 2;
    private int totalScore, clearedLine, currDiff, currLevel, currWorld, goal, levelGoal, topRow = 0, numFalling = 0, continueCount = 0;
    private float totalTime, passTime, remainTime;
    private int gameMode;
    private bool checking = false, warningTime = false, timeUp = false;
    [HideInInspector]
    public bool holdable = false;

    protected override void Awake()
    {
        instance = this; 
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        gameMode = Superpow.Utils.GetGameMode();
        goalRegion.SetActive(gameMode == GameMode.CLASSIC_MODE);
        timerRegion.SetActive(gameMode == GameMode.CHALLENGE_MODE);
        if (gameMode == GameMode.CLASSIC_MODE)
        {
            currDiff = Superpow.Utils.GetDifficulty();
            levelGoal = currDiff * 2 + 8;
            levelText.text = currDiff.ToString();
        }
        else
        {
            currWorld = LevelController.GetCurrentWorld();
            currLevel = LevelController.GetCurrentLevel(currWorld);
            JSONNode node = Superpow.Utils.LoadLevelJson(currWorld, currLevel);
            string[] rows = node["bricks"].Value.Trim().Split('.');
            totalTime = node["time"].AsInt;
            remainTime = totalTime;
            currDiff = node["difficulty"].AsInt;
            SpawnStaticBrick(rows);
            levelText.text = currWorld + "-" + currLevel;
        }
        passTime = 0;
        totalScore = 0;
        clearedLine = 0;
        goal = 0;
        SpawnABrick(Random.Range(0, Brick.BRICK_POS.Length));
        next2Brick.brickType = Random.Range(0, Brick.BRICK_POS.Length);
        RandomNextBrick();
        gameState = GAMESTATE.PLAYING;
    }

    public void FixedUpdate()
    {
        if (gameState == GAMESTATE.PLAYING)
        {
            passTime += Time.deltaTime;
            if (gameMode == GameMode.CHALLENGE_MODE)
            {                
                UpdateTime();
            }
        }
    }

    private void UpdateTime()
    {
        remainTime = (totalTime - passTime);
        System.TimeSpan t = System.TimeSpan.FromSeconds(remainTime);
        if(!warningTime && remainTime <= 5)
        {
            warningTime = true;
            Sound.instance.Play(Sound.Others.WarningTime);
        }
        if(remainTime > 0)
        {
            timerTxt.text = string.Format("{0:D1}:{1:D2}", t.Minutes, t.Seconds);
        }
        else if(!timeUp)
        {
            remainTime = 0;
            timerTxt.text = "0:00";
            timeUp = true;
            StartCoroutine(OnTimeUp());
        }
    }

    private IEnumerator OnTimeUp()
    {
        while (true)
        {
            if (checking || gameState == GAMESTATE.PAUSED)
                yield return new WaitForSeconds(0.1f);
            else break;
        }
        if(gameState == GAMESTATE.PLAYING)
        {
            int totalCount = ConfigController.Config.rubyCostToContinue.Length;
            int cost = ConfigController.Config.rubyCostToContinue[continueCount];
            if (CurrencyController.GetBalance() >= cost && continueCount < totalCount)
            {
                System.Action onYes = (System.Action)(() =>
                {
                    continueCount++;
                    CurrencyController.DebitBalance(cost);
                    totalTime += ConfigController.Config.continueTime;
                    warningTime = false;
                    timeUp = false;
                    gameState = GAMESTATE.PLAYING;
                });
                System.Action onNo = (System.Action)(() =>
                {
                    ShowEndGame(gameMode == GameMode.CLASSIC_MODE, false);
                });
                gameState = GAMESTATE.PAUSED;
                ContinueDialog contDialog = (ContinueDialog)DialogController.instance.GetDialog(DialogType.Continue);
                contDialog.message.SetText(cost.ToString());
                contDialog.onYesClick = onYes;
                contDialog.onNoClick = onNo;
                DialogController.instance.ShowDialog(contDialog);
            }
            else
            {
                ShowEndGame(gameMode == GameMode.CLASSIC_MODE, false);
            }
        }
    }

    public void SpawnStaticBrick(string[] rows)
    {
        List<Vector2> positions = Superpow.Utils.GetPositions(rows);
        staticBrick = (Brick)Instantiate(brickPrefab);
        staticBrick.transform.SetParent(brickRegion);
        staticBrick.transform.localScale = Vector3.one;
        staticBrick.brickPos = new Vector2(0, Mathf.Min(0, LIFT_ROW - rows.Length));
        staticBrick.SpawnStaticBrick(positions);
        staticBrick.GetComponent<RectTransform>().anchoredPosition = new Vector3(staticBrick.brickPos.x * Brick.BRICK_SIZE, staticBrick.brickPos.y * Brick.BRICK_SIZE);
        staticBrick.movable = false;
        bricks.Add(staticBrick);
    }
    
    public void SpawnABrick(int type, bool allowHold = true)
    {
        Brick brick = (Brick)Instantiate(brickPrefab);
        brick.transform.SetParent(brickRegion);
        brick.transform.localScale = Vector3.one;
        brick.brickPos = new Vector2(5, Mathf.Max(topRow + 2, 19));
        brick.SpawnBrick(type);
        brick.GetComponent<RectTransform>().anchoredPosition = new Vector3(brick.brickPos.x * Brick.BRICK_SIZE, brick.brickPos.y * Brick.BRICK_SIZE);
        brick.onStopped += OnBrickStopped;
        brick.normalSpeed = CalculateSpeed(currDiff);
        currentBrick = brick;
        holdable = allowHold;
        bricks.Add(brick);

        shadowBrick.SpawnBrick(type, true);
        UpdateShadow();
    }

    public float CalculateSpeed(int diff)
    {
        float plus = 0;
        for(int i = 0; i < diff; i++)
        {
            plus += Mathf.Max(0.01f, (0.1f - i * 0.005f));
        }
        return Mathf.Max(0.02f, 1f - plus);
    }

    public void UpdateShadow()
    {
        for(int i = 0; i < currentBrick.parts.Count; i++)
        {
            shadowBrick.parts[i].pos = currentBrick.parts[i].pos;
            shadowBrick.parts[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(currentBrick.parts[i].pos.x * Brick.BRICK_SIZE, currentBrick.parts[i].pos.y * Brick.BRICK_SIZE);
        }
        shadowBrick.UpdateCorner();
        int minY = (int)(currentBrick.brickPos.y + currentBrick.botLeft.y);
        foreach (BrickPart p in currentBrick.parts)
        {
            int col = (int)(p.pos.x + currentBrick.brickPos.x);
            foreach (Brick br in bricks)
            {
                if (br != currentBrick && br.brickPos.x + br.botLeft.x <= col && br.brickPos.x + br.topRight.x >= col)
                {
                    foreach (BrickPart bp in br.parts)
                    {
                        if(br.brickPos.x + bp.pos.x == col && currentBrick.brickPos.y + p.pos.y > br.brickPos.y + bp.pos.y)
                        {
                            int deltaY = (int)(currentBrick.brickPos.y + p.pos.y - br.brickPos.y - bp.pos.y - 1);
                            if (deltaY < minY) minY = deltaY;
                        }
                    }
                }
            }
        }
        shadowBrick.brickPos = new Vector2(currentBrick.brickPos.x, currentBrick.brickPos.y - minY);
        shadowBrick.UpdateAnchoredPos();
    }

    public void Update()
    {
        if (currentBrick != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                currentBrick.StartMoveLeft();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                currentBrick.StartMoveRight();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentBrick.StartMoveFast();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                currentBrick.Rotate();
            }
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
            {
                currentBrick.StopMoveLeft();
            }
            if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
            {
                currentBrick.StopMoveRight();
            }
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            {
                currentBrick.StopMoveFast();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentBrick.MoveFlash();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                OnHoldClick();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPlaying())
                {
                    OnPauseClick();
                }
                else if (IsPaused())
                {
                    OnResumeClick();
                }
            }
        }
    }

    public void OnRightPressDown()
    {        
        currentBrick.StartMoveRight();
    }

    public void OnRightPressUp()
    {
        currentBrick.StopMoveRight();
    }

    public void OnLeftPressDown()
    {
        currentBrick.StartMoveLeft();
    }

    public void OnLeftPressUp()
    {
        currentBrick.StopMoveLeft();
    }

    public void OnDownPressDown()
    {
        currentBrick.StartMoveFast();
    }
    public void OnDownPressUp()
    {
        currentBrick.StopMoveFast();
    }

    public void OnRotatePress()
    {
        currentBrick.Rotate();
    }

    public void OnMoveFlashPress()
    {
        currentBrick.MoveFlash();
    }

    public void OnPauseClick()
    {
        gameState = GAMESTATE.PAUSED;
        Sound.instance.PlayButton();
        CUtils.ShowBannerAd();
        UpdatePanels();
    }

    public void OnResumeClick()
    {
        gameState = GAMESTATE.PLAYING;
        Sound.instance.PlayButton();
        CUtils.CloseBannerAd();
        UpdatePanels();
    }

    public void OnMenuClick()
    {
        CUtils.CloseBannerAd();
        Sound.instance.PlayButton();
        CUtils.LoadScene(gameMode == GameMode.CLASSIC_MODE ? 0 : 1);
    }

    public void OnNextClick()
    {
        CUtils.CloseBannerAd();
        Sound.instance.PlayButton();

        if (currLevel < Superpow.Utils.GetNumLevels(currWorld))
        {
            LevelController.SetCurrentLevel(currWorld, currLevel + 1);
            CUtils.LoadScene(2);
        }
        else
        {
            int unlockWorld = LevelController.GetUnlockWorld();
            if (currWorld < Const.NUM_WORLD && currWorld + 1 <= unlockWorld)
            {
                LevelController.SetCurrentWorld(currWorld + 1);
                LevelController.SetCurrentLevel(currWorld + 1, 1);
                CUtils.LoadScene(2);
            }
            else
            {
                CUtils.LoadScene(1);
            }
        }
    }

    public void OnHoldClick()
    {
        bool isHeld = holdBrick.parts.Count > 0;
        if(IsPlaying() && holdable && (!isHeld || holdBrick.brickType != currentBrick.brickType))
        {
            Sound.instance.Play(Sound.Others.Hold);
            holdable = false;
            touchHereTxt.SetActive(false);
            shadowBrick.DestroyAllPart();
            int nextType = isHeld ? holdBrick.brickType : nextBrick.brickType;
            holdBrick.DestroyAllPart();
            holdBrick.SpawnBrick(currentBrick.brickType);
            Vector2 center = holdBrick.GetCenter();
            holdBrick.transform.localPosition = new Vector3((-center.x) * Brick.BRICK_SIZE / 2, (-center.y) * Brick.BRICK_SIZE / 2);
            RemoveBrick(currentBrick);
            SpawnABrick(nextType, false);
            if (!isHeld)
            {
                RandomNextBrick();
            }
        }
        else
        {
            Sound.instance.Play(Sound.Others.CantHold);
        }
    }

    public void OnLeaderboardClick()
    {
#if UNITY_EDITOR
        Debug.Log("You need to implement this function by yourself or simply disable it");
#endif
    }

    public void OnFacebookClick()
    {
        Sound.instance.PlayButton();
        CUtils.LikeFacebookPage(ConfigController.Config.facebookPageID);
    }

    public void OnReplayClick()
    {
        CUtils.CloseBannerAd();
        Sound.instance.PlayButton();
        CUtils.LoadScene(2);
    }

    public void UpdatePanels()
    {
        pauseBtn.SetActive(gameState == GAMESTATE.PLAYING);
        menuBtn.SetActive(gameState != GAMESTATE.PLAYING);

        pausePanel.SetActive(gameState == GAMESTATE.PAUSED);
        endGamePanel.gameObject.SetActive(gameState == GAMESTATE.ENDGAME);

        playBotPanel.SetActive(gameState == GAMESTATE.PLAYING);
        pauseBotPanel.SetActive(gameState == GAMESTATE.PAUSED);
        endGameBotPanel.SetActive(gameState == GAMESTATE.ENDGAME);
    }

    public bool CanMove(Brick brick, Vector2 delta)
    {
        foreach (BrickPart brickPart in brick.parts)
        {
            if (FindPart(brick.brickPos + brickPart.pos + delta, brick)) return false;
        }
        return true;
    }

    public List<int> GetClearRows()
    {
        int[] counts = new int[21];
        List<int> rows = new List<int>();
        foreach (Brick br in bricks)
        {
            foreach (BrickPart brickPart in br.parts)
            {
                int row = (int)(brickPart.pos.y + br.brickPos.y);
                if (row < 0 || row > 20) continue;
                counts[row]++;
                if (counts[row] == NUM_COL)
                    rows.Add(row);
            }
        }
        return rows;
    }

    public IEnumerator ClearRows(List<int> rows)
    { 
        //remove rows
        Sound.instance.Play(Sound.Others.ClearRow);
        foreach (int r in rows)
        {
            for(int i = bricks.Count - 1; i >= 0; i--)
            {
                bricks[i].RemovePart(r);
            }
        }
        foreach (Brick br in bricks)
        {
            br.UpdateBorder();
        }
        yield return new WaitForSeconds(0.4f);
        //move down
        foreach (Brick br in bricks)
        {
            foreach (BrickPart brickPart in br.parts)
            {
                int row = (int)(br.brickPos.y + brickPart.pos.y);
                int count = 0;
                foreach (int r in rows)
                {
                    if (row > r) count++;
                }
                brickPart.pos.y -= count;
                brickPart.GetComponent<RectTransform>().anchoredPosition = new Vector3(brickPart.pos.x * Brick.BRICK_SIZE, brickPart.pos.y * Brick.BRICK_SIZE);
            }
            br.UpdateCorner();
        }
        //update scores
        int newScore = rows.Count == 1 ? 1 : (rows.Count == 2 ? 3 : (rows.Count == 3 ? 5 : 8));
        clearedLine += rows.Count;
        if (gameMode == GameMode.CLASSIC_MODE)
        {
            AddScoreClassic(newScore);
            SpawnNextBrick(0.2f);
            checking = false;
        }
        else if (gameMode == GameMode.CHALLENGE_MODE)
        {
            AddScoreChallenge(newScore);
            if (staticBrick != null)
            {
                if (CheckAndFallBricks() == 0)
                {
                    CheckLiftUp();
                    checking = false;
                    SpawnNextBrick(0.2f);
                }
            }
            else
            {
                checking = false;
                ShowEndGame(false, true);
            }
        }
    }

    private void CheckLiftUp()
    {
        staticBrick.UpdateCorner();
        if (staticBrick.brickPos.y + staticBrick.topRight.y < LIFT_ROW - 1 && staticBrick.brickPos.y + staticBrick.botLeft.y < 0)
        {
            int r = Mathf.Min((int)(LIFT_ROW - 1 - staticBrick.brickPos.y - staticBrick.topRight.y), (int)(-staticBrick.brickPos.y - staticBrick.botLeft.y));
            StartCoroutine(LiftUp(r));
        }
    }

    private IEnumerator LiftUp(int row)
    {
        yield return new WaitForSeconds(0.2f);
        foreach (Brick br in bricks)
        {
            br.brickPos = new Vector2(br.brickPos.x, br.brickPos.y + row);
            br.UpdateAnchoredPos();
        }
    }

    public void AddScoreClassic(int newScore)
    {
        totalScore += newScore;
        scoreText.text = totalScore.ToString();
        goal += newScore;
        if (goal >= levelGoal)
        {
            currDiff++;
            int remainGoal = goal - levelGoal;
            levelText.text = currDiff.ToString();
            levelGoal = currDiff * 2 + 8;
            goal = remainGoal;
        }
        progressBar.sizeDelta = new Vector2(((float)goal / levelGoal) * 95, 38);
    }

    public void AddScoreChallenge(int newScore)
    {
        totalScore += newScore;
        scoreText.text = totalScore.ToString();
    }

    public void RemoveBrick(Brick brick)
    {
        bricks.Remove(brick);
        Destroy(brick.gameObject);
    }

    public bool CanRotate(Brick brick)
    {
        Vector2 rotatePoint = Brick. ROTATE_POINTS[brick.brickType];
        foreach (BrickPart brickPart in brick.parts)
        {
            Vector2 p = brickPart.pos - rotatePoint;
            Vector2 newP = brick.brickPos + new Vector2(p.y, -p.x) + rotatePoint;
            if (newP.y < 0 || FindPart(newP, brick)) return false;
        }
        return true;
    }

    public bool FindPart(Vector2 pos, Brick exceptBrick)
    {
        foreach (Brick br in bricks)
        {
            if (exceptBrick != br)
            {
                foreach (BrickPart part in br.parts)
                {
                    if (pos == br.brickPos + part.pos) return true;
                }
            }
        }
        return false;
    }

    public void OnBrickStopped()
    {
        checking = true;
        if (topRow < currentBrick.brickPos.y + currentBrick.topRight.y)
            topRow = (int)(currentBrick.brickPos.y + currentBrick.topRight.y);        
        Sound.instance.Play(Sound.Others.BrickStop);
        shadowBrick.DestroyAllPart();
        CheckAndClear();
    }    

    public void OnBrickStoppedFall()
    {
        numFalling--;
        if (numFalling == 0)
        {
            if (CheckAndFallBricks() == 0)
            {
                if (!CheckAndClear())
                {
                    CheckLiftUp();
                }
            }
        }
    }

    public int CheckAndFallBricks()
    {
        foreach (Brick br in bricks)
        {
            if (br.CheckAndFall()) numFalling++;
        }
        return numFalling;
    }

    private bool CheckAndClear()
    {
        List<int> rows = GetClearRows();
        if (rows.Count == 0)
        {
            if (currentBrick.brickPos.y + currentBrick.topRight.y >= 20)
            {
                ShowEndGame(gameMode == GameMode.CLASSIC_MODE, false);
            }
            else
            {
                SpawnNextBrick(0.2f);
            }
            checking = false;
        }
        else
        {
            StartCoroutine(ClearRows(rows));
        }
        return rows.Count > 0;
    }

    private void SpawnNextBrick(float time)
    {
        Timer.Schedule(this, time, () =>
        {
            SpawnABrick(nextBrick.brickType);
            RandomNextBrick();
        });
    }

    private void ShowEndGame(bool isClassic, bool complete = false)
    {
        gameState = GAMESTATE.ENDGAME;
        Sound.instance.Play(Sound.Others.EndGame);
        CUtils.ShowInterstitialAd();
        CUtils.ShowBannerAd();
        UpdatePanels();
        endGamePanel.UpdateInfo(currDiff, totalScore, clearedLine, isClassic ? (int)passTime : (int)remainTime, isClassic, complete, continueCount);
    }

    private void RandomNextBrick()
    {
        nextBrick.DestroyAllPart();
        nextBrick.SpawnBrick(next2Brick.brickType);
        Vector2 center = nextBrick.GetCenter();
        nextBrick.transform.localPosition = new Vector3((-center.x) * Brick.BRICK_SIZE / 2, (-center.y) * Brick.BRICK_SIZE / 2);

        next2Brick.DestroyAllPart();
        next2Brick.SpawnBrick(Random.Range(0, Brick.BRICK_POS.Length));
        Vector2 center2 = next2Brick.GetCenter();
        next2Brick.transform.localPosition = new Vector3((-center2.x) * Brick.BRICK_SIZE / 2, (-center2.y) * Brick.BRICK_SIZE / 2);
    }

    public bool IsPlaying()
    {
        return gameState == GAMESTATE.PLAYING;
    }

    public bool IsPaused()
    {
        return gameState == GAMESTATE.PAUSED;
    }
}
