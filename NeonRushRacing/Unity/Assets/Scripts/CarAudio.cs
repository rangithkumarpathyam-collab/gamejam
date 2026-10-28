using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Procedural racing engine audio synthesizer.
    /// Delivers realistic V8 combustion rumble, dynamic rev pitches, gear shift pauses,
    /// and turbo boost whine without requiring external audio asset files.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CarAudio : MonoBehaviour
    {
        [Header("Engine Parameters")]
        [SerializeField] private float idleRpm = 850f;
        [SerializeField] private float maxRpm = 7500f;
        [SerializeField] private float baseFrequency = 45f; // Deep V8 bass pulse
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 0.45f;

        private ArcadeCarController car;
        private AudioSource audioSource;

        // Audio Synthesis variables
        private double phase;
        private double turboPhase;
        private float currentRpm;
        private float samplingRate;

        private void Awake()
        {
            car = GetComponent<ArcadeCarController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.spatialBlend = 0.2f;
            audioSource.volume = 1f;

            samplingRate = AudioSettings.outputSampleRate;
            if (samplingRate <= 0) samplingRate = 48000f;
            currentRpm = idleRpm;
        }

        private void Start()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void Update()
        {
            if (car == null) return;

            float speedRatio = car.SpeedRatio;
            float targetRpm = Mathf.Lerp(idleRpm, maxRpm, speedRatio);

            // Add simulated gear rev fluctuations
            float gearRev = (speedRatio * 5f) % 1.0f;
            targetRpm = Mathf.Lerp(targetRpm * 0.7f, targetRpm * 1.15f, gearRev);

            // Smooth RPM transition
            currentRpm = Mathf.Lerp(currentRpm, targetRpm, Time.deltaTime * 6f);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (masterVolume <= 0.001f) return;

            double rpmRatio = (currentRpm - idleRpm) / (maxRpm - idleRpm);
            double engineFreq = baseFrequency + (rpmRatio * 180.0);
            double phaseIncrement = (engineFreq * 2.0 * System.Math.PI) / samplingRate;

            bool isBoosting = car != null && car.IsBoosting;
            double turboFreq = 1800.0 + (rpmRatio * 1200.0);
            double turboIncrement = (turboFreq * 2.0 * System.Math.PI) / samplingRate;

            for (int i = 0; i < data.Length; i += channels)
            {
                phase += phaseIncrement;
                if (phase > 2.0 * System.Math.PI) phase -= 2.0 * System.Math.PI;

                // V8 Engine Sound: Fundamental + Odd harmonics + rough saw pulse
                double fundamental = System.Math.Sin(phase);
                double harmonic2 = System.Math.Sin(phase * 2.0) * 0.5;
                double harmonic3 = System.Math.Sin(phase * 3.0) * 0.25;
                double harmonic4 = System.Math.Sin(phase * 4.0) * 0.15;
                double pulse = ((phase / System.Math.PI) - 1.0) * 0.35; // Sawtooth bite

                double engineWave = (fundamental + harmonic2 + harmonic3 + harmonic4 + pulse) * 0.28;

                // Turbo Whine (during boost)
                double turboWave = 0;
                if (isBoosting)
                {
                    turboPhase += turboIncrement;
                    if (turboPhase > 2.0 * System.Math.PI) turboPhase -= 2.0 * System.Math.PI;
                    turboWave = System.Math.Sin(turboPhase) * 0.12;
                }

                float sample = (float)((engineWave + turboWave) * masterVolume);

                for (int ch = 0; ch < channels; ch++)
                {
                    data[i + ch] += sample;
                }
            }
        }
    }
}
