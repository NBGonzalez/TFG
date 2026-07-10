using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MinigameCreatorPopup : MonoBehaviour
{
    [Header("Core Fields (Always Visible)")]
    public TMP_Dropdown typeDropdown;
    public TMP_InputField titleInput;
    public TMP_InputField contentInput;

    [Header("Dynamic Containers")]
    public GameObject quizzContainer;
    public GameObject arrowsContainer;
    public GameObject fillBlanksContainer;

    [Header("Quizz Inputs (Arrastra aqu� los 4 inputs y el dropdown)")]
    public TMP_InputField[] quizzOptionsInputs; // Tama�o 4
    public TMP_Dropdown quizzCorrectDropdown;

    [Header("Arrows Inputs (Arrastra 3 de Izquierda y 3 de Derecha)")]
    public TMP_InputField[] arrowLeftInputs;    // Tama�o 3
    public TMP_InputField[] arrowRightInputs;   // Tama�o 3

    [Header("Fill Blanks")]
    [Tooltip("Frase con [corchetes] alrededor de las palabras-hueco. Maximo 2 huecos.")]
    public TMP_InputField fillBlanksSentenceInput;
    [Tooltip("Texto en rojo donde se muestran los errores de validacion de FillBlanks.")]
    public TMP_Text fillBlanksErrorText;
    [Tooltip("4 palabras incorrectas (distractores). Las correctas se anaden solas.")]
    public TMP_InputField[] fillBlankOptionsInputs; // Tamano 4

    [Header("Controls")]
    public Button saveButton;
    public Button cancelButton;

    private ItineraryMiniGameContainerUI minijuegoEnEdicion = null;

    private void Start()
    {
        typeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(ClosePopup);

        // Validacion en vivo de la frase de FillBlanks (muestra los errores en rojo segun escribe).
        if (fillBlanksSentenceInput != null)
            fillBlanksSentenceInput.onValueChanged.AddListener(_ => ValidateFillBlanksLive());
    }

    // Traduce la ETIQUETA visible del dropdown (en espanol o ingles) al codigo canonico
    // que entiende el motor. Es la unica tabla de etiquetas: la usan el switch de containers,
    // el guardado y la edicion, asi que nunca se desincronizan.
    private string GetCanonicalType(string label) => label switch
    {
        "Explicación" or "Explicacion" or "Explain" => "Explain",
        "Quizz" or "Quiz" => "Quizz",
        "Arrows" or "Unir columnas" => "Arrows",
        "FillBlanks" or "Rellenar huecos" => "FillBlanks",
        _ => label
    };

    private void OnDropdownValueChanged(int index)
    {
        quizzContainer.SetActive(false);
        arrowsContainer.SetActive(false);
        fillBlanksContainer.SetActive(false);

        string code = GetCanonicalType(typeDropdown.options[index].text);
        switch (code)
        {
            case "Quizz": quizzContainer.SetActive(true); break;
            case "Arrows": arrowsContainer.SetActive(true); break;
            case "FillBlanks": fillBlanksContainer.SetActive(true); break;
            // "Explain" no tiene contenedor propio (solo title + content).
        }
        Canvas.ForceUpdateCanvases();
    }

    public void OpenPopupForCreate()
    {
        minijuegoEnEdicion = null;
        LimpiarTodosLosCampos();
        typeDropdown.value = 0;
        gameObject.SetActive(true);
    }

    public void OpenPopupForEdit(ItineraryMiniGameContainerUI minijuego)
    {
        minijuegoEnEdicion = minijuego;
        LimpiarTodosLosCampos();

        // 1. Rellenar los fijos
        MiniGameData data = minijuego.miData;
        titleInput.text = data.title;
        contentInput.text = data.content;

        // Buscar el dropdown correcto comparando por tipo canonico
        // (la etiqueta puede estar en espanol, pero data.type es "Explain"/"Quizz"/...).
        for (int i = 0; i < typeDropdown.options.Count; i++)
        {
            if (GetCanonicalType(typeDropdown.options[i].text) == data.type)
            {
                typeDropdown.value = i;
                break;
            }
        }

        // 2. Rellenar espec�ficos seg�n el tipo
        if (data.type == "Quizz" && data.options != null)
        {
            for (int i = 0; i < 4 && i < data.options.Count; i++)
            {
                quizzOptionsInputs[i].text = data.options[i];
                if (data.options[i] == data.correctAnswer) quizzCorrectDropdown.value = i;
            }
        }
        else if (data.type == "Arrows" && data.pairs != null)
        {
            for (int i = 0; i < arrowLeftInputs.Length && i < data.pairs.Count; i++)
            {
                arrowLeftInputs[i].text = data.pairs[i].left;
                arrowRightInputs[i].text = data.pairs[i].right;
            }
        }
        else if (data.type == "FillBlanks")
        {
            // Reconstruimos la frase con corchetes a partir de content + blanks:
            // "Las ____1 ... ____2." -> "Las [rocas] ... [minerales]."
            string sentence = data.content ?? "";
            var correctSet = new HashSet<string>();
            if (data.blanks != null)
            {
                foreach (var bl in data.blanks)
                {
                    sentence = sentence.Replace($"____{bl.id}", $"[{bl.correct}]");
                    correctSet.Add(bl.correct);
                }
            }
            if (fillBlanksSentenceInput != null) fillBlanksSentenceInput.text = sentence;

            // Los distractores son las opciones que NO son respuestas correctas.
            if (fillBlankOptionsInputs != null && data.options != null)
            {
                int di = 0;
                foreach (var opt in data.options)
                {
                    if (di >= fillBlankOptionsInputs.Length) break;
                    if (correctSet.Contains(opt)) continue;
                    if (fillBlankOptionsInputs[di] != null) fillBlankOptionsInputs[di].text = opt;
                    di++;
                }
            }

            ValidateFillBlanksLive();
        }

        gameObject.SetActive(true);
    }

    private void OnSaveClicked()
    {
        if (string.IsNullOrWhiteSpace(titleInput.text)) return;

        // �CREAMOS EL PAQUETE DE DATOS!
        MiniGameData nuevoData = new MiniGameData();
        nuevoData.type = GetCanonicalType(typeDropdown.options[typeDropdown.value].text);
        nuevoData.title = titleInput.text;
        nuevoData.content = contentInput.text;

        // Guardamos cosas seg�n el tipo
        if (nuevoData.type == "Quizz")
        {
            nuevoData.options = new List<string>();
            foreach (var input in quizzOptionsInputs) nuevoData.options.Add(input.text);
            nuevoData.correctAnswer = quizzOptionsInputs[quizzCorrectDropdown.value].text;
        }
        else if (nuevoData.type == "Arrows")
        {
            nuevoData.pairs = new List<PairData>();
            // Usamos .Length para no tener l�mites r�gidos
            for (int i = 0; i < arrowLeftInputs.Length; i++)
            {
                // Comprobamos que el jugador ha escrito algo tanto en la izquierda como en la derecha
                if (!string.IsNullOrEmpty(arrowLeftInputs[i].text) && !string.IsNullOrEmpty(arrowRightInputs[i].text))
                {
                    PairData par = new PairData { left = arrowLeftInputs[i].text, right = arrowRightInputs[i].text };
                    nuevoData.pairs.Add(par);
                }
            }
        }
        else if (nuevoData.type == "FillBlanks")
        {
            var distractores = new List<string>();
            if (fillBlankOptionsInputs != null)
                foreach (var input in fillBlankOptionsInputs)
                    if (input != null) distractores.Add(input.text);

            string sentence = fillBlanksSentenceInput != null ? fillBlanksSentenceInput.text : "";
            if (!TryBuildFillBlanks(sentence, distractores,
                                    out string fbContent, out var fbBlanks, out var fbOptions, out string fbError))
            {
                // Frase invalida: mostramos el error en rojo y abortamos.
                // No creamos un minijuego roto ni cerramos el popup.
                if (fillBlanksErrorText != null) fillBlanksErrorText.text = fbError;
                return;
            }

            nuevoData.content = fbContent;
            nuevoData.blanks = fbBlanks;
            nuevoData.options = fbOptions;
        }

        // Enviamos el paquete completo al Manager
        if (minijuegoEnEdicion == null)
        {
            ItineraryCreatorManager.Instance.CrearNuevoMinijuegoVisual(nuevoData);
        }
        else
        {
            minijuegoEnEdicion.ConfigurarTarjeta(nuevoData);
        }

        ClosePopup();
    }

    // =========================================
    //  FILL BLANKS: parser y validacion
    // =========================================

    /// <summary>
    /// Convierte una frase con [corchetes] en content (con ____1/____2), blanks y options.
    /// Las palabras dentro de [] son las correctas (se anaden solas a options); el resto de
    /// options son los distractores. Devuelve false + mensaje de error si la frase no es valida
    /// (corchetes mal cerrados, sin huecos, hueco vacio, o mas de 2 huecos).
    /// </summary>
    private bool TryBuildFillBlanks(string sentence, List<string> distractores,
        out string content, out List<FillBlankEntry> blanks, out List<string> options, out string error)
    {
        content = sentence ?? "";
        blanks = new List<FillBlankEntry>();
        options = new List<string>();
        error = null;

        // 1. Corchetes balanceados.
        int abiertos = 0;
        foreach (char c in content)
        {
            if (c == '[') abiertos++;
            else if (c == ']')
            {
                if (abiertos == 0) { error = "Hay un ] sin su [ correspondiente."; return false; }
                abiertos--;
            }
        }
        if (abiertos != 0) { error = "Falta cerrar un corchete ]."; return false; }

        // 2. Detectar los huecos en orden de aparicion.
        MatchCollection matches = Regex.Matches(content, @"\[(.*?)\]");
        if (matches.Count == 0) { error = "Marca al menos una palabra con [corchetes]."; return false; }
        if (matches.Count > 2) { error = "Solo puedes poner dos palabras entre corchetes como maximo."; return false; }

        // 3. Construir content (sustituyendo [x] por ____id) y rellenar blanks/options.
        var sb = new System.Text.StringBuilder();
        int last = 0;
        int id = 1;
        foreach (Match m in matches)
        {
            sb.Append(content, last, m.Index - last);

            string correct = m.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(correct)) { error = "Hay un hueco vacio [ ]."; return false; }

            sb.Append($"____{id}");
            blanks.Add(new FillBlankEntry { id = id, correct = correct });
            options.Add(correct);

            last = m.Index + m.Length;
            id++;
        }
        sb.Append(content, last, content.Length - last);
        content = sb.ToString();

        // 4. Anadir los distractores no vacios, sin duplicar las correctas ni entre si.
        if (distractores != null)
        {
            foreach (var d in distractores)
            {
                string w = (d ?? "").Trim();
                if (!string.IsNullOrEmpty(w) && !options.Contains(w)) options.Add(w);
            }
        }

        return true;
    }

    /// <summary>
    /// Valida la frase mientras el usuario escribe y muestra el error en rojo (o lo limpia si va bien).
    /// </summary>
    private void ValidateFillBlanksLive()
    {
        if (fillBlanksErrorText == null) return;

        string sentence = fillBlanksSentenceInput != null ? fillBlanksSentenceInput.text : "";
        if (string.IsNullOrEmpty(sentence))
        {
            fillBlanksErrorText.text = "";
            return;
        }

        bool ok = TryBuildFillBlanks(sentence, null, out _, out _, out _, out string error);
        fillBlanksErrorText.text = ok ? "" : error;
    }

    public void ClosePopup() { gameObject.SetActive(false); }

    private void LimpiarTodosLosCampos()
    {
        // Limpiamos los b�sicos si existen
        if (titleInput != null) titleInput.text = "";
        if (contentInput != null) contentInput.text = "";

        // Limpiamos los Arrays SOLO si los hemos arrastrado en Unity
        if (quizzOptionsInputs != null)
        {
            foreach (var input in quizzOptionsInputs)
            {
                if (input != null) input.text = "";
            }
        }

        if (arrowLeftInputs != null)
        {
            foreach (var input in arrowLeftInputs)
            {
                if (input != null) input.text = "";
            }
        }

        if (arrowRightInputs != null)
        {
            foreach (var input in arrowRightInputs)
            {
                if (input != null) input.text = "";
            }
        }

        if (fillBlanksSentenceInput != null) fillBlanksSentenceInput.text = "";
        if (fillBlanksErrorText != null) fillBlanksErrorText.text = "";

        if (fillBlankOptionsInputs != null)
        {
            foreach (var input in fillBlankOptionsInputs)
            {
                if (input != null) input.text = "";
            }
        }
    }
}