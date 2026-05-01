// LocalItineraryCardUI.cs
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalItineraryCardUI : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;

    [Header("Botones")]
    public Button editButton;
    public Button deleteButton;
    public Button shareButton;

    [Header("Toast 'Mensaje copiado' (opcional)")]
    [Tooltip("Pequeño panel que aparece brevemente diciendo que el mensaje fue copiado al portapapeles.")]
    public GameObject clipboardToastPanel;

    // Variables internas para saber quiénes somos
    private string myItineraryId;
    private string myFilePath;

    private void Start()
    {
        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        shareButton.onClick.AddListener(OnShareClicked);
    }

    // El Manager llamará a esta función para inyectar los datos del JSON
    public void SetupCard(string title, string author, string id, string filePath)
    {
        titleText.text = title;
        authorText.text = "Por: " + author;
        myItineraryId = id;
        myFilePath = filePath;
    }

    private void OnEditClicked()
    {
        Debug.Log("✏️ Editando: " + myItineraryId);
        ItineraryCrossSceneData.itineraryIdToEdit = myItineraryId;

        if (BackgroundTransition.Instance != null)
            BackgroundTransition.Instance.ToggleTransitionAndLoad("ItineraryScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("ItineraryScene");
    }

    private void OnDeleteClicked()
    {
        if (File.Exists(myFilePath))
        {
            File.Delete(myFilePath);
            Debug.Log("🗑️ Archivo borrado: " + myFilePath);
        }
        Destroy(gameObject);
    }

    private void OnShareClicked()
    {
        if (!File.Exists(myFilePath))
        {
            Debug.LogError("[Share] Archivo no encontrado: " + myFilePath);
            return;
        }

        string titulo       = titleText.text;
        string playStoreUrl = "https://play.google.com/store/apps/details?id=" + Application.identifier;

        // ──────────────────────────────────────────────────────────────────────
        //  POR QUÉ EL CAPTION NO APARECE EN WHATSAPP Y CÓMO LO SOLUCIONAMOS
        // ──────────────────────────────────────────────────────────────────────
        //
        //  WhatsApp ≥ 2019 ignora EXTRA_TEXT cuando el intent lleva un archivo
        //  adjunto de tipo "documento" (cualquier MIME que no sea image/* o video/*).
        //  Esto es una decisión deliberada de Meta para evitar spam, y NO puede
        //  saltarse desde fuera de la app de WhatsApp.
        //
        //  SOLUCIÓN: Copiamos el mensaje al portapapeles de Android ANTES de
        //  abrir el diálogo de compartir. Así, cuando el usuario seleccione
        //  WhatsApp y elija el contacto, solo tiene que PEGAR (mantener pulsado
        //  → Pegar). Le mostramos un toast para avisarle de esto.
        //
        //  En el resto de apps (Telegram, Gmail, etc.) el SetText() sí funciona
        //  y el receptor ve el mensaje directamente.
        // ──────────────────────────────────────────────────────────────────────

        string mensaje =
            $"🎮 ¡Te comparto mi itinerario \"{titulo}\"!\n\n" +
            $"📥 Descarga el archivo adjunto y ábrelo con el juego para jugar.\n\n" +
            $"👉 ¿No tienes el juego aún? Descárgalo gratis aquí:\n{playStoreUrl}";

        // 1. COPIAR AL PORTAPAPELES (funciona en Android, iOS y Editor)
        GUIUtility.systemCopyBuffer = mensaje;
        Debug.Log("[Share] Mensaje copiado al portapapeles.");

        // 2. Mostrar aviso al usuario (si hay panel configurado)
        ShowClipboardToast();

        // 3. Preparar el archivo temporal con extensión .itinerario
        string nombreSeguro = titulo;
        foreach (char c in Path.GetInvalidFileNameChars())
            nombreSeguro = nombreSeguro.Replace(c, '_');

        string rutaTemporal = Path.Combine(Application.temporaryCachePath, nombreSeguro + ".itinerario");

        try
        {
            File.Copy(myFilePath, rutaTemporal, overwrite: true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Share] No se pudo crear la copia temporal: " + e.Message);
            return;
        }

        // 4. Compartir: SetText va al campo de texto en apps que lo soporten.
        //    AddFile adjunta el .itinerario como documento.
        //    En WhatsApp el usuario solo deberá pegar el texto del portapapeles.
        new NativeShare()
            .SetText(mensaje)                                      // Funciona en Telegram, Gmail, etc.
            .SetSubject("🎓 Itinerario: " + titulo)               // Asunto (para email)
            .AddFile(rutaTemporal, "application/octet-stream")     // El archivo .itinerario
            .SetCallback(OnShareResult)
            .Share();
    }

    private void OnShareResult(NativeShare.ShareResult result, string shareTarget)
    {
        Debug.Log($"[Share] Resultado: {result}, App: {shareTarget}");
    }

    // ──────────────────────────────────────────────────────────────────────
    //  TOAST DE "MENSAJE COPIADO"
    // ──────────────────────────────────────────────────────────────────────
    private void ShowClipboardToast()
    {
        if (clipboardToastPanel != null)
        {
            clipboardToastPanel.SetActive(true);
            // Lo ocultamos automáticamente tras 3 segundos
            StartCoroutine(HideToastAfter(3f));
        }
        else
        {
            // Fallback: Toast nativo de Android si no hay panel UI configurado
            ShowAndroidToast("📋 Mensaje copiado. Pégalo en WhatsApp.");
        }
    }

    private System.Collections.IEnumerator HideToastAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (clipboardToastPanel != null)
            clipboardToastPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra un Toast nativo de Android (el popup oscuro pequeño que aparece abajo).
    /// Solo funciona en Android. En editor no hace nada.
    /// </summary>
    private void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass  unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass  toastClass = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText",
                    activity,
                    message,
                    toastClass.GetStatic<int>("LENGTH_LONG")
                );
                toast.Call("show");
            }));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Share] No se pudo mostrar el Toast: " + e.Message);
        }
#else
        Debug.Log("[Share] Toast: " + message);
#endif
    }
}