using UnityEngine;
using UnityEngine.UI;
using UomaWeb;

public class CoinIconScript_Tetris : MonoBehaviour
{
    public Text txtCoin;
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        txtCoin.text = $"{UomaDataManager.GetVirtualCurrency()}";
    }

}
