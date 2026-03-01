using UnityEngine;
public class MicrophoneInput : MonoBehaviour {
    [SerializeField] private float volumeThreshold = 0.1f;
    [SerializeField] private int audioFrequency = 44100;
    private AudioClip microphoneClip;
    private int lastSamplePosition;
    private float[] sampleBuffer = new float[1024];

    private void Start() {
        InitializeMicrophone();
    }

    private void InitializeMicrophone() {
        if (Microphone.devices.Length == 0) {
            Debug.LogError("[MICROPHONE] Nenhum dispositivo de microfone encontrado!");
            return;
        }
        string deviceName = Microphone.devices[0];
        microphoneClip = Microphone.Start(deviceName, true, 1, audioFrequency);
        Debug.Log($"[MICROPHONE] Inicializado com: {deviceName}");
    }

    public float GetCurrentVolume() {
        if (microphoneClip == null) return 0f;
        int currentPosition = Microphone.GetPosition(null);
        int sampleCount = currentPosition - lastSamplePosition;
        if (sampleCount <= 0) return 0f;
        microphoneClip.GetData(sampleBuffer, lastSamplePosition);
        float volume = 0f;
        foreach (float sample in sampleBuffer) {
            volume += Mathf.Abs(sample);
        }
        lastSamplePosition = currentPosition;
        return volume / sampleCount;
    }

    private void OnDestroy() {
        if (Microphone.IsRecording(null)) {
            Microphone.End(null);
        }
    }
}