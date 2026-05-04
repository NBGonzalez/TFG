// MiniGameQuizz.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MiniGameQuizz : MonoBehaviour, IMiniGame
{
    [Header("UI Dinámica")]
    [SerializeField] private Transform optionsContainer; // Arrastra el panel Vertical aquí
    [SerializeField] private GameObject buttonPrefab;    // Arrastra Universal_Button aquí

    private MiniGameData data;
    private MiniGameBaseClass baseUI;
    private bool answered = false;

    // Lista para borrar botones al salir
    private List<GameObject> spawnedButtons = new List<GameObject>();

    public void Initialize(MiniGameData data, MiniGameBaseClass baseUI)
    {
        this.data = data;
        this.baseUI = baseUI;
        answered = false;

        // Limpiar por si acaso
        ClearButtons();

        // Crear botones
        SetupOptionsRandomized();
    }

    private void ClearButtons()
    {
        foreach (var btn in spawnedButtons) Destroy(btn);
        spawnedButtons.Clear();

        // Seguridad extra: borrar hijos directos del contenedor
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);
    }

    private void SetupOptionsRandomized()
    {
        if (data.options == null) return;

        // Copia y mezcla
        var options = new List<string>(data.options);
        // Fisher-Yates
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = options[i]; options[i] = options[j]; options[j] = tmp;
        }

        // Instanciar botones DINÁMICAMENTE
        foreach (string opt in options)
        {
            GameObject go = Instantiate(buttonPrefab, optionsContainer);
            spawnedButtons.Add(go);

            Button btn = go.GetComponent<Button>();
            TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null) txt.text = opt;

            baseUI.SetButtonColor(btn, Color.white);

            // Lambda segura
            string selectedOpt = opt;
            btn.onClick.AddListener(() => OnOptionSelected(btn, selectedOpt));
        }

        // Forzar update del layout (Truco sucio pero útil en móviles)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(optionsContainer.GetComponent<RectTransform>());
    }

    private void OnOptionSelected(Button btn, string selected)
    {
        if (answered) return;

        if (selected == data.correctAnswer)
        {
            answered = true;
            baseUI.SetButtonColor(btn, Color.green);
            baseUI.ReportSuccess();
            baseUI.StartCoroutine(baseUI.NextMiniGameDelayed(0.5f));
        }
        else
        {
            answered = true; // Bloqueamos más clicks
            baseUI.StartCoroutine(FailSequence(btn, selected));
        }
    }

    private IEnumerator FailSequence(Button btn, string selected)
    {
        baseUI.ReportFailure();
        baseUI.ShowError("Respuesta incorrecta");
        yield return baseUI.StartCoroutine(baseUI.FlashButtonColor(btn, Color.red, 0.5f));
        baseUI.TriggerFailurePopup(data.content, selected, data.correctAnswer);
    }

    public void TearDown()
    {
        ClearButtons();
    }
}
