using UnityEngine;
using System;

public enum GamePhase { Menu, Flying, Result }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public static Action<GamePhase> OnPhaseChanged;
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Menu;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()    => SetPhase(GamePhase.Flying);
    public void EndMission()   => SetPhase(GamePhase.Result);
    public void ReturnToMenu() => SetPhase(GamePhase.Menu);

    void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }
}
