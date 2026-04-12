using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    [Header("Quizz Inputs (Arrastra aquí los 4 inputs y el dropdown)")]
    public TMP_InputField[] quizzOptionsInputs; // Tamaño 4
    public TMP_Dropdown quizzCorrectDropdown;

    [Header("Arrows Inputs (Arrastra 3 de Izquierda y 3 de Derecha)")]
    public TMP_InputField[] arrowLeftInputs;    // Tamaño 3
    public TMP_InputField[] arrowRightInputs;   // Tamaño 3

    [Header("Fill Blanks Inputs")]
    public TMP_InputField blank1CorrectInput;
    public TMP_InputField blank2CorrectInput;
    public TMP_InputField[] fillBlankOptionsInputs; // Tamaño 4

    [Header("Controls")]
    public Button saveButton;
    public Button cancelButton;

    private ItineraryMiniGameContainerUI minijuegoEnEdicion = null;

    private void Start()
    {
        typeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(ClosePopup);
    }

    private void OnDropdownValueChanged(int index)
    {
        quizzContainer.SetActive(false);
        arrowsContainer.SetActive(false);
        fillBlanksContainer.SetActive(false);

        string selectedType = typeDropdown.options[index].text;
        switch (selectedType)
        {
            case "Quizz": quizzContainer.SetActive(true); break;
            case "Arrows": arrowsContainer.SetActive(true); break;
            case "FillBlanks": fillBlanksContainer.SetActive(true); break;
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

        // Buscar el dropdown correcto
        for (int i = 0; i < typeDropdown.options.Count; i++)
        {
            if (typeDropdown.options[i].text == data.type)
            {
                typeDropdown.value = i;
                break;
            }
        }

        // 2. Rellenar específicos según el tipo
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
            if (data.blanks != null)
            {
                if (data.blanks.Count > 0) blank1CorrectInput.text = data.blanks[0].correct;
                if (data.blanks.Count > 1) blank2CorrectInput.text = data.blanks[1].correct;
            }
            if (data.options != null)
            {
                for (int i = 0; i < 4 && i < data.options.Count; i++)
                {
                    fillBlankOptionsInputs[i].text = data.options[i];
                }
            }
        }

        gameObject.SetActive(true);
    }

    private void OnSaveClicked()
    {
        if (string.IsNullOrWhiteSpace(titleInput.text)) return;

        // ¡CREAMOS EL PAQUETE DE DATOS!
        MiniGameData nuevoData = new MiniGameData();
        nuevoData.type = typeDropdown.options[typeDropdown.value].text;
        nuevoData.title = titleInput.text;
        nuevoData.content = contentInput.text;

        // Guardamos cosas según el tipo
        if (nuevoData.type == "Quizz")
        {
            nuevoData.options = new List<string>();
            foreach (var input in quizzOptionsInputs) nuevoData.options.Add(input.text);
            nuevoData.correctAnswer = quizzOptionsInputs[quizzCorrectDropdown.value].text;
        }
        else if (nuevoData.type == "Arrows")
        {
            nuevoData.pairs = new List<PairData>();
            // Usamos .Length para no tener límites rígidos
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
            nuevoData.blanks = new List<FillBlankEntry>();
            if (!string.IsNullOrEmpty(blank1CorrectInput.text)) nuevoData.blanks.Add(new FillBlankEntry { id = 1, correct = blank1CorrectInput.text });
            if (!string.IsNullOrEmpty(blank2CorrectInput.text)) nuevoData.blanks.Add(new FillBlankEntry { id = 2, correct = blank2CorrectInput.text });

            nuevoData.options = new List<string>();
            foreach (var input in fillBlankOptionsInputs)
            {
                if (!string.IsNullOrEmpty(input.text)) nuevoData.options.Add(input.text);
            }
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

    public void ClosePopup() { gameObject.SetActive(false); }

    private void LimpiarTodosLosCampos()
    {
        // Limpiamos los básicos si existen
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

        if (blank1CorrectInput != null) blank1CorrectInput.text = "";
        if (blank2CorrectInput != null) blank2CorrectInput.text = "";

        if (fillBlankOptionsInputs != null)
        {
            foreach (var input in fillBlankOptionsInputs)
            {
                if (input != null) input.text = "";
            }
        }
    }
}