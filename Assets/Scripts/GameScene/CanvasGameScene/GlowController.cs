using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GlowController : MonoBehaviour
{
    [Header("Referencias")]
    public Image glowImage;

    [Header("Ajustes de Parpadeo")]
    public float flashDuration = 1.5f;
    public int flashBlinks = 3;
    public float maxAlpha = 0.8f;

    private Coroutine currentGlowCoroutine;
    private bool isYellowModeActive = false;

    private Color ColorCorrect
    {
        get
        {
            if (AppColorManager.Instance != null)
                return AppColorManager.Instance.CorrectColor;
            return Color.green;
        }
    }

    private Color ColorIncorrect
    {
        get
        {
            if (AppColorManager.Instance != null)
                return AppColorManager.Instance.IncorrectColor;
            return Color.red;
        }
    }

    private Color ColorWarning => Color.yellow;

    private void Start()
    {
        SetGlowAlpha(0f);
    }

    public void SetYellowMode(bool active)
    {
        isYellowModeActive = active;
        if (currentGlowCoroutine != null) StopCoroutine(currentGlowCoroutine);
        if (active)
        {
            glowImage.color = new Color(ColorWarning.r, ColorWarning.g, ColorWarning.b, maxAlpha * 0.5f);
        }
        else
        {
            SetGlowAlpha(0f);
        }
    }

    public void ShowResult(bool isCorrect)
    {
        if (currentGlowCoroutine != null) StopCoroutine(currentGlowCoroutine);
        currentGlowCoroutine = StartCoroutine(FlashRoutine(isCorrect ? ColorCorrect : ColorIncorrect));
    }

    private IEnumerator FlashRoutine(Color targetColor)
    {
        float timer = 0f;
        targetColor.a = 0f;
        glowImage.color = targetColor;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float wave = Mathf.Sin((timer / flashDuration) * Mathf.PI * flashBlinks);
            float currentAlpha = Mathf.Abs(wave) * maxAlpha;
            SetGlowAlpha(currentAlpha);
            yield return null;
        }
        if (isYellowModeActive)
        {
            SetYellowMode(true);
        }
        else
        {
            SetGlowAlpha(0f);
        }
    }

    /// <summary>
    /// Función a parte que llamarás desde OptionsState para la previsualización de daltonismo.
    /// </summary>
    public void ShowColorBlindPreview()
    {
        // Detiene cualquier parpadeo previo (sea de opciones o de juego) para limpiar la pantalla
        if (currentGlowCoroutine != null) StopCoroutine(currentGlowCoroutine);

        // Lanza la nueva rutina exclusiva
        currentGlowCoroutine = StartCoroutine(ColorBlindPreviewRoutine());
    }

    private IEnumerator ColorBlindPreviewRoutine()
    {
        // 1. Un parpadeo rápido con el color Correcto (dura 0.6 segundos, 1 parpadeo)
        yield return StartCoroutine(StandaloneFlash(ColorCorrect, flashDuration, flashBlinks));

        // Pequeña pausa de décimas de segundo con la pantalla limpia
        yield return new WaitForSeconds(0.1f);

        // 2. Un parpadeo rápido con el color Incorrecto (dura 0.6 segundos, 1 parpadeo)
        yield return StartCoroutine(StandaloneFlash(ColorIncorrect, flashDuration, flashBlinks));
    }

    /// <summary>
    /// Un motor de parpadeo gemelo pero aislado, que acepta tiempos personalizados 
    /// sin leer ni alterar las variables 'flashDuration' ni 'flashBlinks' del inspector.
    /// </summary>
    /// 
    private IEnumerator StandaloneFlash(Color targetColor, float customDuration, int customBlinks)
    {
        float timer = 0f;
        targetColor.a = 0f;
        glowImage.color = targetColor;

        while (timer < customDuration)
        {
            timer += Time.deltaTime;
            float wave = Mathf.Sin((timer / customDuration) * Mathf.PI * customBlinks);
            float currentAlpha = Mathf.Abs(wave) * maxAlpha;
            SetGlowAlpha(currentAlpha);
            yield return null;
        }

        SetGlowAlpha(0f);
    }

    private void SetGlowAlpha(float alpha)
    {
        Color c = glowImage.color;
        c.a = alpha;
        glowImage.color = c;
    }
}
