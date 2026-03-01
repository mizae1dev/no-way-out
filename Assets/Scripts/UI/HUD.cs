using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text gameTimeText;
    [SerializeField] private Image noiseIndicator;
    [SerializeField] private Slider noiseSlider;

    private NoiseSystem noiseSystem;
    private GameManager gameManager;

    private void Start() {
        noiseSystem = FindObjectOfType<NoiseSystem>();
        gameManager = GameManager.Instance;
    }

    private void Update() {
        UpdateHUD();
    }

    private void UpdateHUD() {
        if (gameManager != null) {
            gameTimeText.text = FormatTime(gameManager.GameTime);
        }
        if (noiseSystem != null) {
            float currentNoise = noiseSystem.GetCurrentAverageNoise();
            noiseSlider.value = currentNoise;
            UpdateNoiseIndicatorColor(currentNoise);
        }
    }

    private void UpdateNoiseIndicatorColor(float noiseLevel) {
        if (noiseLevel < 0.4f) {
            noiseIndicator.color = Color.green;
        } else if (noiseLevel < 0.7f) {
            noiseIndicator.color = Color.yellow;
        } else {
            noiseIndicator.color = Color.red;
        }
    }

    private string FormatTime(float time) {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}