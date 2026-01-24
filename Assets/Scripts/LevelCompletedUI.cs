using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCompletedUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI aciertosText; // Texto: "Aciertos: 5/10"

    [Header("Estrellas (Imágenes Doradas)")]
    // Arrastra aquí las imágenes de las estrellas "Llenas"
    [SerializeField] private GameObject star1Filled;
    [SerializeField] private GameObject star2Filled;
    [SerializeField] private GameObject star3Filled;

    [Header("Grupos de Animación (CanvasGroup)")]
    [SerializeField] private CanvasGroup titleGroup; // Título + Nombre Nivel
    [SerializeField] private CanvasGroup starsGroup; // El padre de las estrellas y el texto de aciertos
    [SerializeField] private CanvasGroup buttonGroup; // El botón de continuar

    [Header("Botón")]
    [SerializeField] private Button continueButton;

    [Header("Confetti (Particle Systems)")]
    [SerializeField] private ParticleSystem confetti1;
    [SerializeField] private ParticleSystem confetti2;

    private void Awake()
    {
        // 1. Ocultar grupos (Transparencia 0)
        SetAlpha(titleGroup, 0);
        SetAlpha(starsGroup, 0);
        SetAlpha(buttonGroup, 0);

        // 2. Apagar estrellas doradas (para que se vean las grises de fondo)
        if (star1Filled) star1Filled.SetActive(false);
        if (star2Filled) star2Filled.SetActive(false);
        if (star3Filled) star3Filled.SetActive(false);

        continueButton.onClick.AddListener(OnContinuePressed);
    }

    // El Manager nos pasa los datos
    public void Setup(string levelName, int aciertos, int fallos)
    {
        gameObject.SetActive(true);
        levelNameText.text = levelName;

        // Calculamos nota
        int totalPreguntas = aciertos + fallos;

        // Evitamos división por cero
        float porcentaje = (totalPreguntas > 0) ? (float)aciertos / totalPreguntas : 0;

        // Iniciamos el show
        StartCoroutine(AnimateSequence(aciertos, totalPreguntas, porcentaje));
    }

    private IEnumerator AnimateSequence(int aciertos, int total, float porcentaje)
    {
        // 1. Aparece el Título ("Nivel Completado")
        yield return StartCoroutine(FadeIn(titleGroup));

        // 2. Aparece el bloque de Stats (Aciertos + Estrellas vacías)
        aciertosText.text = $"Aciertos: 0/{total}"; // Empezamos en 0
        yield return StartCoroutine(FadeIn(starsGroup));

        // 3. Cuenta progresiva de aciertos (0 -> 5...)
        yield return StartCoroutine(CountUpScore(aciertos, total));

        // 4. Calcular cuántas estrellas tocan
        int starsEarned = 0;
        if (porcentaje >= 0.3f) starsEarned = 1; // Mínimo para 1 estrella
        if (porcentaje >= 0.6f) starsEarned = 2; // Notable
        if (porcentaje == 1.0f) starsEarned = 3; // Perfecto

        // 5. Encender estrellas una a una con "Pop"
        if (starsEarned >= 1) yield return StartCoroutine(PopStar(star1Filled));
        if (starsEarned >= 2) yield return StartCoroutine(PopStar(star2Filled));
        if (starsEarned >= 3) yield return StartCoroutine(PopStar(star3Filled));

        yield return new WaitForSeconds(0.2f);

        // 6. LÓGICA DEL CONFETI (CLÍMAX)

        // 6.1 Reseteamos los ciclos a 1 por seguridad (limpieza)
        SetBurstCycles(confetti1, 1);
        SetBurstCycles(confetti2, 1);

        // 6.2 Si sacó 3 estrellas -> Modo Épico (Doble de partículas)
        if (starsEarned == 3)
        {
            SetBurstCycles(confetti1, 2);
            SetBurstCycles(confetti2, 2);
            Debug.Log("¡MODO ÉPICO ACTIVADO!");
        }

        // 6.3 Disparamos según corresponda
        if (starsEarned >= 1 && confetti1 != null) confetti1.Play();
        if (starsEarned >= 2 && confetti2 != null) confetti2.Play();

        yield return new WaitForSeconds(0.5f);

        // 7. Aparece el botón
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
        while (t < duration)
        {
            t += Time.deltaTime;
            float percent = t / duration;
            int current = (int)Mathf.Lerp(0, target, percent);
            aciertosText.text = $"Aciertos: {current}/{total}";
            yield return null;
        }
        aciertosText.text = $"Aciertos: {target}/{total}";
    }

    private IEnumerator PopStar(GameObject starObj)
    {
        if (starObj == null) yield break;

        starObj.SetActive(true); // Encender la dorada

        // Pequeño efecto de escala (Boing!)
        Transform t = starObj.transform;
        t.localScale = Vector3.zero; // Empieza invisible

        float duration = 0.4f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            // Curva de rebote (Overshoot)
            float scale = Mathf.Sin(progress * Mathf.PI) * 0.2f + 1f;
            // Simplificado: Lerp normal de 0 a 1 si el rebote da problemas
            t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);

            yield return null;
        }
        t.localScale = new Vector3(1.7f, 1.7f, 1.7f);

        // Sonido de estrella aquí quedaría genial
    }

    // --- HELPER PARA MODIFICAR PARTICULAS ---
    private void SetBurstCycles(ParticleSystem ps, int count)
    {
        if (ps == null) return;

        // 1. Extraer módulo
        var emission = ps.emission;

        // 2. Extraer Burst (si existe)
        if (emission.burstCount > 0)
        {
            var burst = emission.GetBurst(0); // Cogemos el primer burst

            // 3. Modificar
            burst.cycleCount = count; // Cambiamos cuántas veces se repite

            // 4. Aplicar de nuevo (IMPORTANTE)
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