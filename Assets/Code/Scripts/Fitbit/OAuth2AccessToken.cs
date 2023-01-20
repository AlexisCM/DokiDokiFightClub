/// <summary>
/// Object to represent JSON data returned from Fitbit authorization request.
/// </summary>
[System.Serializable]
public class OAuth2AccessToken
{
    // JSON Properties
    public string user_id;          // User id of signed-in user
    public string token_type;       // Usually set to "Bearer"
    public string access_token;     // The token granting access for api requests
    public int expires_in;          // In seconds; the time it takes for the token to expire
    public string refresh_token;    // Used to get new pair of tokens after access token has expired
    public string scope;            // The scopes authorized by the user

    public OAuth2AccessToken(string user_id, string token_type, string access_token, int expires_in, string refresh_token, string scope)
    {
        this.user_id = user_id;
        this.token_type = token_type;
        this.access_token = access_token;
        this.expires_in = expires_in;
        this.refresh_token = refresh_token;
        this.scope = scope;
    }
    
    public override string ToString()
    {
        return $"user_id: {user_id}\ntoken_type: {token_type}\naccess_token: {access_token}\nexpires_in: {expires_in}" +
            $"\nrefresh_token: {refresh_token}\nscope: {scope}";
    }
}
