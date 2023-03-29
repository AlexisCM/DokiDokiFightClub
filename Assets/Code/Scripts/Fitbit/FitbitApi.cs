using System;
using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace DokiDokiFightClub
{
    struct InitialData
    {
        public float RestingHeartRate;
    }

    public class FitbitApi : MonoBehaviour
    {
        /// <summary>Default value to use when Fitbit API fails to retrieve data.</summary>
        public int DefaultRestingHr = 60;

        private static readonly string FITBIT_ACCESS_TOKEN_KEY = "FitbitAccessToken";
        private static readonly string FITBIT_REFRESH_TOKEN_KEY = "FitbitRefreshToken";
        private static readonly string FITBIT_TOKEN_TYPE_KEY = "FitbitTokenType";
        private static readonly string FITBIT_EXPIRES_IN_KEY = "FitbitExpiresIn";

        private const string _clientSecret = "";
        private const string _clientId = "";

        // URLs
        private const string _tokenUrl = "https://api.fitbit.com/oauth2/token";
        private const string _oAuth2Url = "https://www.fitbit.com/oauth2/authorize?";   // Url to request user auth from Fitbit API
        private const string _callbackUrl = "https%3A%2F%2Falexiscm.github.io%2FDokiDokiFightClub%2Fauth%2Fddfc-game%2F";
        // Base URL to access Intraday Heart Rate Data. Must concatenate "{0}/{1}.json", where {0} and {1} represent the data's timespan.
        private const string _baseHeartRateUrl = "https://api.fitbit.com/1/user/-/activities/heart/date/today/today/1sec/time/";

        // URI arguments
        private const string _codeChallMethod = "S256";
        // Proof of Key for Code Exchance (PKCE) Code Verifier:
        private const string _codeVerifier = "";
        private const string _codeChallenge = ""; // Base64-encoded SHA-256 transformation of Code Verifier
        private const string _state = "";
        private const string _scopeParams = "heartrate+sleep";

        // Response Codes
        private string _returnCode; // The code returned from API after successful call

        // Event Handlers
        private Action _onRequestCompletion;
        private Action<FitbitHeartRateData> _onGetHeartRateSuccess;

        private OAuth2AccessToken _oAuth2;  // Represents JSON data returned from Fitbit auth request

        private InitialData _initialData; // Holds the player's initially retrieved heart rate data prior to match-making

        private void Awake()
        {
            // Subscribe methods to event handlers
            _onGetHeartRateSuccess += FetchedDataHandler;
        }

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void LoginToFitbit()
        {
            // Check if the user has a refresh token stored in PlayerPrefs
            if (PlayerPrefs.HasKey(FITBIT_REFRESH_TOKEN_KEY))
            {
                UseRefreshToken();
            }
            else
            {
                AuthorizeFitbitUser();
            }
        }

        /// <summary>
        /// Direct user to the browser, where they must login to fitbit account and authorize use of data.
        /// </summary>
        public void AuthorizeFitbitUser()
        {
            var authorizationUrl = $"{_oAuth2Url}&client_id={_clientId}&response_type=code&code_challenge={_codeChallenge}&" +
                $"code_challenge_method={_codeChallMethod}&state={_state}&scope={_scopeParams}&redirect_uri={_callbackUrl}";

            Application.OpenURL(authorizationUrl);
        }

        public void GetTokensWithReturnCode()
        {
            // Get user-inputted return code from input field
            var returnCodeObj = GameObject.Find("Canvas/CodeEntryField");
            var returnCodeInput = returnCodeObj.GetComponent<TMP_InputField>();
            _returnCode = returnCodeInput.text.ToString();

            // Create request body
            var form = new WWWForm();
            form.AddField("client_id", _clientId);
            form.AddField("grant_type", "authorization_code");
            form.AddField("redirect_uri", UnityWebRequest.UnEscapeURL(_callbackUrl));
            form.AddField("code_verifier", _codeVerifier);
            form.AddField("code", _returnCode);

            // Create request obj and set headers
            var request = UnityWebRequest.Post(_tokenUrl, form);
            request.SetRequestHeader("authorization", "Basic " + EncodeBasicToken());

            StartCoroutine(WaitForRequest(request));
        }

        public void UseRefreshToken()
        {
            var refreshToken = PlayerPrefs.GetString(FITBIT_REFRESH_TOKEN_KEY);

            // Create request body
            var form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", refreshToken);

            // Set request headers
            var request = UnityWebRequest.Post(_tokenUrl, form);
            request.SetRequestHeader("authorization", "Basic " + EncodeBasicToken());

            StartCoroutine(WaitForRefreshTokenRequest(request));
        }

        /// <summary>Returns the user's resting HR via Fitbit API. Should that fail, return a default value.</summary>
        /// <returns></returns>
        public int GetRestingHeartRate()
        {
            int value = (int)Mathf.Round(_initialData.RestingHeartRate);
            int restingHr = value == 0 ? DefaultRestingHr : value;
            return restingHr;
        }

        IEnumerator WaitForRequest(UnityWebRequest req)
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"REQUEST FAILED: {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                _oAuth2 = JsonUtility.FromJson<OAuth2AccessToken>(req.downloadHandler.text);
                // Perform callback on success
                _onRequestCompletion += TokensHandler;
                _onRequestCompletion.Invoke();
                _onRequestCompletion = null;
            }
        }

        IEnumerator WaitForHeartRateData(UnityWebRequest req)
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"REQUEST FAILED: {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                FitbitHeartRateData hrData = JsonConvert.DeserializeObject<FitbitHeartRateData>(req.downloadHandler.text);
                Debug.Log(req.downloadHandler.text);
                _onGetHeartRateSuccess.Invoke(hrData);
            }
        }

        IEnumerator WaitForRefreshTokenRequest(UnityWebRequest req)
        {
            yield return req.SendWebRequest();


            if (req.result != UnityWebRequest.Result.Success)
            {
                // Something went wrong with the request
                if (req.responseCode != 401)
                    Debug.Log($"REQUEST FAILED: {req.error}\n{req.downloadHandler.text}");

                // Refresh token didn't exist or has expired
                PlayerPrefs.DeleteKey(FITBIT_REFRESH_TOKEN_KEY);
                _onRequestCompletion += AuthorizeFitbitUser;
                _onRequestCompletion.Invoke(); // Call AuthorizeFitbitUser to generate new tokens
                _onRequestCompletion = null;
            }
            else
            {
                // Request succeeded with refresh token
                _oAuth2 = JsonUtility.FromJson<OAuth2AccessToken>(req.downloadHandler.text);
                _onRequestCompletion += TokensHandler;
                _onRequestCompletion.Invoke();
                _onRequestCompletion = null;
            }
        }

        private void GetHeartRateData()
        {
            var authToken = "Bearer " + PlayerPrefs.GetString(FITBIT_ACCESS_TOKEN_KEY);
            var request = UnityWebRequest.Get(GetHeartRateIntradayUrl());
            request.SetRequestHeader("authorization", authToken);

            StartCoroutine(WaitForHeartRateData(request));
        }

        private void TokensHandler()
        {
            // Store Fitbit API tokens
            PlayerPrefs.SetString(FITBIT_ACCESS_TOKEN_KEY, _oAuth2.access_token);
            PlayerPrefs.SetString(FITBIT_REFRESH_TOKEN_KEY, _oAuth2.refresh_token);
            PlayerPrefs.SetString(FITBIT_TOKEN_TYPE_KEY, _oAuth2.token_type);
            PlayerPrefs.SetInt(FITBIT_EXPIRES_IN_KEY, _oAuth2.expires_in);

            GetHeartRateData();
        }

        /// <summary>Allows data to be managed after being fetched from Fitbit API.</summary>
        /// <param name="hrData"></param>
        private void FetchedDataHandler(FitbitHeartRateData hrData)
        {
            Debug.Log(hrData.ToString());
            _initialData.RestingHeartRate = float.Parse(hrData.ActivitiesHearts[0].value);
        }

        /// <summary>
        /// Returns a Base64 encoded string of the application's client id and secret concatenated with a colon.
        /// Decoded version is "client_id:client secret".
        /// </summary>
        /// <returns></returns>
        private string EncodeBasicToken()
        {
            byte[] plainTextBytesToken = System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
            return Convert.ToBase64String(plainTextBytesToken);
        }

        /// <summary>
        /// Return the 
        /// </summary>
        /// <param name="timespan">Time interval</param>
        /// <returns>Complete, formatted URL</returns>
        private string GetHeartRateIntradayUrl()
        {
            var now = DateTime.Now.AddMinutes(-1.0);
            var endTime = now;
            var startTime = now.AddMinutes(-2.0); // Data should be retrieved from within the last minute
            return $"{_baseHeartRateUrl}{startTime:HH:mm}/{endTime:HH:mm}.json";
        }
    }
}
