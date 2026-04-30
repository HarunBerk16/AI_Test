using UnityEditor;
using UnityEngine;

public class AddGameInitializer
{
    public static void Execute()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager bulunamadi!"); return; }

        if (gm.GetComponent<GameInitializer>() == null)
        {
            gm.AddComponent<GameInitializer>();
            Debug.Log("GameInitializer eklendi.");
        }
        else
        {
            Debug.Log("GameInitializer zaten mevcut.");
        }
    }
}
