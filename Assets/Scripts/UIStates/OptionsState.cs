using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using GooglePlayGames;
using Unity.Services.Authentication;

// 1. AÑADIMOS EL NAMESPACE DEL PAQUETE
using UISwitcher;

public class OptionsState : UIStateBase
{
    [SerializeField] private Button backButton;

    [Header("Cuenta - Google Play")]
    // 2. CAMBIAMOS EL TIPO 'Toggle' POR 'UISwitcher.UISwitcher'
    [SerializeField] private UISwitcher.UISwitcher googlePlayToggle;
    [SerializeField] private TextMeshProUGUI accountStatusText;

    [Header("Colores de la Aplicacion")]
    [SerializeField] private Button[] paletteButtons;

    [Header("Colores Correcto / Incorrecto")]
    [SerializeField] private Button[] colorBlindButtons;

    [Header("Polling")]
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private float timeoutSeconds = 12f;

    [SerializeField] private GlowController glowController;

    private Coroutine linkCoroutine;

    // ===========================================
    //  ENTER / EXIT
    // ===========================================
    public override void OnEnter()
    {
        backButton.onClick.AddListener(OnBackPressed);
        Debug.Log("STATE: Options");

        SetupGooglePlayToggle();
        SetupPaletteButtons();
        SetupColorBlindButtons();
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();

        if (googlePlayToggle != null)
            googlePlayToggle.onValueChanged.RemoveAllListeners();

        CleanupButtonListeners(paletteButtons);
        CleanupButtonListeners(colorBlindButtons);

        if (linkCoroutine != null)
        {
            StopCoroutine(linkCoroutine);
            linkCoroutine = null;
        }
    }

    private void OnBackPressed()
    {
        stateManager.ChangeState("Main");
    }

    // ===========================================
    //  SECCION 1: GOOGLE PLAY TOGGLE
    // ===========================================
    private void SetupGooglePlayToggle()
    {
        if (googlePlayToggle == null) return;

        bool isLinked = IsGooglePlayLinked();

        // Poner el toggle en el estado correcto SIN disparar el listener
        SetSwitcherWithoutNotify(isLinked);

        // Si ya esta vinculado, deshabilitar el toggle (no puede desvincularse)
        googlePlayToggle.interactable = !isLinked;

        // Texto de estado
        UpdateAccountStatusText(isLinked);

        // Listener: solo se dispara cuando el jugador intenta activarlo
        googlePlayToggle.onValueChanged.AddListener(OnGooglePlayToggleChanged);
    }

    private void OnGooglePlayToggleChanged(bool isOn)
    {
        if (!isOn)
        {
            // No permitimos desactivar: forzamos ON de nuevo
            SetSwitcherWithoutNotify(true);
            return;
        }

        // Intentar vincular con Google Play
        if (LoginManager.Instance == null)
        {
            Debug.LogError("LoginManager no encontrado.");
            SetSwitcherWithoutNotify(false);
            return;
        }

        googlePlayToggle.interactable = false;
        UpdateAccountStatusText(false, "Vinculando...");

        LoginManager.Instance.StartSignInWithGooglePlayGames();

        if (linkCoroutine != null) StopCoroutine(linkCoroutine);
        linkCoroutine = StartCoroutine(WaitForGooglePlayLink());
    }

