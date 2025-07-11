using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Brick : MonoBehaviour {
    public static Vector2[][] BRICK_POS =
        {
            new Vector2[] { new Vector2(0, -1),new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, -1) }, //L
            new Vector2[] { new Vector2(0, -1),new Vector2(0, 0), new Vector2(0, 1), new Vector2(-1, -1) }, //RL
            new Vector2[] { new Vector2(-1, 0),new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, -1) }, //Z
            new Vector2[] { new Vector2(1, 0),new Vector2(0, 0), new Vector2(0, -1), new Vector2(-1, -1) }, //RZ
            new Vector2[] { new Vector2(-1, 0),new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1) }, //T
            new Vector2[] { new Vector2(0, -1),new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 2) }, //I
            new Vector2[] { new Vector2(0, 0),new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) }, //Square
        };

    public static Vector2[] ROTATE_POINTS = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)};
    public static Vector2[] CENTER_POINTS = new Vector2[] { new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(1, 1) };
    public static readonly int BRICK_SIZE = 40, BORDER_SIZE = 2;
    public Sprite[] sprites;
    [HideInInspector]
    public Vector2 brickPos;
    [HideInInspector]
    public float speed, normalSpeed, leftSpeed, rightSpeed;
    public BrickPart partPrefab;
    public GameObject destroyPartPrefab;
    public GameObject[] borderPrefabs, cornerPrefabs;
    public Transform partRegion, frameRegion;
    public Action onStopped;
    private double lastMoveDownTime, lastMoveTime, lastMoveLeftTime, lastMoveRightTime;
    [HideInInspector]
    public int brickType;
    public bool movable;
    [HideInInspector]
    public bool falling;
    private bool leftPress = false, rightPress = false, leftFirst = true, rightFirst = true;
    [HideInInspector]
    public List<BrickPart> parts = new List<BrickPart>();
    [HideInInspector]
    public List<GameObject> borders = new List<GameObject>();
    [HideInInspector]
    public Vector2 botLeft, topRight;
    private bool isShadow;

    public void SpawnBrick(int type, bool shadow = false)
    {
        brickType = type;
        isShadow = shadow;
        foreach (Vector2 vt in BRICK_POS[type])
        {
            BrickPart part = (BrickPart)Instantiate(partPrefab);
            part.transform.SetParent(partRegion);
            part.transform.localScale = Vector3.one;
            part.image.sprite = isShadow ? sprites[7] : sprites[type];
            part.image.SetColorAlpha(isShadow ? 0.2f : 1);
            part.frame.SetActive(!isShadow);
            part.GetComponent<RectTransform>().anchoredPosition = new Vector3(vt.x * BRICK_SIZE, vt.y * BRICK_SIZE);
            part.pos = vt;
            parts.Add(part);
        }
        UpdateCorner();
    }

    public void SpawnStaticBrick(List<Vector2> positions)
    {
        brickType = 8;
        foreach (Vector2 vt in positions)
        {
            BrickPart part = (BrickPart)Instantiate(partPrefab);
            part.transform.SetParent(partRegion);
            part.transform.localScale = Vector3.one;
            part.image.sprite = sprites[brickType];
            part.image.color = Color.gray;
            part.frame.SetActive(false);
            part.GetComponent<RectTransform>().anchoredPosition = new Vector3(vt.x * BRICK_SIZE, vt.y * BRICK_SIZE);
            part.pos = vt;
            parts.Add(part);
        }
        UpdateCorner();
    }

    public void Start()
    {
        lastMoveDownTime = 0;
        lastMoveTime = 0;
        speed = normalSpeed;
        falling = false;
    }

    public bool CheckAndFall()
    {
        if (!falling && (brickPos.y + botLeft.y > 0) && MainController.instance.CanMove(this, Vector2.down))
        {
            falling = true;
            speed = 0.005f;
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (!MainController.instance.IsPlaying() || isShadow) return;
        int step = Mathf.Max(1, (int)(Time.deltaTime / speed));
        for (int i = 0; i < step; i++)
        {
            if (movable || falling)
            {
                //move down
                if ((brickPos.y + botLeft.y > 0) && MainController.instance.CanMove(this, Vector2.down))
                {
                    if (CUtils.GetCurrentTime() - lastMoveDownTime > speed)
                    {
                        MoveDown();
                        lastMoveDownTime = CUtils.GetCurrentTime();
                    }
                }
                else if (CUtils.GetCurrentTime() - lastMoveTime > (falling ? 0.05f : 0.5f)) //Brick stopped
                {
                    MainController.instance.holdable = false;
                    if (falling)
                        OnStoppedFall();
                    else
                        StartCoroutine(ShowStopHighlight());
                    break;
                }
            }
            if (movable)
            {
                //move left
                if (leftPress && (leftFirst || CUtils.GetCurrentTime() - lastMoveLeftTime > leftSpeed)
                    && brickPos.x + botLeft.x > 0 && MainController.instance.CanMove(this, Vector2.left))
                {
                    leftSpeed = leftFirst ? 0.2f : 0.1f;
                    leftFirst = false;
                    MoveLeft();
                    lastMoveLeftTime = CUtils.GetCurrentTime();
                }
                //move right
                if (rightPress && (rightFirst || CUtils.GetCurrentTime() - lastMoveRightTime > rightSpeed)
                    && brickPos.x + topRight.x < MainController.NUM_COL - 1 && MainController.instance.CanMove(this, Vector2.right))
                {
                    rightSpeed = rightFirst ? 0.2f : 0.1f;
                    rightFirst = false;
                    MoveRight();
                    lastMoveRightTime = CUtils.GetCurrentTime();
                }
            }
        }
    }

    private void OnStoppedFall()
    {
        falling = false;
        MainController.instance.OnBrickStoppedFall();
    }

    public IEnumerator ShowStopHighlight()
    {
        movable = false;
        if (gameObject.activeSelf)
        {
            foreach (BrickPart part in parts)
            {
                part.image.sprite = sprites[7];
                part.image.SetColorAlpha(0.6f);
            }
            yield return new WaitForSeconds(0.2f);
            foreach (BrickPart part in parts)
            {
                part.image.sprite = sprites[brickType];
                part.image.SetColorAlpha(1f);
            }
            if (onStopped != null) onStopped();
        }
    }

    public void DestroyAllPart()
    {
        for(int i = parts.Count - 1; i >=0; i--)
        {
            Destroy(parts[i].gameObject);
        }
        parts.Clear();
    }

    public void MoveLeft()
    {
        Sound.instance.Play(Sound.Others.Move);
        lastMoveTime = CUtils.GetCurrentTime();
        brickPos.x--;
        UpdateBrickPos();
    }

    public void MoveRight()
    {
        Sound.instance.Play(Sound.Others.Move);
        lastMoveTime = CUtils.GetCurrentTime();
        brickPos.x++;
        UpdateBrickPos();
    }

    bool flashed = false;
    public void MoveFlash()
    {
        if (!flashed)
        {
            flashed = true;
            Sound.instance.Play(Sound.Others.FastMove);
            speed = 0.0005f;
        }
    }

    public void StartMoveFast()
    {
        speed = 0.05f * normalSpeed;
    }

    public void StopMoveFast()
    {
        speed = normalSpeed;
    }

    public void StartMoveLeft()
    {
        leftPress = true;
    }

    public void StopMoveLeft()
    {
        leftPress = false;
        leftFirst = true;
    }

    public void StartMoveRight()
    {
        rightPress = true;
    }

    public void StopMoveRight()
    {
        rightPress = false;
        rightFirst = true;
    }

    public void RemovePart(int row)
    {
        List<int> removeIndexes = new List<int>();
        int i = 0;
        foreach (BrickPart part in parts)
        {
            if (row == part.pos.y + brickPos.y)
            {
                removeIndexes.Add(i);
            }
            i++;
        }
        for (int j = removeIndexes.Count - 1; j >= 0; j--)
        {
            ShowDestroyEffect(parts[removeIndexes[j]].image.transform.position);
            Destroy(parts[removeIndexes[j]].gameObject);
            parts.RemoveAt(removeIndexes[j]);
        }
        if (parts.Count == 0) MainController.instance.RemoveBrick(this);
    }

    public void ShowDestroyEffect(Vector3 pos)
    {
        GameObject part = (GameObject)Instantiate(destroyPartPrefab);
        part.transform.SetParent(transform.parent);
        part.transform.localScale = Vector3.one;
        part.GetComponent<Image>().sprite = sprites[brickType];
        part.transform.position = pos;
    }

    public void Rotate()
    {
        if (movable && MainController.instance.CanRotate(this))
        {
            Sound.instance.Play(Sound.Others.Rotate);
            List<Vector2> savedPos = new List<Vector2>();
            Vector2 savedBrickPos = new Vector2(brickPos.x, brickPos.y);
            lastMoveTime = CUtils.GetCurrentTime();
            Vector2 rotatePoint = ROTATE_POINTS[brickType];
            foreach (BrickPart part in parts)
            {
                savedPos.Add(new Vector2(part.pos.x, part.pos.y));
                Vector2 p = part.pos - rotatePoint;
                Vector2 newP = new Vector2(p.y, -p.x);
                part.pos = newP + rotatePoint;
                part.GetComponent<RectTransform>().anchoredPosition = new Vector3(part.pos.x * BRICK_SIZE, part.pos.y * BRICK_SIZE);
            }
            UpdateCorner();
            if (brickPos.x + botLeft.x < 0)
            {
                brickPos.x -= (brickPos.x + botLeft.x);
            }
            if (brickPos.x + topRight.x > MainController.NUM_COL - 1)
            {
                brickPos.x -= (brickPos.x + topRight.x - 9);
            }
            //Rotate revert if errored
            if (!MainController.instance.CanMove(this, Vector2.zero))
            {
                brickPos = savedBrickPos;
                int i = 0;
                foreach (BrickPart part in parts)
                {
                    part.pos = savedPos[i];
                    part.GetComponent<RectTransform>().anchoredPosition = new Vector3(part.pos.x * BRICK_SIZE, part.pos.y * BRICK_SIZE);
                    i++;
                }
                UpdateCorner();
            }
            UpdateBrickPos();
        }
    }

    public void UpdateCorner()
    {
        botLeft = parts[0].pos;
        topRight = parts[0].pos;
        foreach (BrickPart part in parts)
        {
            if (part.pos.x < botLeft.x) botLeft.x = part.pos.x;
            if (part.pos.y < botLeft.y) botLeft.y = part.pos.y;
            if (part.pos.x > topRight.x) topRight.x = part.pos.x;
            if (part.pos.y > topRight.y) topRight.y = part.pos.y;
        }
        UpdateBorder();
    }

    private Vector2[] TOP_LEFT_BOT_RIGHT = { new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0) };
    private Vector3[] BORDER_POSITIONS = { new Vector3(0, BRICK_SIZE - BORDER_SIZE), new Vector3(0, 0), new Vector3(0, 0), new Vector3(BRICK_SIZE - BORDER_SIZE, 0) };
    private Vector3[] CORNER_POSITIONS = { new Vector3(0, BRICK_SIZE - BORDER_SIZE), new Vector3(0, 0), new Vector3(BRICK_SIZE - BORDER_SIZE, 0), new Vector3(BRICK_SIZE - BORDER_SIZE, BRICK_SIZE - BORDER_SIZE) };

    public void UpdateBorder()
    {
        if (!isShadow && brickType != 8)
        {
            foreach (GameObject bd in borders)
            {
                Destroy(bd);
            }
            borders.Clear();
            foreach (BrickPart part in parts)
            {
                for (int i = 0; i < TOP_LEFT_BOT_RIGHT.Length; i++)
                {
                    if (!HasPart(part, TOP_LEFT_BOT_RIGHT[i]))
                    {
                        GameObject bd = (GameObject)Instantiate(borderPrefabs[i]);
                        bd.transform.SetParent(frameRegion);
                        bd.transform.localScale = Vector3.one;
                        bd.GetComponent<RectTransform>().anchoredPosition = new Vector3(part.pos.x * BRICK_SIZE, part.pos.y * BRICK_SIZE) + BORDER_POSITIONS[i];
                        borders.Add(bd);
                    }
                }
            }
            //out corner
            foreach (BrickPart part in parts)
            {
                for (int i = 0; i < TOP_LEFT_BOT_RIGHT.Length; i++)
                {
                    if (HasOutCorner(part, TOP_LEFT_BOT_RIGHT[i], TOP_LEFT_BOT_RIGHT[(i + 1) % 4]))
                    {
                        GameObject cn = (GameObject)Instantiate(cornerPrefabs[4 + i]);
                        cn.transform.SetParent(frameRegion);
                        cn.transform.localScale = Vector3.one;
                        cn.GetComponent<RectTransform>().anchoredPosition = new Vector3(part.pos.x * BRICK_SIZE, part.pos.y * BRICK_SIZE) + CORNER_POSITIONS[i];
                        borders.Add(cn);
                    }
                }
            }
            //in corner
            foreach (BrickPart part in parts)
            {
                for (int i = 0; i < TOP_LEFT_BOT_RIGHT.Length; i++)
                {
                    if (HasInCorner(part, TOP_LEFT_BOT_RIGHT[i], TOP_LEFT_BOT_RIGHT[(i + 1) % 4]))
                    {
                        GameObject cn = (GameObject)Instantiate(cornerPrefabs[i]);
                        cn.transform.SetParent(frameRegion);
                        cn.transform.localScale = Vector3.one;
                        cn.GetComponent<RectTransform>().anchoredPosition = new Vector3(part.pos.x * BRICK_SIZE, part.pos.y * BRICK_SIZE) + CORNER_POSITIONS[i];
                        borders.Add(cn);
                    }
                }
            }
        }
    }

    private bool HasPart(BrickPart p, Vector2 side)
    {
        foreach (BrickPart part in parts)
        {
            if(part != p)
            {
                if (p.pos + side == part.pos) return true;
            }
        }
        return false;
    }

    private bool HasOutCorner(BrickPart p, Vector2 side1, Vector2 side2)
    {
        return !HasPart(p, side1) && !HasPart(p, side2);
    }

    private bool HasInCorner(BrickPart p, Vector2 side1, Vector2 side2)
    {
        return HasPart(p, side1) && HasPart(p, side2);
    }

    public Vector2 GetCenter()
    {
        return CENTER_POINTS[brickType];
    }

    public void MoveDown()
    {
        lastMoveTime = CUtils.GetCurrentTime();
        brickPos.y--;
        UpdateBrickPos();
    }

    public void UpdateBrickPos()
    {
        UpdateAnchoredPos();
        if(!falling)
            MainController.instance.UpdateShadow();
    }

    public void UpdateAnchoredPos()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector3(brickPos.x * BRICK_SIZE, brickPos.y * BRICK_SIZE);
    }
}
