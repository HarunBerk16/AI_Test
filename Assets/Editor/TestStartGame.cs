using UnityEngine;

public class TestStartGame
{
    public static void Execute()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) { Debug.LogError("[Test] GameStateManager yok!"); return; }
        gsm.StartGame();
        Debug.Log("[Test] StartGame çağrıldı, phase: " + gsm.CurrentPhase);
    }
}
