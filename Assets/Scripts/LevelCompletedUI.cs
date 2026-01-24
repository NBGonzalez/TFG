using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using RDG; // Necesario para Vibration

public class LevelCompletedUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI aciertosText;

    [Header("Estrellas (Imágenes Doradas)")]
    [SerializeField] private GameObject star1Filled;
    [SerializeField] private GameObject star2Filled;
    [SerializeField] private GameObject star3Filled;

    [Header("Grupos de Animación (CanvasGroup)")]
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup starsGroup;
    [SerializeField] private CanvasGroup buttonGroup;

    [Header("Botón")]
    [SerializeField] private Button continueButton;

    [Header("Confetti (Particle Systems)")]
    [SerializeField] private ParticleSystem confetti1;
    [SerializeField] private ParticleSystem confetti2;

    private void Awake()
    {
        SetAlpha(titleGroup, 0);
        SetAlpha(starsGroup, 0);
        SetAlpha(buttonGroup, 0);

        if (star1Filled) star1Filled.SetActive(false);
        if (star2Filled) star2Filled.SetActive(false);
        if (star3Filled) star3Filled.SetActive(false);

        continueButton.onClick.AddListener(OnContinuePressed);
    }

    public void Setup(string levelName, int aciertos, int fallos)
    {
        gameObject.SetActive(true);
        levelNameText.text = levelName;

        int totalPreguntas = aciertos + fallos;
        float porcentaje = (totalPreguntas > 0) ? (float)aciertos / totalPreguntas : 0;

        StartCoroutine(AnimateSequence(aciertos, totalPreguntas, porcentaje));
    }

    private IEnumerator AnimateSequence(int aciertos, int total, float porcentaje)
    {
        // 1. Título
        yield return StartCoroutine(FadeIn(titleGroup));

        // 2. Stats
        aciertosText.text = $"Aciertos: 0/{total}";
        yield return StartCoroutine(FadeIn(starsGroup));

        // 3. Cuenta progresiva (VIBRA AL CONTAR)
        yield return StartCoroutine(CountUpScore(aciertos, total));

        // 4. Calcular estrellas
        int starsEarned = 0;
        if (porcentaje >= 0.3f) starsEarned = 1;
        if (porcentaje >= 0.6f) starsEarned = 2;
        if (porcentaje >= 0.99f) starsEarned = 3;

        // 5. Encender estrellas (VIBRA AL APARECER)
        if (starsEarned >= 1) yield return StartCoroutine(PopStar(star1Filled));
        if (starsEarned >= 2) yield return StartCoroutine(PopStar(star2Filled));
        if (starsEarned >= 3) yield return StartCoroutine(PopStar(star3Filled));

        yield return new WaitForSeconds(0.2f);

        // 6. LÓGICA DEL CONFETI + VIBRACIONES CRONOMETRADAS

        // 6.1 Configuración de ciclos
        SetBurstCycles(confetti1, 1);
        SetBurstCycles(confetti2, 1);

        if (starsEarned == 3)
        {
            SetBurstCycles(confetti1, 2);
            SetBurstCycles(confetti2, 2);
            Debug.Log("¡MODO ÉPICO ACTIVADO!");
        }

        // 6.2 Disparo visual (Las partículas se encargan de sus propios delays visuales)
        if (starsEarned >= 1 && confetti1 != null) confetti1.Play();
        if (starsEarned >= 2 && confetti2 != null) confetti2.Play();

        // 6.3 SECUENCIA DE VIBRACIÓN CRONOMETRADA
        // Simulamos los tiempos que me has dicho para que la vibración coincida

        // --- MOMENTO 0.0s: Salta Confetti 1 (Burst 1) ---
        if (starsEarned >= 1)
        {
            // Vibración seca y corta (50ms, intensidad media-alta)
            Vibration.Vibrate(50, 50);
        }

        // --- MOMENTO 0.3s: Salta Confetti 2 ---
        // Esperamos 0.3 segundos
        yield return new WaitForSeconds(0.3f);

        if (starsEarned >= 2)
        {
            // Vibración seca (50ms, intensidad media-alta)
            Vibration.Vibrate(50, 100);
        }

        // --- MOMENTO 0.5s: Salta Confetti 1 (Burst 2 - Solo si 3 estrellas) ---
        // Ya hemos esperado 0.3s, así que esperamos 0.2s más para llegar a 0.5s
        yield return new WaitForSeconds(0.2f);

        if (starsEarned == 3)
        {
            // Vibración seca para el rebote
            Vibration.Vibrate(50, 150);
        }

        // Pequeña pausa dramática antes del botón
        yield return new WaitForSeconds(0.1f);

        // 7. GRAN FINAL: Aparece el botón + Vibración Larga
        // Vibración de 500ms a MÁXIMA potencia (255)
        Vibration.Vibrate(500, 255);

        yield return StartCoroutine(FadeIn(buttonGroup));
    }

    // --- Efectos ---

    private IEnumerator FadeIn(CanvasGroup group)
    {
        if (group == null) yield break;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, t / 0.5f);
            yield return null;
        }
        group.alpha = 1;
    }

    private IEnumerator CountUpScore(int target, int total)
    {
        float duration = 0.8f;
        float t = 0;
        int lastValue = -1; // Para detectar cambio de número

        while (t < duration)
        {
            t += Time.deltaTime;
            float percent = t / duration;
            int current = (int)Mathf.Lerp(0, target, percent);

            aciertosText.text = $"Aciertos: {current}/{total}";

            // VIBRACIÓN AL CONTAR: Solo si el número ha cambiado en este frame
            if (current != lastValue)
            {
                // Vibración muy sutil y cortísima (Tick)
                // 15ms de tiempo, 100 de intensidad (suave)
                Vibration.Vibrate(15, 100);
                lastValue = current;
            }

            yield return null;
        }
        // Asegurar valor final
        if (lastValue != target) Vibration.Vibrate(15, 40);
        aciertosText.text = $"Aciertos: {target}/{total}";
    }

    private IEnumerator PopStar(GameObject starObj)
    {
        if (starObj == null) yield break;

        starObj.SetActive(true);

        // VIBRACIÓN AL SALIR LA ESTRELLA
        // Usamos el efecto "Click" de sistema que es muy limpio
        //Vibration.VibratePredefined(Vibration.PredefinedEffect.EFFECT_CLICK);
        Vibration.Vibrate(20, 100);
        // Si no te gusta el click, usa: Vibration.Vibrate(20, 100);

        Transform t = starObj.transform;
        t.localScale = Vector3.zero;

        float duration = 0.4f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // He vuelto a poner tu efecto de rebote (Cos) que mola más
            float scale = Mathf.Cos(progress * Mathf.PI) * 0.2f + 1f;
            // Pero cuidado, Cos empieza en 1, baja y sube. Para crecer desde 0 con rebote:
            // Mejor usar una curva de animación o un BackEaseOut. 
            // Si tu fórmula anterior te gustaba visualmente, déjala. 
            // Aquí pongo un Lerp con "overshoot" simple:
            float overshoot = Mathf.Sin(progress * Mathf.PI) * 0.3f;
            t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress) + (Vector3.one * overshoot);

            yield return null;
        }
        t.localScale = new Vector3(1.7f, 1.7f, 1.7f);
    }

    // --- HELPER PARA MODIFICAR PARTICULAS ---
    private void SetBurstCycles(ParticleSystem ps, int count)
    {
        if (ps == null) return;
        var emission = ps.emission;
        if (emission.burstCount > 0)
        {
            var burst = emission.GetBurst(0);
            burst.cycleCount = count;
            emission.SetBurst(0, burst);
        }
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }

    private void OnContinuePressed()
    {
        Debug.Log("Botón de continuar pulsado.");
        SceneManager.LoadScene("MainScene");
    }
}