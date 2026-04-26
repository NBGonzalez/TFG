using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class LevelInfoPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundButton; // Para cerrar tocando fuera

    [Header("Estrellas")]
    [SerializeField] private Image[] stars; // Arrastra las 3 imágenes aquí
    [SerializeField] private Sprite starOn;  // Sprite dorado
    [SerializeField] private Sprite starOff; // Sprite gris

    [Header("Animación")]
    [SerializeField] private RectTransform panelRect; // El panel que se mueve
    [SerializeField] private CanvasGroup overlayGroup; // El fondo negro para el fade
    [SerializeField] private float animationSpeed = 0.3f;

    private string _currentLanguage;
    private string _currentLevelId;
    private bool _isLocal;

    private void Awake()
    {
        // Configurar botones
        playButton.onClick.AddListener(OnPlayClicked);
        closeButton.onClick.AddListener(Hide);
        backgroundButton.onClick.AddListener(Hide);

        // Ocultar al inicio por si acaso se quedó abierto en el editor
        gameObject.SetActive(false);
    }

    // --- FUNCIÓN PRINCIPAL: MOSTRAR ---
    public void Show(LevelModel levelData, string language, int starsCount, bool isLocal)
    {
        _currentLanguage = language;
        _currentLevelId = levelData.id;
        _isLocal = isLocal; // Lo guardamos

        // 1. Rellenar datos
        titleText.text = levelData.title;
        descText.text = levelData.description;

        // 2. Pintar estrellas
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = (i < starsCount) ? starOn : starOff;
        }

        // 3. Activar y Animar
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateIn());
    }

    // --- FUNCIÓN PRINCIPAL: OCULTAR ---
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateOut());
    }

    private void OnPlayClicked()
    {
        // Lógica de carga de nivel
        Debug.Log($"JUGANDO: {_currentLanguage} - {_currentLevelId}");
        GameManager.Instance.SetCurrentLevel(_currentLanguage, _currentLevelId, _isLocal);
        BackgroundTransition.Instance.ToggleTransitionAndLoad("GameScene");
    }

    // --- CORUTINAS DE ANIMACIÓN ---

    private IEnumerator AnimateIn()
    {
        float t = 0;
        // Posición inicial: Abajo del todo (oculto)
        // Asumimos que el panel tiene una altura de unos 500-600px. Lo bajamos esa cantidad.
        float hiddenY = -panelRect.rect.height;

        panelRect.anchoredPosition = new Vector2(0, hiddenY);
        overlayGroup.alpha = 0;

        while (t < 1)
        {
            t += Time.deltaTime / animationSpeed;
            float smoothT = Mathf.SmoothStep(0, 1, t); // Suavizado

            // Fade del fondo negro
            overlayGroup.alpha = Mathf.Lerp(0, 1, smoothT);

            // Deslizamiento del panel hacia Y=0
            float newY = Mathf.Lerp(hiddenY, 0, smoothT);
            panelRect.anchoredPosition = new Vector2(0, newY);

            yield return null;
        }

        // Asegurar final
        panelRect.anchoredPosition = Vector2.zero;
        overlayGroup.alpha = 1;
    }

    private IEnumerator AnimateOut()
    {
        float t = 0;
        float hiddenY = -panelRect.rect.height;

        while (t < 1)
        {
            t += Time.deltaTime / animationSpeed;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            overlayGroup.alpha = Mathf.Lerp(1, 0, smoothT);

            float newY = Mathf.Lerp(0, hiddenY, smoothT);
            panelRect.anchoredPosition = new Vector2(0, newY);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}