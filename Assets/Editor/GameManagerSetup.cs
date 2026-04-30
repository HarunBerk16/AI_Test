using UnityEditor;
using UnityEngine;

public class GameManagerSetup
{
    public static void Execute()
    {
        // GameManager objesi
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<ExplosionManager>();

        Debug.Log("GameManager olusturuldu!");
    }
}