    private IEnumerator WaitForGooglePlayLink()
    {
        float elapsed = 0f;
        yield return new WaitForSeconds(0.2f);

        while (elapsed < timeoutSeconds)
        {
            if (IsGooglePlayLinked())
            {
                // Exito
                SetSwitcherWithoutNotify(true);
                googlePlayToggle.interactable = false; // Ya vinculado, se bloquea
                UpdateAccountStatusText(true);
                linkCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
            elapsed += checkInterval;
        }

        // Timeout: fallo
        SetSwitcherWithoutNotify(false);
        googlePlayToggle.interactable = true;
        UpdateAccountStatusText(false, "No se pudo vincular. Intentalo de nuevo.");
        linkCoroutine = null;
    }

    private bool IsGooglePlayLinked()
    {
        try
        {
            return PlayGamesPlatform.Instance != null && PlayGamesPlatform.Instance.IsAuthenticated();
        }
        catch
        {
            return false;
        }
    }

    private void UpdateAccountStatusText(bool linked, string customMsg = null)
    {
        if (accountStatusText == null) return;

        if (!string.IsNullOrEmpty(customMsg))
        {
            accountStatusText.text = customMsg;
            return;
        }

        accountStatusText.text = linked
            ? "Cuenta vinculada con Google Play"
            : "Cuenta local (progreso solo en este dispositivo)";
    }

    private void SetSwitcherWithoutNotify(bool newValue)
    {
        if (googlePlayToggle == null) return;

        // 1. Le quitamos la capacidad de escuchar temporalmente
        googlePlayToggle.onValueChanged.RemoveListener(OnGooglePlayToggleChanged);

        // 2. Le cambiamos el valor (usamos isOn, que es el estándar)
        googlePlayToggle.isOn = newValue;

        // 3. Le devolvemos el oído
        googlePlayToggle.onValueChanged.AddListener(OnGooglePlayToggleChanged);
    }

    // ===========================================
    //  SECCION 2: PALETA DE COLORES DE LA APP
    // ===========================================
    private void SetupPaletteButtons()
    {
        if (paletteButtons == null || AppColorManager.Instance == null) return;

        for (int i = 0; i < paletteButtons.Length; i++)
        {
            if (paletteButtons[i] == null) continue;

            int index = i; // closure-safe
            paletteButtons[i].onClick.AddListener(() => OnPaletteSelected(index));
        }

        HighlightSelectedPalette();
    }

    private void OnPaletteSelected(int index)
    {
        if (AppColorManager.Instance == null) return;
        AppColorManager.Instance.SetPalette(index);
        HighlightSelectedPalette();
    }

    private void HighlightSelectedPalette()
    {
        if (paletteButtons == null || AppColorManager.Instance == null) return;
        int selected = AppColorManager.Instance.CurrentPaletteIndex;

        for (int i = 0; i < paletteButtons.Length; i++)
        {
            if (paletteButtons[i] == null) continue;

            // Escalar el boton seleccionado un poco mas grande
            paletteButtons[i].transform.localScale = (i == selected)
                ? Vector3.one * 1.15f
                : Vector3.one;
        }
    }

    // ===========================================
    //  SECCION 3: COLORES CORRECTO / INCORRECTO
    // ===========================================
    private void SetupColorBlindButtons()
    {
        if (colorBlindButtons == null || AppColorManager.Instance == null) return;

        for (int i = 0; i < colorBlindButtons.Length; i++)
        {
            if (colorBlindButtons[i] == null) continue;

            int index = i; // closure-safe
            colorBlindButtons[i].onClick.AddListener(() => OnColorBlindSelected(index));
        }

        HighlightSelectedColorBlind();
    }

    private void OnColorBlindSelected(int index)
    {
        if (AppColorManager.Instance == null) return;
        AppColorManager.Instance.SetColorBlindMode(index);

        if (glowController != null) glowController.ShowColorBlindPreview();
        Debug.Log($"[OptionState] Color Blind Mode set to index {index}");

        HighlightSelectedColorBlind();
    }

    private void HighlightSelectedColorBlind()
    {
        if (colorBlindButtons == null || AppColorManager.Instance == null) return;
        int selected = AppColorManager.Instance.CurrentColorBlindIndex;

        for (int i = 0; i < colorBlindButtons.Length; i++)
        {
            if (colorBlindButtons[i] == null) continue;

            colorBlindButtons[i].transform.localScale = (i == selected)
                ? Vector3.one * 1.15f
                : Vector3.one;
        }
    }

    // ===========================================
    //  UTILIDADES
    // ===========================================
    private void CleanupButtonListeners(Button[] buttons)
    {
        if (buttons == null) return;
        foreach (var btn in buttons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
    }
}