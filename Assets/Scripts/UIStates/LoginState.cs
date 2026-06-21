using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;

/// <summary>
/// Estado UI simple para manejar el login:
/// - llama a LoginManager para iniciar login (Google / Guest)
/// - muestra un spinner b�sico (o deshabilita botones) mientras espera
/// - espera hasta que AuthenticationService.Instance.IsSignedIn sea true (o timeout)
/// - si OK -> stateManager.ChangeState("Main") ; si falla -> muestra error
/// </summary>
public class LoginState : UIStateBase
{
    [Header("UI refs")]
    [SerializeField] private Button googleButton;
    [SerializeField] private Button guestButton;

    [Header("Feedback UI")]
    [SerializeField] private GameObject loadingPanel;      // panel simple que contiene "Cargando..." o spinner
    [SerializeField] private TextMeshProUGUI errorText;    // texto para mostrar errores

    [Header("Polling settings")]
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private float timeoutSeconds = 12f;

    private Coroutine waitCoroutine;

    public override void OnEnter()
    {
        base.OnEnter();
        // limpiar UI
        SetLoading(false);
        SetError(null);

        // listeners
        if (googleButton != null) googleButton.onClick.AddListener(OnGoogleClicked);
        if (guestButton != null) guestButton.onClick.AddListener(OnGuestClicked);
    }

    public override void OnExit()
    {
        base.OnExit();
        // quitar listeners
        if (googleButton != null) googleButton.onClick.RemoveAllListeners();
        if (guestButton != null) guestButton.onClick.RemoveAllListeners();

        // cancelar coroutine si quedase viva
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        SetLoading(false);
        SetError(null);
    }

    private void OnGoogleClicked()
    {
        SetError(null);
        SetLoading(true);

        if (LoginManager.Instance == null)
        {
            SetLoading(false);
            SetError("LoginManager no presente en la escena.");
            return;
        }

        // Llamamos al manager para que inicie el flujo (tu implementaci�n ya lo maneja)
        LoginManager.Instance.LoginGooglePlayGames();

        // Esperamos hasta que Unity Authentication est� firmado (o until timeout)
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitForSignInCoroutine(timeoutSeconds));
    }

    private void OnGuestClicked()
    {
        SetError(null);
        SetLoading(true);

        if (LoginManager.Instance == null)
        {
            SetLoading(false);
            SetError("LoginManager no presente en la escena.");
            return;
        }

        LoginManager.Instance.StartAnonymousSignIn();

        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitForSignInCoroutine(timeoutSeconds));
    }

    private IEnumerator WaitForSignInCoroutine(float timeout)
    {
        float elapsed = 0f;

        // Primero, una ligera espera inicial para dejar que los callbacks as�ncronos empiecen
        yield return new WaitForSeconds(0.15f);

        while (elapsed < timeout)
        {
            // Comprueba si el usuario est� autenticado:
            bool unitySigned = false;
            try
            {
                // AuthenticationService podr�a no estar inicializado; protegemos con try
                unitySigned = Unity.Services.Authentication.AuthenticationService.Instance != null
                              && Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn;
            }
            catch
            {
                unitySigned = false;
            }

            // La se�al real de "conectado en la app" es Unity Authentication.
            // Antes mir�bamos tambi�n PlayGamesPlatform.IsAuthenticated(), pero eso
            // daba por bueno el login autom�tico de Google y hac�a que el bot�n de
            // invitado tambi�n saltara a Main sin que el usuario lo decidiera.
            if (unitySigned)
            {
                // �xito: navegar a Main
                SetLoading(false);
                SetError(null);

                // cancelar coroutine referencia
                waitCoroutine = null;

                stateManager.ChangeState("Main");
                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
            elapsed += checkInterval;
        }

        // Timeout: fallo
        SetLoading(false);
        SetError("Error: tiempo de espera agotado. Revisa conexi�n o intenta m�s tarde.");
        waitCoroutine = null;
    }

    // Helpers UI
    private void SetLoading(bool on)
    {
        if (loadingPanel != null) loadingPanel.SetActive(on);

        // opcional: deshabilitar botones mientras carga
        if (googleButton != null) googleButton.interactable = !on;
        if (guestButton != null) guestButton.interactable = !on;
    }

    private void SetError(string msg)
    {
        if (errorText != null) errorText.text = msg ?? "";
    }
}
