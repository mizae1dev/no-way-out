using UnityEngine;
using System.Collections.Generic;

public class NoiseSystem : MonoBehaviour {
    [System.Serializable]
    public class NoiseData {
        public float intensity;
        public Vector3 position;
        public float time;
        public float duration = 2f;
    }

    [Header("Noise Settings")]
    [SerializeField] private float noiseDecay = 0.5f;
    [SerializeField] private List<NoiseData> activeNoises = new List<NoiseData>();

    [Header("Noise Thresholds")]
    [SerializeField] private float investigateThreshold = 0.4f;
    [SerializeField] private float chaseThreshold = 0.7f;

    [Header("Microphone")]
    [SerializeField] private bool useMicrophone = true;
    [SerializeField] private float microphoneVolumeSensitivity = 0.8f;

    private AudioClip microphoneClip;
    private int microphoneSamplePosition;
    private EnemyAI enemyAI;

    private void Start() {
        enemyAI = FindObjectOfType<EnemyAI>();
        if (useMicrophone) StartMicrophoneInput();
    }

    private void Update() {
        UpdateNoises();
        if (useMicrophone) UpdateMicrophoneInput();
    }

    public void AddNoise(float intensity, Vector3 position) {
        intensity = Mathf.Clamp01(intensity);
        NoiseData noise = new NoiseData { intensity = intensity, position = position, time = Time.time };
        activeNoises.Add(noise);

        if (enemyAI != null) {
            if (intensity >= chaseThreshold) enemyAI.ChaseNoise(position, intensity);
            else if (intensity >= investigateThreshold) enemyAI.InvestigateNoise(position, intensity);
        }

        Debug.Log($"[NOISE] Intensity: {intensity} at {position}");
    }

    private void UpdateNoises() {
        for (int i = activeNoises.Count - 1; i >= 0; i--) {
            NoiseData noise = activeNoises[i];
            float age = Time.time - noise.time;
            if (age > noise.duration) {
                activeNoises.RemoveAt(i);
            }
        }
    }

    private void StartMicrophoneInput() {
        if (Microphone.devices.Length == 0) {
            Debug.LogWarning("[MICROPHONE] Nenhum microfone detectado!");
            useMicrophone = false;
            return;
        }
        string microphoneDevice = Microphone.devices[0];
        microphoneClip = Microphone.Start(microphoneDevice, true, 1, 44100);
        microphoneSamplePosition = 0;
        Debug.Log($"[MICROPHONE] Iniciado: {microphoneDevice}");
    }

    private void UpdateMicrophoneInput() {
        if (microphoneClip == null) return;
        int currentPosition = Microphone.GetPosition(null);
        if (currentPosition < microphoneSamplePosition) microphoneSamplePosition = 0;
        int sampleCount = currentPosition - microphoneSamplePosition;
        if (sampleCount <= 0) return;

        float[] samples = new float[sampleCount];
        microphoneClip.GetData(samples, microphoneSamplePosition);
        float averageVolume = 0f;
        foreach (float sample in samples) {
            averageVolume += Mathf.Abs(sample);
        }
        averageVolume /= sampleCount;

        float noiseIntensity = Mathf.Clamp01(averageVolume * microphoneVolumeSensitivity);
        if (noiseIntensity > 0.1f) {
            AddNoise(noiseIntensity, transform.position);
        }
        microphoneSamplePosition = currentPosition;
    }

    public float GetCurrentAverageNoise() {
        if (activeNoises.Count == 0) return 0f;
        float totalIntensity = 0f;
        foreach (NoiseData noise in activeNoises) {
            totalIntensity += noise.intensity;
        }
        return totalIntensity / activeNoises.Count;
    }

    public Vector3 GetLoudestNoisePosition() {
        if (activeNoises.Count == 0) return transform.position;
        NoiseData loudest = activeNoises[0];
        foreach (NoiseData noise in activeNoises) {
            if (noise.intensity > loudest.intensity) loudest = noise;
        }
        return loudest.position;
    }

    public List<NoiseData> GetActiveNoises() => new List<NoiseData>(activeNoises);
}