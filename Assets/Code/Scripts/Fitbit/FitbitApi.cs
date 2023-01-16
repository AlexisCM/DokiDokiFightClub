using System;
using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class FitbitApi : MonoBehaviour
{
    private const string _clientSecret = "";
    private const string _clientId = "";

    // URLs
    private const string _tokenUrl = "https://api.fitbit.com/oauth2/token";
    private const string _oAuth2Url = "https://www.fitbit.com/oauth2/authorize?";   // Url to request user auth from Fitbit API
    private const string _heartRateUrl = "https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json";
    private const string _callbackUrl = "https%3A%2F%2Falexiscm.github.io%2FDokiDokiFightClub%2Fauth%2Fddfc-game%2F";

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
    private Action<HeartRateTimeSeries> _onGetHeartRateSuccess;

    private OAuth2AccessToken _oAuth2;  // Represents JSON data returned from Fitbit auth request

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

    // Update is called once per frame
    void Update()
    {

    }

    public void LoginToFitbit()
    {
        // Check if the user has a refresh token stored in PlayerPrefs
        if (PlayerPrefs.HasKey("FitbitRefreshToken"))
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
        var refreshToken = PlayerPrefs.GetString("FitbitRefreshToken");

        // Create request body
        var form = new WWWForm();
        form.AddField("grant_type", "refresh_token");
        form.AddField("refresh_token", refreshToken);

        // Set request headers
        var request = UnityWebRequest.Post(_tokenUrl, form);
        request.SetRequestHeader("authorization", "Basic " + EncodeBasicToken());

        StartCoroutine(WaitForRefreshTokenRequest(request));
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
            HeartRateTimeSeries hrData = JsonConvert.DeserializeObject<HeartRateTimeSeries>(req.downloadHandler.text);
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
            PlayerPrefs.DeleteKey("FitbitRefreshToken");
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
        var authToken = "Bearer " + PlayerPrefs.GetString("FitbitAccessToken");
        var request = UnityWebRequest.Get(_heartRateUrl);
        request.SetRequestHeader("authorization", authToken);

        StartCoroutine(WaitForHeartRateData(request));
    }

    private void TokensHandler()
    {
        // Store Fitbit API tokens
        PlayerPrefs.SetString("FitbitAccessToken", _oAuth2.access_token);
        PlayerPrefs.SetString("FitbitRefreshToken", _oAuth2.refresh_token);
        PlayerPrefs.SetString("FitbitTokenType", _oAuth2.token_type);
        PlayerPrefs.SetInt("FitbitExpiresIn", _oAuth2.expires_in);

        GetHeartRateData();
    }

    private void FetchedDataHandler(HeartRateTimeSeries hrData)
    {
        Debug.Log(hrData.ToString());
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
}
