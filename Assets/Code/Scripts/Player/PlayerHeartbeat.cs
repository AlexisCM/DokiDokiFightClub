using Mirror;
using System.Collections;
using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerHeartbeat : NetworkBehaviour
    {
        [Header("SFX Settings")]
        [SerializeField]
        AudioSource _heartbeatSource;

        [SerializeField]
        float _defaultVolumePct = 0.3f;

        [SerializeField]
        float _defaultMinPitchPct = 1f;

        [Header("Player Heart Rate Values")]
        [SerializeField]
        int _currentHr = 80;

        private FitbitApi _fitbitApi;
        private HeartRateScaler _heartRateScaler;
        private int _restingHeartRate;

        public void Start()
        {
            _fitbitApi = FindObjectOfType<FitbitApi>();
            _heartRateScaler = new HeartRateScaler(_restingHeartRate, _defaultVolumePct);
            _heartbeatSource.volume = _defaultVolumePct;
            _heartbeatSource.pitch = _defaultMinPitchPct;

            _restingHeartRate = _fitbitApi.GetRestingHeartRate();

            // Heart rate SFX should only play from the remote player's object
            if (!isLocalPlayer)
                Initialize();
        }

        //public void Update()
        //{
        //    // TESTING! REMOVE LATER! ----------------------------------------------------
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        UpdateHeartRate();
        //    }
        //}

        private void Initialize()
        {
            StartCoroutine(PlayHeartbeatSfx());
        }

        /// <summary>Retrieve updated heart rate data to convert into audio effects.</summary>
        public void UpdateHeartRate()
        {
            // TODO: Set HR data in Start method once script is moved onto Networked Player Object
            _restingHeartRate = _fitbitApi.GetRestingHeartRate();
            Debug.Log($"<color=green>Resting HR = {_restingHeartRate}bpm</color>");

            _currentHr = _fitbitApi.GetCurrentHeartRate();
            ConvertAudioLevels(_currentHr);
        }

        private void ConvertAudioLevels(int updatedHeartRate)
        {
            var mappedAudioLevel = _heartRateScaler.MapToAudioLevel(updatedHeartRate);
            var mappedPlaybackSpeed = _heartRateScaler.MapToPlaybackSpeed(updatedHeartRate);
            _heartbeatSource.volume = mappedAudioLevel;
            _heartbeatSource.pitch = mappedPlaybackSpeed;
        }

        private IEnumerator PlayHeartbeatSfx()
        {
            while (_heartbeatSource.clip.loadState != AudioDataLoadState.Loaded)
                yield return null;
            _heartbeatSource.Play();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// Scales the heart rate value between the desired targets based on what the its expected min. and max. measurements.
    /// </summary>
    internal class HeartRateScaler
    {
        readonly int _measuredMin = 50; // Minimum range of the measured Heart Rate
        readonly int _measuredMax = 150; // Maximum range of the measured Heart Rate
        readonly float _targetMinVolume = 0.5f; // Minimum Range of the target scaling (Volume)
        readonly float _targetMaxVolume = 1.0f; // Maximum Range of the target scaling (Volume)
        readonly float _targetMinPitch = 1f; // Minimum Range of the target scaling (Pitch)
        readonly float _targetMaxPitch = 1.9f; // Maximum Range of the target scaling (Pitch)

        /// <param name="measuredMin">Minimum range of the measured Heart Rate. Set this value to the player's Resting Heart Rate.</param>
        /// <param name="targetMin">Minimum Range of the target scaling (Volume). Set this value to the default Heart Rate SFX volume.</param>
        public HeartRateScaler(int measuredMin, float targetMin)
        {
            _measuredMin = measuredMin;
            _targetMinVolume = targetMin;
        }

        /// <summary>Scales the heartRate to an inverval of the minimum and maximum Volume levels.</summary>
        /// <remarks><see href="https://stats.stackexchange.com/questions/281162/scale-a-number-between-a-range">Formula Credit: Stephan Kolassa</see></remarks>
        /// <param name="heartRate">The float value to scale</param>
        /// <returns>The heart rate mapped to the appropriate volume level (float)</returns>
        public float MapToAudioLevel(float heartRate)
        {
            float scaled;
            scaled = (heartRate - _measuredMin) / (_measuredMax - _measuredMin);
            scaled *= _targetMaxVolume - _targetMinVolume;
            scaled += _targetMinVolume;
            return scaled;
        }

        public float MapToPlaybackSpeed(float heartRate)
        {
            float scaled;
            scaled = (heartRate - _measuredMin) / (_measuredMax - _measuredMin);
            scaled *= _targetMaxPitch - _targetMinPitch;
            scaled += _targetMinPitch;
            return scaled;
        }
    }
}
