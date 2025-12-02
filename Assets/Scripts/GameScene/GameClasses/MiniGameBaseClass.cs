using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MiniGameBaseClass : MonoBehaviour
{
    [Header("Base UI")]
    [Tooltip("Título del minijuego (data.title)")]
    public TextMeshProUGUI titleText;

    [Tooltip("Texto principal del minijuego (data.content)")]
    public TextMeshProUGUI contentText;

    [Tooltip("Texto para mensajes/feedback corto")]
    public TextMeshProUGUI feedbackText;

    [Header("Common Controls")]
    public Button backButton;

    [Header("Mount point")]
    public RectTransform gameArea; // donde se instancian los minijuegos

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
            backButton.onClick.AddListener(OnBackPressed);
        }
    }

    protected void OnBackPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

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

    public GameSceneManager Manager => manager;
}
