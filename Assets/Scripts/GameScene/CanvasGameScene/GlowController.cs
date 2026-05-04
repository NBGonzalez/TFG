using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GlowController : MonoBehaviour
{
    [Header("Referencias")]
    public Image glowImage;

    [Header("Ajustes de Parpadeo")]
    public float flashDuration = 1.5f; // Cuánto dura el parpadeo en total
    public int flashBlinks = 3;        // Cuántas veces parpadea la luz
    public float maxAlpha = 0.8f;      // Brillo máximo (0.0 a 1.0)

    [Header("Colores")]
    public Color colorCorrect = Color.green;
    public Color colorIncorrect = Color.red;
    public Color colorWarning = Color.yellow; // Para los minijuegos fallados

    private Coroutine currentGlowCoroutine;
    private bool isYellowModeActive = false;

    private void Start()
    {
        // Apagamos la luz al empezar
        SetGlowAlpha(0f);
    }

    // =================================================================
    // 1. EL MODO AMARILLO (Fijo, cuando juega minijuegos fallados)
    // =================================================================
    public void SetYellowMode(bool active)
    {
        isYellowModeActive = active;

        if (currentGlowCoroutine != null) StopCoroutine(currentGlowCoroutine);

        if (active)
        {
            glowImage.color = new Color(colorWarning.r, colorWarning.g, colorWarning.b, maxAlpha * 0.5f); // Un poco más tenue para no molestar
        }
        else
        {
            SetGlowAlpha(0f);
        }
    }

    // =================================================================
    // 2. PARPADEO (Llamas a esto cuando el jugador responde)
    // =================================================================
    public void ShowResult(bool isCorrect)
    {
        if (currentGlowCoroutine != null) StopCoroutine(currentGlowCoroutine);
        currentGlowCoroutine = StartCoroutine(FlashRoutine(isCorrect ? colorCorrect : colorIncorrect));
    }

    private IEnumerator FlashRoutine(Color targetColor)
    {
        float timer = 0f;

        // Asignamos el color base, pero con el alpha a 0
        targetColor.a = 0f;
        glowImage.color = targetColor;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;

            // Magia matemática: Usamos un Seno para hacer la curva de parpadeo suave
            // Multiplicamos por PI y por los parpadeos para hacer la onda
            float wave = Mathf.Sin((timer / flashDuration) * Mathf.PI * flashBlinks);

            // Nos aseguramos de que el valor sea positivo (valor absoluto)
            float currentAlpha = Mathf.Abs(wave) * maxAlpha;

            SetGlowAlpha(currentAlpha);
            yield return null;
        }

        // Al terminar el parpadeo, volvemos a como estábamos
        if (isYellowModeActive)
        {
            SetYellowMode(true); // Vuelve al amarillo
        }
        else
        {
            SetGlowAlpha(0f); // Se apaga completamente
        }
    }

    // Función auxiliar para cambiar la transparencia limpiamente
    private void SetGlowAlpha(float alpha)
    {
        Color c = glowImage.color;
        c.a = alpha;
        glowImage.color = c;
    }
}
