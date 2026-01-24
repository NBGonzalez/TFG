using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameFillBlanks : MonoBehaviour, IMiniGame
{
    [Header("Specific UI")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject buttonPrefab;

    private MiniGameData data;
    private MiniGameBaseClass baseUI;
    private List<Button> generatedButtons = new();
    private Dictionary<int, string> playerAnswers = new(); // key: blank id -> selected option
    private string displayedContent; // copia del content con sustituciones

    // -------------------------
    // IMiniGame
    // -------------------------
    public void Initialize(MiniGameData data, MiniGameBaseClass baseUI)
    {
        this.data = data;
        this.baseUI = baseUI;

        // preparar estructura de respuestas y texto mostrado
        SetupBlanksAndText();

        // limpiar y generar botones de opciones
        ClearOptions();
        GenerateButtons();

        // actualizar UI base (contentText) con el texto inicial
        UpdateBaseContentText();
    }

    public void TearDown()
    {
        // limpiar listeners y objetos generados
        foreach (var b in generatedButtons)
        {
            if (b != null)
            {
                b.onClick.RemoveAllListeners();
                Destroy(b.gameObject);
            }
        }
        generatedButtons.Clear();
        playerAnswers.Clear();
    }

    // -------------------------
    // Setup
    // -------------------------
    private void SetupBlanksAndText()
    {
        playerAnswers.Clear();
        if (data?.blanks != null)
        {
            foreach (var bl in data.blanks)
            {
                playerAnswers[bl.id] = ""; // vacío al inicio
            }
        }

        displayedContent = data?.content ?? "";
    }

    private void ClearOptions()
    {
        if (optionsContainer == null) return;
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
        generatedButtons.Clear();
    }

    private void GenerateButtons()
    {
        if (data == null || data.options == null || buttonPrefab == null || optionsContainer == null) return;

        // mezclar opciones (Fisher-Yates simple)
        List<string> opts = new List<string>(data.options);
        for (int i = 0; i < opts.Count - 1; i++)
        {
            int r = Random.Range(i, opts.Count);
            var tmp = opts[i]; opts[i] = opts[r]; opts[r] = tmp;
        }

        foreach (var option in opts)
        {
            GameObject go = Instantiate(buttonPrefab, optionsContainer);
            Button btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = option;

            // asignar listener
            btn.onClick.RemoveAllListeners();
            string optCopy = option; // closure safe
            btn.onClick.AddListener(() => OnSelected(btn, optCopy));

            generatedButtons.Add(btn);

            // reset visual con helper del baseUI (si está)
            baseUI?.SetButtonColor(btn, Color.white);
        }
    }

    // -------------------------
    // Selección de opción
    // -------------------------
    private void OnSelected(Button btn, string selected)
    {
        // buscar primer hueco vacío (orden por id creciente)
        int targetId = -1;
        foreach (var kv in playerAnswers)
        {
            if (string.IsNullOrEmpty(kv.Value))
            {
                targetId = kv.Key;
                break;
            }
        }

        if (targetId == -1)
        {
            // todos llenos
            baseUI?.ShowError("Todos los huecos ya están completos.");
            return;
        }

        // asignar respuesta y actualizar texto mostrado
        playerAnswers[targetId] = selected;
        ReplacePlaceholderInDisplayedContent(targetId, selected);
        UpdateBaseContentText();

        // chequeo automático si ya están todos rellenados
        if (AllFilled())
        {
            if (AllCorrect())
            {
                baseUI.ReportSuccess();
                // éxito: marcar botón pulsado en verde (feedback breve) y avanzar
                baseUI?.SetButtonColor(btn, Color.green);
                // usar coroutine del baseUI para respetar delays
                if (baseUI != null) baseUI.StartCoroutine(baseUI.NextMiniGameDelayed(0.6f));
            }
            else
            {
                baseUI.ReportFailure();
                // error: parpadeo en rojo en el botón que se ha pulsado
                if (baseUI != null)
                    baseUI.StartCoroutine(baseUI.FlashButtonColor(btn, Color.red));
                baseUI?.ShowError("Hay respuestas incorrectas. Intenta de nuevo.");
                // no bloqueamos, permitimos reintentos: vaciamos solo los huecos incorrectos para que reconozca cambios
                ClearIncorrectAnswers();
                UpdateBaseContentText();
            }
        }
    }

    // -------------------------
    // Manipulación del texto mostrado
    // -------------------------
    private void ReplacePlaceholderInDisplayedContent(int blankId, string chosen)
    {
        string placeholder = $"____{blankId}";

        // Reemplazamos la primera aparición del placeholder por la elección.
        // Para evitar reemplazar otras apariciones idénticas (si existieran),
        // construimos la nueva cadena manualmente.
        int idx = displayedContent.IndexOf(placeholder);
        if (idx >= 0)
        {
            displayedContent = displayedContent.Substring(0, idx) + chosen + displayedContent.Substring(idx + placeholder.Length);
        }
    }

    private void UpdateBaseContentText()
    {
        if (baseUI != null && baseUI.contentText != null)
        {
            // Puedes añadir estilo aquí (por ej color) si quieres:
            // actualmente ya hemos insertado el texto elegido directamente en displayedContent
            baseUI.contentText.text = displayedContent;
        }
    }

    // -------------------------
    // Validación helpers
    // -------------------------
    private bool AllFilled()
    {
        foreach (var kv in playerAnswers)
            if (string.IsNullOrEmpty(kv.Value)) return false;
        return true;
    }

    private bool AllCorrect()
    {
        if (data == null || data.blanks == null) return false;
        foreach (var bl in data.blanks)
        {
            if (!playerAnswers.ContainsKey(bl.id)) return false;
            if (playerAnswers[bl.id] != bl.correct) return false;
        }
        return true;
    }

    // Vacía únicamente las respuestas que están incorrectas (para permitir reintentos)
    private void ClearIncorrectAnswers()
    {
        if (data == null || data.blanks == null) return;
        var wrongIds = new List<int>();
        foreach (var bl in data.blanks)
        {
            if (!playerAnswers.ContainsKey(bl.id)) continue;
            if (playerAnswers[bl.id] != bl.correct)
                wrongIds.Add(bl.id);
        }

        // restaurar placeholders en displayedContent para cada id erróneo
        foreach (int id in wrongIds)
        {
            string placeholder = $"____{id}";

            // Reemplazamos la respuesta actual por el placeholder (la primera aparición)
            // Buscamos la respuesta (playerAnswers[id]) y la sustituimos por el placeholder
            string answer = playerAnswers[id] ?? "";
            int idx = displayedContent.IndexOf(answer);
            if (idx >= 0)
            {
                displayedContent = displayedContent.Substring(0, idx) + placeholder + displayedContent.Substring(idx + answer.Length);
            }

            playerAnswers[id] = ""; // marcar vacío de nuevo
        }
    }
}
