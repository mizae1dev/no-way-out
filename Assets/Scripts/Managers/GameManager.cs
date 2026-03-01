using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public enum GameState { Playing, Paused, GameOver, Victory }
    private GameState currentState = GameState.Playing;

    [Header("Objectives")]
    private Dictionary<string, bool> objectives = new Dictionary<string, bool> {
        { "RestoreGenerator1", false },
        { "RestoreGenerator2", false },
        { "RestoreGenerator3", false },
        { "ObtainAccessCard1", false },
        { "ObtainAccessCard2", false },
        { "CollectInfoFile1", false },
        { "CollectInfoFile2", false },
        { "CollectInfoFile3", false },
        { "RepairEvacuation", false },
        { "ReachExit", false }
    };

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float gameTime = 0f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (currentState == GameState.Playing) {
            gameTime += Time.deltaTime;
        }
    }

    public void CompleteObjective(string objectiveKey) {
        if (objectives.ContainsKey(objectiveKey)) {
            objectives[objectiveKey] = true;
            Debug.Log($"[OBJECTIVE] {objectiveKey} completado!");
            CheckVictoryConditions();
        }
    }

    public bool IsObjectiveComplete(string objectiveKey) {
        return objectives.ContainsKey(objectiveKey) && objectives[objectiveKey];
    }

    private void CheckVictoryConditions() {
        if (objectives["RestoreGenerator1"] && objectives["RestoreGenerator2"] && objectives["RestoreGenerator3"] && objectives["RepairEvacuation"] && objectives["ReachExit"]) {
            WinGame();
        }
    }

    public void WinGame() {
        currentState = GameState.Victory;
        Debug.Log("[GAME] Jogador venceu!");
        Time.timeScale = 0f;
    }

    public void LoseGame() {
        currentState = GameState.GameOver;
        Debug.Log("[GAME] Game Over!");
        Time.timeScale = 0f;
    }

    public GameState CurrentState => currentState;
    public float GameTime => gameTime;
}