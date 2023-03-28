using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerHeartbeat : MonoBehaviour
    {
        [SerializeField]
        AudioSource _heartbeatSource;

        readonly float _defaultVolumePct = 0.5f;

        [SerializeField]
        int currentHr = 80;

        private FitbitApi _fitbitApi;
        private HeartRateScaler _heartRateScaler;
        private int _restingHeartRate;

        public void Start()
        {
            _fitbitApi = FindObjectOfType<FitbitApi>();
            _heartRateScaler = new HeartRateScaler(_restingHeartRate, _defaultVolumePct);
            _heartbeatSource.volume = _defaultVolumePct;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UpdateHeartRate();
            }
        }

        /// <summary>Retrieve updated heart rate data to convert into audio effects.</summary>
        public void UpdateHeartRate()
        {
            // TODO: Set HR data in Start method once script is moved onto Networked Player Object
            _restingHeartRate = _fitbitApi.GetRestingHeartRate();
            Debug.Log($"<color=green>Resting HR = {_restingHeartRate}bpm</color>");
            // TODO: Get HR data using Fitbit API
            ConvertAudioLevels(currentHr);
        }

        private void ConvertAudioLevels(int updatedHeartRate)
        {
            var mappedAudioLevel = _heartRateScaler.MapToAudioLevel(updatedHeartRate);
            _heartbeatSource.volume = mappedAudioLevel;
        }
    }

    internal class HeartRateScaler
    {
        readonly int _measuredMin = 50; // Minimum range of the measured Heart Rate
        readonly int _measuredMax = 150; // Maximum range of the measured Heart Rate
        readonly float _targetMin = 0.5f; // Minimum Range of the target scaling (Volume)
        readonly float _targetMax = 1.0f; // Maximum Range of the target scaling (Volume)

        /// <param name="measuredMin">Minimum range of the measured Heart Rate. Set this value to the player's Resting Heart Rate.</param>
        /// <param name="targetMin">Minimum Range of the target scaling (Volume). Set this value to the default Heart Rate SFX volume.</param>
        public HeartRateScaler(int measuredMin, float targetMin)
        {
            _measuredMin = measuredMin;
            _targetMin = targetMin;
        }

        /// <summary>Scales the heartRate to an inverval of the minimum and maximum Volume levels.</summary>
        /// <remarks><see href="https://stats.stackexchange.com/questions/281162/scale-a-number-between-a-range">Formula Credit: Stephan Kolassa</see></remarks>
        /// <param name="heartRate">The float value to scale</param>
        /// <returns>The heart rate mapped to the appropriate volume level (float)</returns>
        public float MapToAudioLevel(float heartRate)
        {
            float scaled;
            scaled = (heartRate - _measuredMin) / (_measuredMax - _measuredMin);
            scaled *= _targetMax - _targetMin;
            scaled += _targetMin;

            Debug.Log($"Mapped HR Value = {scaled}");
            return scaled;
        }
    }
}
