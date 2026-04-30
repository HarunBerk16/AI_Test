using UnityEditor;
using UnityEngine;

public class ResetGameData
{
    [MenuItem("KamikazeGame/Reset Game Data")]
    public static void Execute()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Tum oyun verisi sifirlandı: Coin=0, Level=1, Upgrades=0");
    }
}
