using UnityEngine;
using System.Collections;

public class MonoUtils : MonoBehaviour {

    public static MonoUtils instance;
    public Sprite[] difficultySprites;
    private void Awake()
    {
        instance = this;
    }
}
