using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class LoginManager : UIStateBase
{
    private string m_GooglePlayGamesTokem;

    private async void Awake()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();

       if(UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Services Initializing");
            await UnityServices.InitializeAsync();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    m_GooglePlayGamesTokem = code;
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                });
                stateManager.ChangeState("Main");
            }
            else
            {
                //Error = "Failed to retrieve Google play games authorization code";
                Debug.Log($"Google Play Games login unsuccessful");
            }
        });
    }

    public void StartSignInWithGooglePlayGames() // Happens when the player click the Sign In button.
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not yet authenticated with Google Play Games -- attemping login again");
            LoginGooglePlayGames();
            return;
        }
        SignInLOrLinkWithGooglePlayGames();
    }
    private async void SignInLOrLinkWithGooglePlayGames() // Is new player o existing player who wants to connect to Google account?
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesTokem))
        {
            Debug.LogWarning("Authorization code is null or empty!");
            return;
        }
        if(!AuthenticationService.Instance.IsSignedIn) // For new Players, it creates an account for them
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesTokem);
        }
        else 
        {
            // For existing players.
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesTokem);
        }
    }
    async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            // Take the GooglePlayGames token and hands it to Unity's services.
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }

        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async void StartAnonymousSignIn()
    {
        await SignUpAnonymouslyAsync();
    }

    private async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            stateManager.ChangeState("Main");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    private async Task LinkWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
}
