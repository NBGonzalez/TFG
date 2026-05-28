// MiniGameBaseClass.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class MiniGameBaseClass : MonoBehaviour
{
    [Header("Base UI")]
    [Tooltip("Título del minijuego (data.title)")]
    public TextMeshProUGUI titleText;

    [Tooltip("Texto principal del minijuego (data.content)")]
    public TextMeshProUGUI contentText;

    [Tooltip("Texto para mensajes/feedback corto")]
    public TextMeshProUGUI feedbackText;

    [Tooltip("Componente Image que mostrará la ilustración de la carpeta Resources")]
    public Image generalQuestionImage;

    [Header("Common Controls")]
    public Button backButton;

    [Header("Mount point")]
    public RectTransform gameArea; // donde se instancian los minijuegos

    [Header("ExitText")]
    [Tooltip("Texto que aparecerá cuando salgas de partida")]
    public string exitText = "¿Estás seguro de que quieres salir? Tu progreso se perderá.";

    protected GameSceneManager manager;
    protected MiniGameData data;

    // Lo llama el GameSceneManager antes de inicializar el contenido
    public virtual void Show(MiniGameData data, GameSceneManager mgr)
    {
        this.data = data;
        this.manager = mgr;

        // Título
        if (titleText != null)
            titleText.text = data.title ?? "";

        // Texto principal (content)
        if (contentText != null)
            contentText.text = data.content ?? "";

        // Feedback vacío por defecto
        if (feedbackText != null)
            feedbackText.text = "";

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnTryToExitMinigame);
        }
        if (generalQuestionImage != null)
        {
            // Comprobamos si el JSON trae el array "images" con elementos
            if (data.images != null && data.images.Count > 0 && !string.IsNullOrEmpty(data.images[0]))
            {
                // El JSON dice "sql_sql-1-1.png", extraemos solo "sql_sql-1-1" para que Unity no falle
                string imageNameWithoutExtension = Path.GetFileNameWithoutExtension(data.images[0]);

                // Construimos la ruta desde la raíz de la carpeta Resources
                string resourcePath = "MiniGameImages/" + imageNameWithoutExtension;

                // Cargamos el Sprite de forma síncrona e instantánea
                Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);

                if (loadedSprite != null)
                {
                    generalQuestionImage.sprite = loadedSprite;
                    //generalQuestionImage.SetNativeSize();
                    generalQuestionImage.gameObject.SetActive(true); // Se muestra si existe
                }
                else
                {
                    Debug.LogWarning($"[Resources] No se encontró el sprite en la ruta: Resources/{resourcePath}");
                    generalQuestionImage.gameObject.SetActive(false);
                }
            }
            else
            {
                // Si el minijuego actual (como el Quizz del JSON) no tiene imágenes, apagamos el componente
                generalQuestionImage.gameObject.SetActive(false);
            }
        }
    }

    //protected void OnBackPressed()
    //{
    //    Debug.Log("[MiniGameBaseClass] Back button pressed. Returning to MainScene.");
    //    BackgroundTransition.Instance.ToggleTransitionAndLoad("MainScene");
    //    //UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    //}

    public void TriggerFailurePopup(string question, string userAns, string correctAns)
    {
        if (manager != null)
        {
            manager.HandleMiniGameFailure(data, question, userAns, correctAns);
        }
    }

    // Colores centralizados (leen de AppColorManager si existe, si no usan defaults)
    public Color CorrectColor => AppColorManager.Instance != null ? AppColorManager.Instance.CorrectColor : Color.green;
    public Color IncorrectColor => AppColorManager.Instance != null ? AppColorManager.Instance.IncorrectColor : Color.red;

    // Helpers reutilizables
    public void ShowError(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }

    public void ClearError()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void SetButtonColor(Button btn, Color c)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = c;
        colors.highlightedColor = c;
        colors.pressedColor = c;
        colors.selectedColor = c;
        btn.colors = colors;
    }

    public void ReportSuccess()
    {
        if (manager.glowController != null) manager.glowController.ShowResult(true);

        if (manager != null) manager.RecordResult(true);
    }

    public void ReportFailure()
    {
        if (manager.glowController != null) manager.glowController.ShowResult(false);

        if (manager != null) manager.RecordResult(false);
    }

    public IEnumerator FlashButtonColor(Button btn, Color c, float time = 0.35f)
    {
        if (btn == null) yield break;
        var colors = btn.colors;
        Color original = colors.normalColor;
        SetButtonColor(btn, c);
        yield return new WaitForSeconds(time);
        SetButtonColor(btn, original);
    }

    public void NextMiniGameImmediate() => manager?.NextMiniGame();

    public IEnumerator NextMiniGameDelayed(float delay = 0.7f)
    {
        yield return new WaitForSeconds(delay);
        manager?.NextMiniGame();
    }

    public void OnTryToExitMinigame()
    {
        ConfirmationPopupManager.Instance.RequestConfirmation(exitText,
            () => BackgroundTransition.Instance.ToggleTransitionAndLoad("MainScene") // <- Pasamos la función como parámetro
        );
    }

    public GameSceneManager Manager => manager;
}
