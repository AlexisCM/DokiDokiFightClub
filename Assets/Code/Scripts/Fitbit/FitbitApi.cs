using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FitbitApi : MonoBehaviour
{
    private const string _consumerSecret = "";
    private const string _clientId = "";

    // URIs
    private const string _tokenUrl = "https://api.fitbit.com/oauth2/token";
    private const string _oAuth2Uri = "https://www.fitbit.com/oauth2/authorize?";
    private const string _callbackUrl = "https%3A%2F%2Falexiscm.github.io%2FDokiDokiFightClub%2Fauth%2Fddfc-game%2F";

    // URI arguments
    private const string _codeChallenge = "";
    private const string _codeChallMethod = "S256";
    private const string _state = "";
    private const string _scopeParams = "heartrate+profile+sleep";

    // Response Codes
    private string _returnCode; // The code returned from API after successful call

    private UnityWebRequest _request;

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

        StartCoroutine(UseReturnCode());
    }

    public void UseRefreshToken()
    {
        string refreshToken = PlayerPrefs.GetString("FitbitRefreshToken");
    }

    public void AuthorizeFitbitUser()
    {
        var authorizationUrl = $"{_oAuth2Uri}&client_id={_clientId}&response_type=code&code_challenge={_codeChallenge}&" +
            $"code_challenge_method={_codeChallMethod}&state={_state}&scope={_scopeParams}&redirect_uri={_callbackUrl}";

        Application.OpenURL(authorizationUrl);
    }

    private IEnumerator UseReturnCode()
    {
        var plainTextBytesToken = System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_consumerSecret}");
        var encodedToken = Convert.ToBase64String(plainTextBytesToken);

        var form = new WWWForm();
        form.AddField("client_id", _clientId);
        form.AddField("grant_type", "authorization_code");
        form.AddField("redirect_uri", UnityWebRequest.UnEscapeURL(_callbackUrl));
        form.AddField("code", _returnCode);

        var headers = form.headers;
        headers["Authorization"] = "Basic " + encodedToken;
        headers["Content-Type"] = "application/x-www-form-urlencoded";

        using (_request = UnityWebRequest.Post(_tokenUrl, form))
        {
            yield return _request.SendWebRequest();

            if (_request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(_request.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}
