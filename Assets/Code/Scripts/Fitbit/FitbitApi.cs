using Newtonsoft.Json;
using System;
using System.Collections;
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
    public Action<OAuth2AccessToken> OnRequestSuccess;
    public Action<HeartRateTimeSeries> OnGetHeartRateSuccess;

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
        AuthorizeFitbitUser();
        // Check if the user has a refresh token stored in PlayerPrefs
        //if (PlayerPrefs.HasKey("FitbitRefreshToken"))
        //{
        //    UseRefreshToken();
        //}
        //else
        //{
        //    AuthorizeFitbitUser();
        //}
    }

    public void UseRefreshToken()
    {
        string refreshToken = PlayerPrefs.GetString("FitbitRefreshToken");
    }

    public void AuthorizeFitbitUser()
    {
        var authorizationUrl = $"{_oAuth2Url}&client_id={_clientId}&response_type=code&code_challenge={_codeChallenge}&" +
            $"code_challenge_method={_codeChallMethod}&state={_state}&scope={_scopeParams}&redirect_uri={_callbackUrl}";
        //var authorizationUrl = $"{_oAuth2Uri}&client_id={_clientId}&response_type=code&scope={_scopeParams}&redirect_uri={_callbackUrl}";

        Application.OpenURL(authorizationUrl);
    }

    public void GetData()
    {
    }

    public void UseReturnCode()
    {
        // Get user-inputted return code from input field
        GameObject returnCodeObj = GameObject.Find("Canvas/CodeEntryField");
        TMP_InputField returnCodeInput = returnCodeObj.GetComponent<TMP_InputField>();
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

        // TODO: Handle access/refresh token, get user data.
        OnRequestSuccess += ReturnCodeHandler;
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
            OAuth2AccessToken accessToken = JsonUtility.FromJson<OAuth2AccessToken>(req.downloadHandler.text);
            Debug.Log(req.downloadHandler.text);
            // Perform callback on success through delegate
            //if (OnRequestSuccess != null)
            //    OnRequestSuccess(accessToken);
            OnRequestSuccess.Invoke(accessToken);
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
            OnGetHeartRateSuccess.Invoke(hrData);
        }
    }

    private void GetAllData()
    {
        GetHeartRateData();
    }

    private void GetHeartRateData()
    {
        string authToken = "Bearer " + PlayerPrefs.GetString("FitbitAccessToken");
        var request = UnityWebRequest.Get(_heartRateUrl);
        request.SetRequestHeader("authorization", authToken);

        OnGetHeartRateSuccess += FetchedDataHandler;
        StartCoroutine(WaitForHeartRateData(request));
    }

    private void ReturnCodeHandler(OAuth2AccessToken oAuth2Obj)
    {
        // Store Fitbit API tokens
        PlayerPrefs.SetString("FitbitAccessToken", oAuth2Obj.access_token);
        PlayerPrefs.SetString("FitbitRefreshToken", oAuth2Obj.refresh_token);
        PlayerPrefs.SetString("FitbitTokenType", oAuth2Obj.token_type);
        PlayerPrefs.SetInt("FitbitExpiresIn", oAuth2Obj.expires_in);

        GetAllData();

        // Unsubscribe method from eventhandler once work is finished
        OnRequestSuccess -= ReturnCodeHandler;
    }

    private void RefreshTokenHandler()
    {

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
