using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameQuizz : MonoBehaviour, IMiniGame
{
    [Header("UI")]
    [SerializeField] private List<Button> optionButtons; // botones presentes en prefab (puede ser variable)

    private MiniGameData data;
    private MiniGameBaseClass baseUI;
    private bool answered = false;

    public void Initialize(MiniGameData data, MiniGameBaseClass baseUI)
    {
        this.data = data;
        this.baseUI = baseUI;
        answered = false;

        // El texto principal (la pregunta) ya lo pone Base UI en data.content.
        // Configurar botones con opciones aleatorias
        SetupOptionsRandomized();
    }

    private void SetupOptionsRandomized()
    {
        // Copia segura de las opciones (evita modificar el original)
        var options = new List<string>(data.options ?? new List<string>());

        // Fisher-Yates shuffle
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = options[i];
            options[i] = options[j];
            options[j] = tmp;
        }

        // Asignar a botones (si hay más botones que opciones, ocultamos extras)
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var btn = optionButtons[i];
            btn.onClick.RemoveAllListeners();

            if (i < options.Count)
            {
                string opt = options[i];
                btn.gameObject.SetActive(true);

                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = opt;

                baseUI.SetButtonColor(btn, Color.white);
                btn.onClick.AddListener(() => OnOptionSelected(btn, opt));
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }

        // Forzar recalculo de layout para evitar problemas en móvil
        Canvas.ForceUpdateCanvases();
        if (optionButtons.Count > 0 && optionButtons[0] != null)
        {
            var parent = optionButtons[0].transform.parent as RectTransform;
            if (parent != null)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
    }

    private void OnOptionSelected(Button btn, string selected)
    {
        if (answered) return;

        if (selected == data.correctAnswer)
        {
            answered = true;
            baseUI.SetButtonColor(btn, Color.green);
            // Reporta a la clase base un acierto, y esta a su vez al manager
            baseUI.ReportSuccess();
            StartCoroutine(baseUI.NextMiniGameDelayed(0.5f));
        }
        else
        {
            // Reporta a la clase base un fallo, y esta a su vez al manager
            baseUI.ReportFailure();
            baseUI.ShowError("Respuesta incorrecta");
            StartCoroutine(baseUI.FlashButtonColor(btn, Color.red));
        }
    }

    public void TearDown()
    {
        foreach (var b in optionButtons)
            if (b != null) b.onClick.RemoveAllListeners();
    }
}

