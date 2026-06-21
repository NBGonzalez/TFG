using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    private string m_GooglePlayGamesTokem;

    // Controla que Google Play Games se active SOLO cuando el usuario lo decide.
    private bool m_PlayGamesActivated = false;
    // Evita que el login se lance dos veces seguidas (el botón está cableado por partida doble).
    private bool m_IsSigningIn = false;

    

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // IMPORTANTE: aqui NO activamos Google Play Games.
        // Si llamasemos a PlayGamesPlatform.Activate() en el arranque, GPGS intentaria
        // el login automatico (mostrando el banner "Bienvenido ..." con tu avatar).
        // Lo activamos solo cuando el usuario pulsa el boton de Google Play.

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Services Initializing");
            await UnityServices.InitializeAsync();
        }
    }

    // Activa Google Play Games de forma perezosa: solo la primera vez que el
    // usuario decide conectarse. Asi evitamos el inicio de sesion automatico.
    private void EnsurePlayGamesActivated()
    {
        if (m_PlayGamesActivated) return;
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        m_PlayGamesActivated = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // ============================
    // GooglePlayGames login flow
    // ============================
    public void LoginGooglePlayGames()
    {
        // Evita lanzar el login dos veces (el boton del prefab llama a
        // StartSignInWithGooglePlayGames y, ademas, LoginState anade su propio listener).
        if (m_IsSigningIn) return;
        m_IsSigningIn = true;

        // Activamos GPGS justo ahora: es la decision explicita del usuario de conectarse.
        EnsurePlayGamesActivated();

        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");
                Debug.Log("Bienvenido -----------------> " + PlayGamesPlatform.Instance.GetUserDisplayName());

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
                {
                    Debug.Log("Authorization code: " + code);
                    m_GooglePlayGamesTokem = code;
                    // Con el token disponible, conectamos con Unity Authentication.
                    await SignInLOrLinkWithGooglePlayGamesAsync();
                    m_IsSigningIn = false;
                });
            }
            else
            {
                Debug.Log($"Google Play Games login unsuccessful");
                m_IsSigningIn = false;
            }
        });
    }

    // Entrada publica del boton del prefab. Reutiliza el mismo flujo que LoginState.
    public void StartSignInWithGooglePlayGames() // Happens when the player click the Sign In button.
    {
        LoginGooglePlayGames();
    }

    private async Task SignInLOrLinkWithGooglePlayGamesAsync() // Jugador nuevo o jugador existente que quiere enlazar su cuenta de Google.
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesTokem))
        {
            Debug.LogWarning("Authorization code is null or empty!");
            return;
        }
        if(!AuthenticationService.Instance.IsSignedIn) // Jugador nuevo: se le crea una cuenta.
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesTokem);
        }
        else
        {
            // Jugador existente (p. ej. estaba como invitado): enlazamos la cuenta.
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
            //stateManager.ChangeState("Main");

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

    // ============================
    // OBTENER NOMBRE DEL JUGADOR
    // ============================
    public string GetPlayerName()
    {
        // 1. Prioridad m�xima: Si est� en Google Play, devolvemos su nombre real (Ej: "Juan Perez")
        //    Comprobamos m_PlayGamesActivated: si el usuario entr� como invitado, GPGS
        //    nunca se activ� y no debemos tocar PlayGamesPlatform.Instance.
        if (m_PlayGamesActivated && PlayGamesPlatform.Instance.IsAuthenticated())
        {
            return PlayGamesPlatform.Instance.GetUserDisplayName();
        }
        // 2. Si est� logeado como an�nimo en Unity, le damos un nombre gen�rico con parte de su ID
        else if (AuthenticationService.Instance.IsSignedIn)
        {
            string id = AuthenticationService.Instance.PlayerId;
            // Cortamos el ID para que no quede un churro gigante en la pantalla del amigo
            string shortId = id.Length >= 5 ? id.Substring(0, 5) : id;
            return "Jugador_" + shortId;
        }

        // 3. Si por alg�n motivo est� offline o probando en el Editor de Unity
        return "Creador An�nimo";
    }
}
